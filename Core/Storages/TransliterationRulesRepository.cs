﻿using System.Collections.Generic;
using Core.Transliteration;

namespace Core.Storages
{
    public class TransliterationRulesRepository : DictionaryRepository<TransliterationRule, string, string[]>
    {
        protected override KeyValuePair<string, string[]> Item2KeyValuePair(TransliterationRule item)
        {
            return new(item.Source, item.Target);
        }

        protected override TransliterationRule KeyValuePair2Item(KeyValuePair<string, string[]> keyValuePair)
        {
            return new() { Source = keyValuePair.Key, Target = keyValuePair.Value };
        }
    }
}