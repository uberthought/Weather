using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Weather
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Refresh = new Command(
                execute: () =>
                {
                    updated = "Hello, World!";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updated)));
                });
        }
        public event PropertyChangedEventHandler PropertyChanged;

        string updated = String.Empty;

        public String Updated
        {
            get { return updated; }
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(updated));
            }
        }
        public ICommand Refresh { private set; get; }

    }
}
