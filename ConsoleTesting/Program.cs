﻿using System;
using System.Collections.Generic;
using System.Linq;
using Translation;
using System.Text;
using System.Threading.Tasks;
using Translation.Storages;
using Translation.Matching;
using System.IO;
using Translation.Transliteration;
using Translation.Web;
using Newtonsoft.Json;

namespace ConsoleTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            //RepairSimilaritiesDB();
            //NewMatches();
            //RecalcSimilarities();
            //RecalcSimilaritiesTest();
            //PrepareToManualTranslate();
            //DB.Save();
            //TestTimer();
            WikiTest();

            //Console.ReadKey();
        }

        private static void WikiTest()
        {
            Language language0 = Language.Load(DB.EngLetters);
            Language language1 = Language.Load(DB.RusLetters);

            //string[] toTranslateWords = File.ReadAllLines(FileName.ToTranslateWords);
            //var translatedWords = WikiTranslator.Translate(toTranslateWords, language0, language1);
            //File.WriteAllLines(@"D:\Desktop\out.txt", translatedWords.Select(x => x.ToString()).ToArray());

            //var v = WikiTranslator.TranslateRough(new[] { "abi'l" }, language0, language1).ToList();
            File.WriteAllLines(
                @"D:\Desktop\Wiki.GetSimilar.txt",
                WikiTranslator.TranslateExact(Wiki.GetSimilar("Abi'l-Hadid"))
                    .Select(x => x.ToString())
                    .ToArray()
            );
        }

        private static void TestTimer()
        {
            QueueTimer timer = new QueueTimer(1000);
            timer.Tick += (s, e) => Console.WriteLine("1");
            timer.Tick += (s, e) => Console.WriteLine("2");
            timer.Tick += (s, e) => Console.WriteLine("3");
            timer.Tick += (s, e) => Console.WriteLine("4");
            timer.WaitMyTurn();
            Console.WriteLine("5");
        }

        private static void RepairSimilaritiesDB()
        {
            EngToRusSimilaritiesDB.Matrix oldItems = null;

            if (File.Exists(FileName.EngToRusSimilarities))
            {
                using (StreamReader file = File.OpenText(FileName.EngToRusSimilarities))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    oldItems = (EngToRusSimilaritiesDB.Matrix)serializer.Deserialize(file, typeof(EngToRusSimilaritiesDB.Matrix));
                }
            }

            var newDB = new EngToRusSimilaritiesDB();

            for (int rusLetter = 0; rusLetter < newDB.RusCount - 1; rusLetter++)
            {
                for (int engLetter = 0; engLetter < newDB.EngCount - 1; engLetter++)
                {
                    newDB[engLetter, rusLetter] = oldItems.Items[rusLetter, engLetter];
                }
            }

            newDB.Save(FileName.EngToRusSimilarities);
        }

        private static void NewMatches()
        {
            Language language0 = Language.Load(DB.EngLetters);
            Language language1 = Language.Load(DB.RusLetters);

            var paris = new[]
            {
                //("yffe", "иф"),
                ("richuin", "ришьон")
            };

            foreach (var (w0, w1) in paris)
            {
                WordMatch match = WordMatch.Create(w0, w1, language0, language1);

                Console.WriteLine($"Words: {w0}, {w1}");
                Console.Write("Matches:");

                foreach (var m in match.LetterMatches)
                {
                    Console.Write($" {m.Letters0}-{m.Letters1}");
                }

                Console.WriteLine();

                foreach (var lm in match.LetterMatches)
                {
                    Console.WriteLine($" {lm.Letters0}-{lm.Letters1}-{lm.Similarity}");
                }

                float similarity = match.Similarity;
                Console.WriteLine($"\nSimilarity: {similarity}");
                Console.WriteLine();
            }
        }

        private static void RecalcSimilarities()
        {
            Language language0 = Language.Load(DB.EngLetters);
            Language language1 = Language.Load(DB.RusLetters);
            string[] toTranslateWords = File.ReadAllLines(FileName.ToTranslateWords);

            List<WordInLangs> translatedWords = new List<WordInLangs>();
            List<string> toTransliterateWords = new List<string>();
            List<string> toWikiTranslate = new List<string>();

            foreach (string word in toTranslateWords)
            {
                // 1. Проверить наличие слов в БД переводов.
                string translation = DB.Translated.GetTranslation(word);
                if (translation != null)
                {
                    translatedWords.Add(new WordInLangs(word, translation));
                }
                else
                {
                    // 2.Проверить наличие слов в БД без переводов
                    // Буквы через wiki не переводить, они не переведутся
                    if (DB.NotTranslated.Contains(word) || word.Length == 1)
                    {
                        toTransliterateWords.Add(word);
                    }
                    else
                    {
                        toWikiTranslate.Add(word);
                    }
                }
            }

            // 3. Wiki перевод
            List<WordInLangs> wikiTranslations = WikiTranslator.Translate(toWikiTranslate, language0, language1).ToList();
            foreach (WordInLangs wordInLangs in wikiTranslations)
            {
                if (wordInLangs.Lang2Word != null)
                {
                    DB.Translated.Add(wordInLangs);
                    translatedWords.Add(wordInLangs);
                }
                else
                {
                    DB.NotTranslated.Add(wordInLangs.Lang1Word);
                    toTransliterateWords.Add(wordInLangs.Lang1Word);
                }
            }

            // 4. Производим обучение переводчика
            IEnumerable<WordInLangs> wordsToLearn = DB.Translated.WordsInLangs
                .Union(DB.EngToRusMap.Select(x => new WordInLangs(x.eng.ToString(), x.rus)));

            List<TranslationRule> rules = RuleRecognizer.Recognize(language0, language1, wordsToLearn);
            RulesDB rulesDB = new RulesDB();
            rulesDB.AddRange(rules);
            rulesDB.Save(FileName.RulesDB);

            // 5. Производим транслитерацию списка слов отложенных для транслитерации
            Translator translator = Translator.Create(rules, language0);

            foreach (string word in toTransliterateWords)
            {
                translatedWords.Add(new WordInLangs(word, translator.Translate(word)));
            }

            File.Delete(@"D:\Desktop\CK2Works\Translated.txt");
            File.WriteAllLines(
                @"D:\Desktop\CK2Works\Translated.txt",
                translatedWords.Select(wordInLangs => wordInLangs.ToString())
            );

            DB.Save();
        }

        private static void RecalcSimilaritiesTest()
        {
            Language language0 = Language.Load(DB.EngLetters);
            Language language1 = Language.Load(DB.RusLetters);
            string[] toTranslateWords = File.ReadAllLines(FileName.ToTranslateWords);

            List<(string original, string translation, string transliteration)> translatedWords = new List<(string, string, string)>();
            List<string> toTransliterateWords = new List<string>();
            List<string> toWikiTranslate = new List<string>();


            IEnumerable<WordInLangs> wordsToLearn = DB.Translated.WordsInLangs
                .Union(DB.EngToRusMap.Select(x => new WordInLangs(x.eng.ToString(), x.rus)));

            List<TranslationRule> rules = RuleRecognizer.Recognize(language0, language1, wordsToLearn);

            Translator translator = Translator.Create(rules, language0);
            List<WordInLangs> transliteratedWords = new List<WordInLangs>();

            foreach (string word in toTranslateWords)
            {
                // 1. Проверить наличие слов в БД переводов.
                string translation = DB.Translated.GetTranslation(word);
                if (translation != null && WordMatch.Create(word, translation, language0, language1).Similarity >= 0.68)
                {
                    translatedWords.Add((word, translation, translator.Translate(word)));
                }
            }

            File.Delete(@"D:\Desktop\CK2Works\Transliterated.txt");
            File.WriteAllLines(
                @"D:\Desktop\CK2Works\Transliterated.txt",
                translatedWords.Select(x => $"{x.original}\t{x.translation}\t{x.transliteration}")
            );
        }

        //private static void PrepareToManualTranslate()
        //{
        //    string wordsFileName = @"D:\Desktop\CK2Works\NotTransleated.txt";
        //    string englishWordsFileName = @"D:\Desktop\CK2Works\Dictionaries\words.txt";

        //    HashSet<string> engWords = File.ReadAllLines(englishWordsFileName)
        //           .Select(w => w.ToLower())
        //           .ToHashSet();
        //    char[] forbiddenChars = "()[]§".ToArray();
        //    HashSet<char> allowedChars = @" '-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÄÅÆÇÉÍÎÑÒÓÖØÚÜÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþœŠšŽž"
        //        .ToHashSet();

        //    List<string> words = File.ReadAllLines(wordsFileName)
        //        .Where(line => line.Any(char.IsLetter) &&
        //                    !line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
        //                        .Any(word => engWords.Contains(word.ToLower())) && // Не является предложением
        //                    !forbiddenChars.Any(c => line.Contains(c))  //Не содержит запрещённые символы
        //        )
        //        .Select(line => CharEncoding.Decode(line)
        //                            .Replace('‘', '\'')
        //                            .Replace('`', '\'')
        //                            .Replace('’', '\'')
        //                            .Replace('´', '\'')
        //                            .Replace('‚', ',')
        //                            .Replace('¸', ',')
        //                            .Replace('—', '-')
        //                            .Replace('–', '-')
        //                            .Replace('“', '"')
        //                            .Replace('”', '"')
        //                            .Replace("…", "...")
        //        )
        //        .Where(line => line.All(c => allowedChars.Contains(c)))
        //        .SelectMany(line => line.ToLower().Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries))
        //        .Distinct()
        //        .OrderBy(x => x)
        //        .ToList();

        //    File.Delete(FileName.ToTranslateWords);
        //    File.WriteAllLines(FileName.ToTranslateWords, words);
        //}
    }
}