using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace Weather
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            // setup Refresh button binding
            Refresh = new Command(
                execute: async () =>
                {
                    // show fake data
                    updated = DateTime.Now.ToString("dd MMM hh:mm tt");
                    location = "Fake Location";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updated)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));

                    // get the device's location
                    var deviceLocation = await Geolocation.GetLocationAsync();

                    // delay to simulate a network request
                    await Task.Delay(1000);

                    // show updated fake data after fake network request
                    updated = DateTime.Now.ToString("dd MMM hh:mm tt");
                    location = "Updated Fake Location";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updated)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));
                });
        }

        // Updated label binding
        string updated = String.Empty;
        public String Updated
        {
            get { return updated; }
            set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(updated)); }
        }

        // Location label binding
        string location = String.Empty;
        public String Location
        {
            get { return location; }
            set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(location)); }
        }

        // Refresh button binding
        public ICommand Refresh { private set; get; }

    }
}
