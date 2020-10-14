﻿using System;
using System.Linq;
using System.Collections.Generic;

using Translation.Graphemes;
using Translation.Storages;

namespace Translation
{
    /// <summary>
    /// Класс реализует функционал связанный с языком
    /// </summary>
    public class Language
    {
        private readonly Grapheme[] graphemesByChar;
        private readonly int[] identifiersByChar;
        private readonly Grapheme[] graphemesByIdentifier;
        private readonly char[] charsByIdentifier;
        public readonly int MinLetter;
        public readonly int MaxLetter;

        private Language(int minLetter, int maxLetter)
        {
            this.graphemesByChar = new Grapheme[maxLetter - minLetter + 1];
            this.identifiersByChar = new int[maxLetter - minLetter + 1];
            this.graphemesByIdentifier = new Grapheme[maxLetter - minLetter + 1];
            this.charsByIdentifier = new char[maxLetter - minLetter + 1];
            this.MinLetter = minLetter;
            this.MaxLetter = maxLetter;
        }

        public static Language Load(LettersDB lettersDB)
        {
            var groups = new[] { lettersDB.Vowels, lettersDB.Consonants, lettersDB.Silents };

            (int minLetter, int maxLetter) = groups
                .SelectMany(group => group)
                .Aggregate(
                    (min: (int)char.MaxValue, max: (int)char.MinValue),
                    (a, l) => (Math.Min(a.min, l), Math.Max(a.max, l)));

            Language language = new Language(minLetter, maxLetter);
            GraphemeType[] types = new[] { GraphemeType.Vowel, GraphemeType.Consonant, GraphemeType.Silent };
            int identifier = 0;

            foreach ((IEnumerable<char> letters, GraphemeType type) in groups.Zip(types))
            {
                foreach (char letter in letters)
                {
                    Grapheme g = new Grapheme(type, letter.ToString());
                    language.graphemesByIdentifier[identifier] = g;
                    language.charsByIdentifier[identifier] = letter;
                    language.graphemesByChar[letter - minLetter] = g;
                    language.identifiersByChar[letter - minLetter] = identifier++;
                }
            }

            return language;
        }

        public Grapheme ToGrapheme(char letter)
        {
            return this.graphemesByChar[letter - this.MinLetter].Clone();
        }

        public List<Grapheme> ToGraphemes(string word)
        {
            List<Grapheme> vowels = new List<Grapheme>(word.Length);
            List<Grapheme> graphemes = new List<Grapheme>(word.Length);
            Grapheme lasVowel = null;
            Grapheme lastNotSilent = null;
            int consonantsInRow = 0;

            foreach (char letter in word)
            {
                Grapheme current = this.ToGrapheme(letter);
                graphemes.Add(current);

                if (current.Type == GraphemeType.Silent)
                {
                    continue;
                }

                if (current.Type == GraphemeType.Vowel)
                {
                    vowels.Add(current);

                    if (lasVowel != null)
                    {
                        if (consonantsInRow < 2)
                        {
                            lasVowel.Flags.Set((uint)VowelFlag.OpenSyllable);
                        }
                    }

                    lasVowel = current;
                    consonantsInRow = 0;
                }
                else
                {
                    consonantsInRow++;
                }

                if (lastNotSilent != null)
                {
                    if (lastNotSilent.Type == GraphemeType.Vowel)
                    {
                        current.Flags.Set(current.Type == GraphemeType.Vowel ? (uint)VowelFlag.PreviousVowel : (uint)ConsonantFlag.PreviousVowel);
                    }
                    if (current.Type == GraphemeType.Vowel)
                    {
                        lastNotSilent.Flags.Set(lastNotSilent.Type == GraphemeType.Vowel ? (uint)VowelFlag.NextVowel : (uint)ConsonantFlag.NextVowel);
                    }
                }

                lastNotSilent = current;
            }

            if (vowels.Count > 0)
            {
                if (vowels.Count <= 2)
                {
                    vowels[0].Flags.Set((uint)VowelFlag.Stressed);
                }
                else
                {
                    vowels[vowels.Count - 3].Flags.Set((uint)VowelFlag.Stressed);
                }
            }

            Grapheme first = graphemes[0];
            if (first.Type == GraphemeType.Vowel || first.Type == GraphemeType.Consonant)
            {
                first.Flags.Set((uint)CommonFlag.First);
            }
            Grapheme last = graphemes[graphemes.Count - 1];
            if (last.Type == GraphemeType.Vowel || last.Type == GraphemeType.Consonant)
            {
                last.Flags.Set((uint)CommonFlag.Last);
            }

            return graphemes;
        }

        public int ToIdentifier(char letter)
        {
            return this.identifiersByChar[letter - this.MinLetter];
        }

        public int[] ToIdentifiers(string word)
        {
            int[] indexes = new int[word.Length];

            for (int i = 0; i < word.Length; i++)
            {
                indexes[i] = this.ToIdentifier(word[i]);
            }

            return indexes;
        }

        public char ToLetter(int identifier)
        {
            return this.charsByIdentifier[identifier];
        }

        public Grapheme ToGrapheme(int identifier)
        {
            return this.graphemesByIdentifier[identifier].Clone();
        }
    }
}