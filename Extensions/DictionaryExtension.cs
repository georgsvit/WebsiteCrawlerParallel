using System.Collections.Generic;

namespace WebsiteCrawlerParallel.Extensions
{
    public static class DictionaryExtension
    {
        public static IEnumerable<TKey> CustomConcat<TKey, TValue>(this Dictionary<TKey, TValue> baseDict, Dictionary<TKey, TValue> supplementDict)
        {
            List<TKey> result = new();

            foreach (var item in supplementDict)
                if (baseDict.TryAdd(item.Key, item.Value))
                    result.Add(item.Key);

            return result;
        }
    }
}
