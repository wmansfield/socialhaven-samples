using Haven.SDK;
using Haven.SDK.Models;
using Haven.SDK.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// The system hierarchy is as follows:
// An 'Account' is a 'Member' of a 'Principal' and the 'Principal' is a 'Seat' inside of a 'Term'
// Custom naming makes this confusing, but consider these naming variations:
// "my@email.com" was invited to use an "Account" that is "part of" the "Advanced Tier"
// "my@email.com" was invited to manage a "Student" that is "part of" the "Primary Class"
// "my@email.com" was added to the "Subscription" that "grants access" to the "Advanced Course"

namespace ThirdPartyAutomation
{
    class Program
    {
        private static AccountInfo _account = new AccountInfo() // typically acquired by logging in, sdk.Auth.login
        {
            api_key = "",
            api_secret = "",
            account_id = new Guid("")
        };

        private static Guid? _sampleFactionID = new Guid("");
        private static string _apiUrl = "https://app.socialhaven.com/api/";


        private static SampleConsumer _consumer;

        static void Main(string[] args)
        {
            _consumer = new SampleConsumer(_apiUrl, _account);
            ConnectionSample().Wait();
            SDKPattern().Wait();

            //UseCases_Bulletin().Wait();
            //UseCases_Form().Wait();
            //UseCases_Conversations().Wait();
            //UseCases_Push().Wait();
            //UseCases_Groups().Wait();
            //UseCases_Terms().Wait();
            //UseCases_Principals().Wait();

            Console.WriteLine("Press any key to close..");
            Console.ReadKey();
        }

        static async Task ConnectionSample()
        {
            try
            {
                // All requests are in an envelope of ItemResult, ListResult, or ActionResult, use .item or .items to retrieve data
                ItemResult<AccountInfo> accountResponse = await _consumer.GetSelf();
                Console.WriteLine("I am: {0}", accountResponse?.item?.email);

                // The same api call can be unwrapped if the caller is not interested in managing at that level
                AccountInfo account = await _consumer.GetSelf_Unwrapped();
                Console.WriteLine("I am: {0}", account?.email);
            }
            catch (Exception ex)
            {
                //Exceptions can happen, be sure you wrap them.
                Console.WriteLine(ex.FirstNonAggregateException().Message);
            }
            
        }

        static async Task SDKPattern()
        {
            Console.WriteLine("Look into the source for DemonstratePattern for more info");
            FeedItem feedItem = await _consumer.DemonstratePattern(_sampleFactionID.GetValueOrDefault());
            Console.WriteLine("First Feed Item: {0}", feedItem.post.body);
        }

        static async Task UseCases_Bulletin()
        {
            Bulletin bulletin = await _consumer.Bulletin_Create(_sampleFactionID.GetValueOrDefault());
            Console.WriteLine("Created Item: {0}", bulletin.bulletin_id);

            bulletin = await _consumer.Bulletin_Create_WithForm(_sampleFactionID.GetValueOrDefault());
            Console.WriteLine("Created Item: {0}", bulletin.bulletin_id);

            bulletin = await _consumer.Bulletin_Create_With_Target(_sampleFactionID.GetValueOrDefault());
            Console.WriteLine("Created Item: {0}", bulletin.bulletin_id);

            bulletin = await _consumer.Bulletin_Edit(_sampleFactionID.GetValueOrDefault(), bulletin.bulletin_id);
            Console.WriteLine("Edited Item: {0}", bulletin.bulletin_id);
        }

        static async Task UseCases_Form()
        {
            // this flow submits answers so we need to mimic user log in, using Self() acquires updated account info
            AccountInfo account = await _consumer.GetSelf().DemoUnPack();
            _consumer = new SampleConsumer(_apiUrl, account);


            Form form = await _consumer.Form_Create(_sampleFactionID.GetValueOrDefault());
            Console.WriteLine("Created Item: {0}", form.form_id);

            form = await _consumer.Form_Create_With_Target(_sampleFactionID.GetValueOrDefault());
            Console.WriteLine("Created Item: {0}", form.form_id);

            form = await _consumer.Form_Edit(_sampleFactionID.GetValueOrDefault(), form.form_id);
            Console.WriteLine("Edited Item: {0}", form.form_id);
            
            bool success = await _consumer.Form_Submit(_sampleFactionID.GetValueOrDefault(), form.form_id);
            Console.WriteLine("Submitted Item: {0}", success);

            string filePath = await _consumer.Form_GetAnswers(_sampleFactionID.GetValueOrDefault(), form.form_id);
            Console.WriteLine("Downloaded to: {0}", filePath);
        }

        static async Task UseCases_Conversations()
        {
            List<AccountSimple> people = await _consumer.Convo_FindTarget(_sampleFactionID.GetValueOrDefault());

            Conversation conversation = await _consumer.Convo_Start(_sampleFactionID.GetValueOrDefault(), people.FirstOrDefault(x => x.account_id != _account.account_id).account_id);
            Console.WriteLine("Created Conversation: {0}", conversation.conversation_id);
        }

        static async Task UseCases_Push()
        {
            bool success = await _consumer.Push_SendGeneric(_sampleFactionID.GetValueOrDefault(), _account.account_id);
            Console.WriteLine("Send Push: {0}", success);
        }


        static async Task UseCases_Groups()
        {
            // this flow requires knowing principals, for demo we just login and use our own
            AccountInfo account = await _consumer.GetSelf().DemoUnPack();

            List<Group> groups = await _consumer.GroupsGet(_sampleFactionID.GetValueOrDefault(), 0, 1);
            Guid group_id = groups.FirstOrDefault().group_id;

            Guid principal_id = account.factions_member.FirstOrDefault(x => x.principal_id != null).principal_id.GetValueOrDefault();

            // Create User Token
            bool added = await _consumer.Principal_Group_Add(_sampleFactionID.GetValueOrDefault(), principal_id, group_id);
            Console.WriteLine("Added to Group: {0}", added);

            // now remove it 
            bool removed = await _consumer.Principal_Group_Remove(_sampleFactionID.GetValueOrDefault(), principal_id, group_id);
            Console.WriteLine("Removed from Group: {0}", removed);

        }

        static async Task UseCases_Terms()
        {
            // this flow requires knowing principals, for demo we just login and use our own
            AccountInfo account = await _consumer.GetSelf().DemoUnPack();

            List<Term> terms = await _consumer.TermsGet(_sampleFactionID.GetValueOrDefault(), 0, 100);
            Guid term_id = terms.LastOrDefault().term_id;

            Guid principal_id = account.factions_member.FirstOrDefault(x => x.principal_id != null).principal_id.GetValueOrDefault();


            // Create User Token
            Seat seat = await _consumer.Principal_Seat_Add(_sampleFactionID.GetValueOrDefault(), principal_id, term_id);
            Console.WriteLine("Added Seat: {0}", seat.seat_id);

            // now remove it 
            bool removed = await _consumer.Principal_Seat_Remove(_sampleFactionID.GetValueOrDefault(), seat.seat_id);
            Console.WriteLine("Removed from Seat: {0}", removed);

        }

        static async Task UseCases_Principals()
        {
            List<Term> terms = await _consumer.TermsGet(_sampleFactionID.GetValueOrDefault(), 0, 100);
            Guid term_id = terms.LastOrDefault().term_id;

            Principal principal = await _consumer.Principal_Register(_sampleFactionID.GetValueOrDefault(), term_id, "wmansfield@socialhaven.com");
            Console.WriteLine("Created Principal: {0}", principal.principal_id);

            bool invited = await _consumer.Principal_Invite(_sampleFactionID.GetValueOrDefault(), principal.principal_id, "contact@socialhaven.com");
            Console.WriteLine("Invited to Principal: {0}", invited);

            principal = await _consumer.Principal_Edit(_sampleFactionID.GetValueOrDefault(), principal.principal_id, "Updated Name");
            Console.WriteLine("Updated Principal: {0}", principal.display_name);

            bool changed = await _consumer.Principal_ChangeStatus(_sampleFactionID.GetValueOrDefault(), principal.principal_id, false);
            Console.WriteLine("Updated Status: {0}", changed);

        }

        
    }
}
