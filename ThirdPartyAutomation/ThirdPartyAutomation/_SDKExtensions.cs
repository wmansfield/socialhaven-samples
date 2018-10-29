using Haven.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyAutomation
{
    public static class _SDKExtensions
    {
        /// <summary>
        /// Should not use this in production, only for quick demonstration
        /// </summary>
        public static async Task<TModel> DemoUnPack<TModel>(this Task<ItemResult<TModel>> task)
        {
            ItemResult<TModel> result = await task;
            if(result.IsSuccess())
            {
                return result.item;
            }
            return default(TModel);
        }
        /// <summary>
        /// Should not use this in production, only for quick demonstration
        /// </summary>
        public static async Task<List<TModel>> DemoUnPack<TModel>(this Task<ListResult<TModel>> task)
        {
            ListResult<TModel> result = await task;
            if (result.IsSuccess())
            {
                return result.items;
            }
            return new List<TModel>();
        }

        public static Exception FirstNonAggregateException(this Exception ex)
        {
            AggregateException aggregate = ex as AggregateException;
            if (aggregate != null)
            {
                foreach (var item in aggregate.InnerExceptions)
                {
                    return item.FirstNonAggregateException();
                }
            }

            return ex;
        }
        public static void Replace(this List<KeyValuePair<string, string>> list, string key, string value)
        {
            if (list != null)
            {
                int found = list.FindIndex(x => x.Key == key);
                if (found >= 0)
                {
                    if (list[found].Value == value)
                    {
                        return; // Short Circuit, the same
                    }
                    list.RemoveAt(found);
                }
                list.Add(new KeyValuePair<string, string>(key, value));
            }

        }

        public static void Remove(this List<KeyValuePair<string, string>> list, string key)
        {
            if (list != null)
            {
                int found = list.FindIndex(x => x.Key == key);
                if (found >= 0)
                {
                    list.RemoveAt(found);
                }
            }
        }
    }
}
