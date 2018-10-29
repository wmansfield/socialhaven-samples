using Haven.SDK;
using Haven.SDK.Models;
using Haven.SDK.Models.Requests;
using Haven.SDK.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyAutomation
{
    public class SampleConsumer
    {
        #region Constructor

        public SampleConsumer(string apiServer, AccountInfo account)
        {
            _apiServer = apiServer;
            _account = account;
        }

        #endregion

        #region Private Properties

        private AccountInfo _account;
        private string _apiServer;

        private HavenSDK _havenAnonymous;
        private HavenSDK _havenAuthenticated;

        #endregion

        #region Sample Login Methods

        public async Task<AccountInfo> Login(string user, string password)
        {
            HavenSDK sdk = this.GetHavenSDK(true, null);

            AuthLoginInput input = new AuthLoginInput()
            {
                user = user,
                password = password
            };

            AccountInfo result = await sdk.Auth.LoginAsync(input).DemoUnPack();

            if(result != null)
            {
                _account = result;
            }
            return result;
        }
        public async Task<AccountInfo> LoginAuto(string code)
        {
            HavenSDK sdk = this.GetHavenSDK(true, null);

            RedeemInput input = new RedeemInput()
            {
                code = code,
            };

            RedeemResponse response = await sdk.Auth.VerifyAccessCodeAsync(input).DemoUnPack();

            if(response.next_screen == RedeemScreen.AutoLogin)
            {
                _account = response.account_info;
            }
            return _account;
        }

        public Task<ItemResult<AccountInfo>> GetSelf()
        {
            HavenSDK sdk = this.GetHavenSDK(null);
            return sdk.Accounts.GetSelfAsync();
        }
        public async Task<AccountInfo> GetSelf_Unwrapped()
        {
            // typically, if you unwrap it at this layer, you may want to trap exceptions as well [this code obviously doesnt]
            HavenSDK sdk = this.GetHavenSDK(null);
            ItemResult<AccountInfo> response = await sdk.Accounts.GetSelfAsync();
            if(response.IsSuccess())
            {
                return response.item;
            }
            return null;
        }

        #endregion

        #region Naming Convention Methods

        public async Task<FeedItem> DemonstratePattern(Guid faction_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            // the SDK has a property for every entity it supports
            // however, most entities have a pluralized property as well
            // the pluralized version typically has more efficient and/or preferred API calls to perform operations

            // Notice this is:    .FeedItem.
            ListResult<FeedItem> response = await sdk.FeedItem.GetFeedItemByFactionAsync(faction_id, 0, 10);
            // the response will contain the items for the feed, however, it does not have the expected business logic applied
            // this version is useful for admin level items perhaps?
            //response.items;


            // Notice this is:    .FeedItems.
            response = await sdk.FeedItems.GetFeedItemForAccountAsync(faction_id, 0, 10);
            // this response will not contain feed items that the CURRENT USER can see.

            return response.items.FirstOrDefault();
        }

        #endregion


        #region Use Case Account

        public async Task<string> Account_CreateLoginToken(Guid faction_id, Guid account_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            return await sdk.Factions.GenerateLoginToken(faction_id, new Haven.SDK.Models.Requests.LoginTokenInput()
            {
                account_id = account_id,
                expire_minutes = 5
            }).DemoUnPack();
        }


        public async Task<Account> Account_Get(Guid faction_id, Guid account_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            Account result = await sdk.Account.GetAccountAsync(account_id).DemoUnPack();
            return result;
        }
        public async Task<List<Account>> Account_Find(Guid faction_id, string keyword)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            List<Account> result = await sdk.Account.Find(0, 10, keyword).DemoUnPack();
            return result;
        }
        #endregion

        #region Use Case Bulletin

        public async Task<Bulletin> Bulletin_Create(Guid faction_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            List<BulletinCategory> categories = await sdk.BulletinCategories.GetBulletinCategoryForFactionAsync(faction_id, 0, 1).DemoUnPack();
            Guid bulletin_category_id = categories.FirstOrDefault().bulletin_category_id;

            Bulletin bulletin = this.CreateBulletinInstance(faction_id, bulletin_category_id);

            bulletin = await sdk.Bulletin.CreateBulletinAsync(bulletin).DemoUnPack();
            return bulletin;
        }
        public async Task<Bulletin> Bulletin_Create_WithForm(Guid faction_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            List<BulletinCategory> categories = await sdk.BulletinCategories.GetBulletinCategoryForFactionAsync(faction_id, 0, 1).DemoUnPack();
            Guid bulletin_category_id = categories.FirstOrDefault().bulletin_category_id;

            List<FormConfig> formConfigs = await sdk.FormConfig.GetFormConfigByFactionAsync(faction_id, 0, 1, flow: FormFlowType.OnDemand, purpose: FormIntent.Generated).DemoUnPack();
            Guid form_config_id = formConfigs.FirstOrDefault().form_config_id;

            Bulletin bulletin = this.CreateBulletinInstance(faction_id, bulletin_category_id);
            bulletin.title = "This has a form attached!";
            bulletin.cta_form_config_id = form_config_id;

            bulletin = await sdk.Bulletin.CreateBulletinAsync(bulletin).DemoUnPack();
            return bulletin;

        }
        public async Task<Bulletin> Bulletin_Create_With_Target(Guid faction_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            List<BulletinCategory> categories = await sdk.BulletinCategories.GetBulletinCategoryForFactionAsync(faction_id, 0, 1).DemoUnPack();
            Guid bulletin_category_id = categories.FirstOrDefault().bulletin_category_id;

            List<Term> terms = await sdk.Terms.GetActiveTermByFactionAsync(faction_id, 0, 1).DemoUnPack();
            Guid term_id = terms.FirstOrDefault().term_id;

            Bulletin bulletin = this.CreateBulletinInstance(faction_id, bulletin_category_id);

            bulletin.title = "This is targeted to a term";
            bulletin.scope = BulletinScope.Term;
            bulletin.term_id = term_id;

            // By Group
            //bulletin.scope = BulletinScope.Group;
            //bulletin.group_id = group_id;

            // by Principal
            //bulletin.scope = BulletinScope.Principal;
            //bulletin.principal_id = principal_id;

            bulletin = await sdk.Bulletin.CreateBulletinAsync(bulletin).DemoUnPack();
            return bulletin;
        }

        public async Task<Bulletin> Bulletin_Edit(Guid faction_id, Guid bulletin_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            Bulletin bulletin = await sdk.Bulletin.GetBulletinAsync(bulletin_id).DemoUnPack();
            bulletin.title = string.Format("I was updated at {0}", DateTime.UtcNow);
            bulletin = await sdk.Bulletin.UpdateBulletinAsync(bulletin.bulletin_id, bulletin).DemoUnPack();
            return bulletin;
        }

        private Bulletin CreateBulletinInstance(Guid faction_id, Guid bulletin_category_id)
        {
            return new Bulletin()
            {
                faction_id = faction_id,
                account_id_owner = _account.account_id,
                bulletin_category_id = bulletin_category_id,
                scope = BulletinScope.Faction, // available to everyone
                stack_tab = -1, // no stack configured
                title = "My Bulletin Title",
                summary = "My bulletin summary",
                disable_push = true, // typically this is false, but for demo, this is useful
                active = true,
                sections = new List<BulletinSection>()
                {
                    new BulletinSection()
                    {
                        kind = BulletinSectionKind.header,
                        text = "Hello Header"
                    },
                    new BulletinSection()
                    {
                        kind = BulletinSectionKind.text,
                        text = "this is general text for a bulletin"
                    }
                },
            };
        }

        #endregion

        #region Use Case Form

        public async Task<Form> Form_Create(Guid faction_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            Form form = this.CreateFormInstance(faction_id);

            form = await sdk.Forms.UpsertFormAsync(form).DemoUnPack();
            return form;
        }
        
        public async Task<Form> Form_Create_With_Target(Guid faction_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            List<Group> groups = await sdk.Group.GetGroupByFactionAsync(faction_id, 0, 1).DemoUnPack();
            Guid group_id = groups.FirstOrDefault().group_id;

            Form form = this.CreateFormInstance(faction_id);

            form.title = "This is targeted to a group";
            form.scope = FormScope.Group;
            form.group_id = group_id;

            // by Principal
            //form.scope = FormScope.Principal;
            //form.principal_id = principal_id;

            // by Term
            //form.scope = FormScope.Term;
            //form.term_id = term_id;

            form = await sdk.Forms.UpsertFormAsync(form).DemoUnPack();
            return form;
        }

        public async Task<Form> Form_Edit(Guid faction_id, Guid form_id)
        {
            // NOTE: Adding new questions to forms is supported, however, only NEW people assigned to the form will be able to fill them out.
            // If the form is a ExecutionType.Live, they can go in and edit the form and provide the new values.
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            Form form = await sdk.Form.GetFormAsync(form_id).DemoUnPack();
            form.title = string.Format("I was updated at {0}", DateTime.UtcNow);

            form.sections.Add(new FormSection()
            { 
                kind = FormSectionKind.form_question,
                question = new FormQuestion()
                {
                    title = "This is a new question",
                    config = new FormOptionConfig()
                    {
                        kind = FormOptionKind.Text,
                        code = "new",
                        profile = null
                    }
                }
            });

            form = await sdk.Forms.UpsertFormAsync(form).DemoUnPack();
            return form;
        }

        public async Task<bool> Form_Submit(Guid faction_id, Guid form_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            FactionPublic faction = _account.factions_member.FirstOrDefault(x => x.faction_id == faction_id);

            List<Form> forms = await sdk.Forms.GetForFactionAccount(faction_id, true, true, null, 0, 20).DemoUnPack();
            Form form = forms.FirstOrDefault(x => x.form_id == form_id);
            if(form == null)
            {
                throw new Exception("Check demonstration code for proper IDs");
            }

            FormResponse response = new FormResponse()
            {
                account_id = _account.account_id, // can be different than current user if admin
                form_id = form.form_id,
                faction_id = form.faction_id,
                member_id = form.attachment == FormAttachmentType.Member ? faction.member_id.GetValueOrDefault() : (Guid?)null, // Required if FormAttachmentType.Member
                principal_id = form.attachment == FormAttachmentType.Principal ? faction.principal_id.GetValueOrDefault() : (Guid?)null, // Required if FormAttachmentType.Principal
            };
            response.response_data = new List<FormResponseData>();
            foreach (FormSection section in form.sections.Where(x => x.kind == FormSectionKind.form_question))
            {
                FormResponseData data = new FormResponseData()
                {
                    form_question_id = section.question.form_question_id
                };
                switch (section.question.config.kind)
                {
                    case FormOptionKind.Text:
                        data.response_raw = "Answered";
                        data.response_display = "Answered";
                        break;
                    case FormOptionKind.Date:
                        data.response_raw = DateTime.UtcNow.ToString();
                        data.response_display = data.response_raw;
                        break;
                    case FormOptionKind.Number:
                        data.response_raw = "134";
                        data.response_display = "134";
                        break;
                    case FormOptionKind.SingleChoice:
                        ValuePair singleOption = section.question.config.options.FirstOrDefault();
                        data.response_raw = singleOption.value;
                        data.response_display = singleOption.name;
                        break;
                    case FormOptionKind.MultipleChoice:
                        ValuePair option = section.question.config.options.FirstOrDefault();
                        data.response_raw = option.value;
                        data.response_display = option.name;
                        break;
                    case FormOptionKind.Email:
                        data.response_raw = "noone@domain.com";
                        data.response_display = "noone@domain.com";
                        break;
                    case FormOptionKind.PhotoUpload:
                        //TODO: Photo Upload Sample
                        // uploadUrl = sdk.Media.GetTemporaryUploadUrl();
                        // upload to URL
                        break;
                    default:
                        break;
                }
                response.response_data.Add(data);
            }
            ActionResult result = await sdk.Forms.SubmitFormAsync(response);
            return result.IsSuccess();
        }

        public async Task<string> Form_GetAnswers(Guid faction_id, Guid form_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            // Get data to self-shape one-by-one
            List<Form> result = await sdk.Forms.GetReport(form_id, null, 0, int.MaxValue).DemoUnPack();
            foreach (var item in result)
            {
                // enumerate item.sections to get the question.answer  (this does the first)
                Console.WriteLine("{0}: {1}", item.form_response.submitted_by, item.sections.FirstOrDefault(x => x.kind == FormSectionKind.form_question).question.answer.response_raw);
            }

            // Download CSV URL
            string url = sdk.Forms.GenerateExportCSV(form_id); // generates url for web-download, expects cookies if using webpage

            // Download CSV
            byte[] data = await sdk.Forms.ExportCSVAsync(form_id);

            string tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName()));
            string tempPath = System.IO.Path.Combine(tempFolder, "export.xlsx");
            System.IO.Directory.CreateDirectory(tempFolder);

            System.IO.File.WriteAllBytes(tempPath, data);
            return tempPath;
        }

        private Form CreateFormInstance(Guid faction_id)
        {
            //Note: Form Config is a UI Feature. Providing the form_config_id does not copy the questions from that config by the sdk.
            return new Form()
            {
                faction_id = faction_id,
                account_id_creator = _account.account_id,
                form_config_id = null,// optional reference to a template
                scope = FormScope.Faction, // available to everyone
                attachment = FormAttachmentType.Principal, // this is critical, one form per principal. (if school scenario:  One form per student, not per parent)
                required = true,
                execution = FormExecutionType.Report,
                flow = FormFlowType.Standard,
                external_identifier = "your-system-id",// if this is enabled, each question can have an id field for easier mapping
                enabled = true,
                title = "My Form Title",
                summary = "My Form summary",
                sections = new List<FormSection>()
                {
                    new FormSection()
                    {
                        kind = FormSectionKind.header,
                        text = "Hello Header"
                    },
                    new FormSection()
                    {
                        kind = FormSectionKind.text,
                        text = "this is general text for a form"
                    },
                    new FormSection()
                    {
                        kind = FormSectionKind.form_question,
                        question = new FormQuestion()
                        {
                            title = "Single Choice Question w/Other",
                            config = new FormOptionConfig()
                            {
                                kind = FormOptionKind.SingleChoice,
                                other = true,
                                other_text = "Provide Other",
                                code = "your-system-id-other", // useful for external mapping
                                options = new List<ValuePair>()
                                {
                                    new ValuePair()
                                    {
                                        name = "First Answer",
                                        value = "first" // export value, can be the same as name or custom
                                    },
                                    new ValuePair()
                                    {
                                        name = "Second Answer",
                                        value = "Second Answer" // export value
                                    }
                                }
                            }
                        }
                    }
                },
            };
        }

        #endregion

        #region Use Case Conversation

        public async Task<List<AccountSimple>> Convo_FindTarget(Guid faction_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            return await sdk.Conversations.FindTarget(faction_id, 10, "").DemoUnPack();
        }

        public async Task<Conversation> Convo_Start(Guid faction_id, Guid account_id_other)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            Conversation conversation = new Conversation()
            {
                faction_id = faction_id,
                creator_account_id = _account.account_id, // can be different than current user if admin
                account_list = new List<Guid>()
                {
                    account_id_other // can be multiple accounts
                },
                latest_message = new Message()
                {
                    faction_id = faction_id,
                    account_id = _account.account_id,
                    stamp_utc = DateTime.UtcNow,
                    text = "Hello party people."
                }
            };

            conversation = await sdk.Conversations.StartConversationAsync(conversation).DemoUnPack();
            return conversation; // NOTE: conversation may not have all avatars and extra info immediately after creation, if making UI elements you should pre-cache
        }


        #endregion

        #region Use Case Push

        public async Task<bool> Push_SendGeneric(Guid faction_id, Guid account_id_test)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            GenericPushInput input = new GenericPushInput()
            {
                message = "Welcome to Social Haven",
                account_ids = new List<Guid>()
                {
                    account_id_test
                }
            };
            ActionResult result = await sdk.Factions.SendGenericPushAsync(faction_id, input);
            return result.IsSuccess();
        }


        #endregion

        #region Use Case Principal

        public async Task<Seat> Principal_Seat_Add(Guid faction_id, Guid principal_id, Guid term_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            List<SeatType> seatTypes = await sdk.SeatType.GetSeatTypeByFactionAsync(faction_id, 0, 1).DemoUnPack();

            Seat seat = new Seat()
            {
                faction_id = faction_id,
                principal_id = principal_id,
                term_id = term_id,
                seat_type_id = seatTypes.FirstOrDefault().seat_type_id,
                added_utc = DateTime.UtcNow,
            };
            Seat result = await sdk.Seat.CreateSeatAsync(seat).DemoUnPack();
            return result;
        }
        public async Task<bool> Principal_Seat_Remove(Guid faction_id, Guid seat_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            ActionResult result = await sdk.Seats.RemoveSeatAsync(seat_id); // sdk.Seat.Delete() is supported, however, use caution, all related data may become invalidated.
            return result.IsSuccess();
        }


        public async Task<bool> Principal_Group_Add(Guid faction_id, Guid principal_id, Guid group_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            // typically this is done per principal, although account is supported, it is not reasonable

            GroupTarget groupTarget = new GroupTarget()
            {
                account_id = _account.account_id,
                group_id = group_id,
                principal_id = principal_id,
                kind = GroupTargetKind.Principal,
                role = GroupMemberRole.Writer, // not fully enforced yet
                hidden = false,
                suppress_main = false
            };
            ActionResult result = await sdk.GroupTargets.UpsertGroupTargetAsync(groupTarget);
            return result.IsSuccess();
        }
        public async Task<bool> Principal_Group_Remove(Guid faction_id, Guid principal_id, Guid group_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            List<GroupTarget> grouptargets = await sdk.GroupTarget.GetGroupTargetByGroupAsync(group_id, 0, int.MaxValue).DemoUnPack();
            GroupTarget groupTarget = grouptargets.FirstOrDefault(x => x.principal_id == principal_id);
            if (groupTarget == null)
            {
                throw new Exception("User is not a part of the group");
            }

            ActionResult result = await sdk.GroupTarget.DeleteGroupTargetAsync(groupTarget.group_target_id);
            return result.IsSuccess();
        }

        public async Task<Principal> Principal_Register(Guid faction_id, Guid term_id, string email)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            List<SeatType> seatTypes = await sdk.SeatType.GetSeatTypeByFactionAsync(faction_id, 0, 1).DemoUnPack();

            PrincipalRegisterInput principal = new PrincipalRegisterInput()
            {
                invite_emails = new List<string>() { email },
                seat = new Seat()
                {
                    faction_id = faction_id,
                    term_id = term_id,
                    seat_type_id = seatTypes.FirstOrDefault().seat_type_id,
                    added_utc = DateTime.UtcNow,
                },
                principal = new Principal()
                {
                    faction_id = faction_id,
                    external_identifier = "my-external",
                    access_code = "", // will be generated
                    display_name = "New Principal",
                    enabled = true,
                    expected_signers = 1, // for document integration
                    full_name = "New Principal",
                    limit = 1, // how many 'Accounts' can be associated with this principal
                }
            };
            Principal result = await sdk.Principals.RegisterPrincipalAsync(principal).DemoUnPack();
            return result;
        }
        public async Task<bool> Principal_Invite(Guid faction_id, Guid principal_id, string email)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            // To invite a user, you must first create a principal then create a principal invite, see Principal_Create for combined version
            PrincipalInvite invite = new PrincipalInvite()
            {
                faction_id = faction_id,
                principal_id = principal_id,
                email = email,
                stamp_utc = DateTime.UtcNow
            };
            ActionResult result = await sdk.PrincipalInvite.CreatePrincipalInviteAsync(invite);
            return result.IsSuccess();
        }
        

        public async Task<Principal> Principal_Edit(Guid faction_id, Guid principal_id, string new_name)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            Principal result = await sdk.Principal.GetPrincipalAsync(principal_id).DemoUnPack();
            result.display_name = new_name;

            result = await sdk.Principal.UpdatePrincipalAsync(result.principal_id, result).DemoUnPack();
            return result;
        }

        public async Task<bool> Principal_ChangeStatus(Guid faction_id, Guid principal_id, bool enabled)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            ActionResult result = await sdk.Principals.ChangeStatusAsync(principal_id, enabled);
            return result.IsSuccess();
        }

        public async Task<Principal> Principal_Get(Guid faction_id, Guid principal_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            Principal result = await sdk.Principal.GetPrincipalAsync(principal_id).DemoUnPack();
            return result;
        }
        public async Task<Principal> Principal_Get(Guid faction_id, string external_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            Principal result = await sdk.Principals.GetPrincipalByExternaIdAsync(faction_id, external_id).DemoUnPack();
            return result;
        }
        public async Task<List<Principal>> Principal_Find(Guid faction_id, string keyword)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            List<Principal> result = await sdk.Principals.FindPrincipalByFactionAsync(faction_id, keyword, 0, 10).DemoUnPack();
            return result;
        }

        public async Task<List<Group>> GroupsGet(Guid faction_id, int skip, int take)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            return await sdk.Group.GetGroupByFactionAsync(faction_id, skip, take).DemoUnPack();
        }
        public async Task<List<Term>> TermsGet(Guid faction_id, int skip, int take)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            return await sdk.Terms.GetActiveTermByFactionAsync(faction_id, skip, take).DemoUnPack();
        }

        #endregion

        #region Use Case Staff

        public async Task<bool> Staff_Invite(Guid faction_id, Guid principal_id, string email)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);

            Invite invite = new Invite()
            {
                faction_id = faction_id,
                email = email,
                code = null, // auto generated
                type = InviteType.Administrator
            };
            ActionResult result = await sdk.Invite.CreateInviteAsync(invite);
            return result.IsSuccess();
        }
        public async Task<bool> Manager_Change(Guid faction_id, Guid manager_id)
        {
            HavenSDK sdk = this.GetHavenSDK(faction_id);
            ActionResult result = await sdk.Managers.ChangeTypeAsync(manager_id, ManagerType.Administrator);
            return result.IsSuccess();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Really only useful for initial login or registration
        /// </summary>
        /// <returns></returns>
        protected virtual HavenSDK GetHavenAnonymousSDK()
        {
            return GetHavenSDK(true, null);
        }
        protected virtual HavenSDK GetHavenSDK(Guid? faction_id)
        {
            return GetHavenSDK(false, faction_id);
        }
        protected virtual HavenSDK GetHavenSDK(bool anonymous, Guid? faction_id)
        {
            HavenSDK result = null;

            if(anonymous)
            {
                if (_havenAnonymous == null)
                {
                    _havenAnonymous = this.CreateHavenSDK(true);
                }
                result = _havenAnonymous;
            }
            else
            {
                if (_havenAuthenticated == null)
                {
                    _havenAuthenticated = this.CreateHavenSDK(false);
                }
                result = _havenAuthenticated;
            }

            // While not required, it is highly recommended for efficient routing
            if (faction_id.HasValue)
            {
                result.CustomHeaders.Replace("X-Faction", faction_id.Value.ToString());
            }
            else
            {
                result.CustomHeaders.Remove("X-Faction");
            }
            return result;
        }
        protected virtual HavenSDK CreateHavenSDK(bool anonymous)
        {
            HavenSDK result = new HavenSDK(_apiServer);

            result.CustomHeaders.Add(new KeyValuePair<string, string>("accept-language", "en-US")); // should send default language if multiple languages are configured
            result.CustomHeaders.Add(new KeyValuePair<string, string>("X-DevicePlatform", "web")); // some requests are custom shaped by platform and version
            result.CustomHeaders.Add(new KeyValuePair<string, string>("X-DeviceVersion", "1.0"));
            //result.CustomHeaders.Add(new KeyValuePair<string, string>("X-DeviceToken", "no-pii-here"));  // (optional) useful for specific tracking in some extended libraries
            //result.CustomHeaders.Add(new KeyValuePair<string, string>("X-Label", "name-from-social-haven")); // (optional) should be supplied if using non-label specific api urls

            if (!anonymous)
            {
                result.ApplicationKey = _account.api_key;
                result.ApplicationSecret = _account.api_secret;
            }
            return result;
        }

        #endregion
    }
}
