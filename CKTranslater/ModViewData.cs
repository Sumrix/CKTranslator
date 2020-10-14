﻿using System.ComponentModel;

namespace CKTranslater
{
    /// <summary>
    /// Представление мода в интерфейсе
    /// </summary>
    public class ModViewData : INotifyPropertyChanged
    {
        private bool isChecked;
        private int progress;
        private int progressMax;

        public ModInfo ModInfo { get; set; }
        public bool IsChecked
        {
            get => this.isChecked;
            set
            {
                this.isChecked = value;
                this.NotifyPropertyChanged(nameof(this.IsChecked));
            }
        }
        public int Progress
        {
            get => this.progress;
            set
            {
                this.progress = value;
                this.NotifyPropertyChanged(nameof(this.Progress));
            }
        }
        public int ProgressMax
        {
            get => this.progressMax;
            set
            {
                this.progressMax = value;
                this.NotifyPropertyChanged(nameof(this.ProgressMax));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ModViewData()
        {
            this.progressMax = 100;
            this.progress = 0;
        }
    }
}
