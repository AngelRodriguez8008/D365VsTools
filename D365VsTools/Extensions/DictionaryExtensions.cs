// Created by: Rodriguez Mustelier Angel (rodang)
// Modify On: 2021-05-31 10:19

using System.Collections.Generic;

namespace D365VsTools.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out TValue result);
            return result;
        }
    }
}