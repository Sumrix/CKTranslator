﻿using System.Collections.Generic;
using System.Linq;
using Core.Storages;
using Core.Translation.Graphemes;
using Core.Translation.Interpolation;
using Core.Translation.Matching;
using Core.Translation.Parsing;

namespace Core.Translation.Transliteration
{
    /// <summary>
    ///     Создаёт правила перевода по существующему словарю
    /// </summary>
    public static class RuleRecognizer
    {
        private static List<GraphemeTranslation> CorrectGraphemeLengths(
            Language srcLanguage,
            IReadOnlyCollection<IReadOnlyList<GraphemeTranslation>> wordGraphemeTranslations)
        {
            var graphemeTranslations = wordGraphemeTranslations
                .SelectMany(t => t)
                .ToList();
            TranslationLengthCorrector lengthCorrector =
                TranslationLengthCorrector.Create(graphemeTranslations, srcLanguage);
            var correctedGraphemeTranslations = wordGraphemeTranslations
                .SelectMany(t => lengthCorrector.Correct(t))
                .ToList();
            return correctedGraphemeTranslations;
        }

        private static List<TransliterationRule> CreateTranslationRules(
            IEnumerable<GraphemeTranslation> correctedGraphemeTranslations)
        {
            GraphemeStatistic statistic = GraphemeStatistic.Create(correctedGraphemeTranslations);
            var rules = TransliterationRule.Create(statistic);
            foreach (TransliterationRule rule in rules)
            {
                Interpolater.Interpolate(rule.Target, Bit.OnesCount((uint) rule.Target.Length - 1));
            }

            return rules;
        }

        public static List<TransliterationRule> Recognize(
            Language sourceLanguage,
            Language resultLanguage,
            IEnumerable<WordInLangs> wordTextTranslations)
        {
            var wordGraphemeTranslations = (
                from t in wordTextTranslations
                let match = WordMatch.Create(t.Lang1Word, t.Lang2Word, sourceLanguage, resultLanguage)
                where match.Success
                select GraphemeTranslation.Create(match.LetterMatches, sourceLanguage.ToGraphemes(t.Lang1Word))
            ).ToList();

            var correctedGraphemeTranslations =
                RuleRecognizer.CorrectGraphemeLengths(sourceLanguage, wordGraphemeTranslations);

            var allGraphemeTranslations = Db.EngToRusMap
                .Select(x => new GraphemeTranslation(sourceLanguage.ToGrapheme(x.eng.ToString()), x.rus))
                .Union(correctedGraphemeTranslations);

            var rules =
                RuleRecognizer.CreateTranslationRules(allGraphemeTranslations);

            return rules;
        }
    }
}