﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Translation.Storages
{
    public class LettersRepository : Repository, IReadOnlyCollection<char>
    {
        private Letters data;

        public List<char> Vowels
        {
            get => this.data.Vowels;
            set => this.data.Vowels = value;
        }

        public List<char> Consonants
        {
            get => this.data.Consonants;
            set => this.data.Consonants = value;
        }

        public List<char> Silents
        {
            get => this.data.Silents;
            set => this.data.Silents = value;
        }

        public char this[int index] => index switch
        {
            _ when index < this.Vowels.Count => this.Vowels[index],
            _ when (index -= this.Vowels.Count) < this.Consonants.Count => this.Consonants[index],
            _ => this.Silents[index - this.Consonants.Count]
        };

        public int Count => this.Vowels.Count + this.Consonants.Count + this.Silents.Count;

        public IEnumerator<char> GetEnumerator()
        {
            return this.Vowels
                .Union(this.Consonants)
                .Union(this.Silents)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        protected override void LoadData(string fileName)
        {
            this.data = JsonHelper.Deserialize<Letters>(fileName);
        }

        protected override object GetDataToSave()
        {
            return this.data;
        }

        public class Letters
        {
            public List<char> Consonants;
            public List<char> Silents;
            public List<char> Vowels;
        }
    }
}