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
                    // get the device's location
                    var deviceLocation = await Geolocation.GetLocationAsync();

                    // create NWS request object
                    var nwsService = new NWSService(deviceLocation.Latitude, deviceLocation.Longitude);

                    // request weather data
                    var tempF = (await nwsService.GetTemperature());
                    var dewPointF = (await nwsService.GetDewPoint());
                    var rh = await nwsService.GetRelativeHumidity();
                    var windSpeedMPH = (await nwsService.GetWindSpeed());
                    var windAngle = await nwsService.GetWindDirection();
                    var imageSourceUrl = await nwsService.GetConditionsIconUrl();

                    var windDirection = "";
                    if (windAngle > 337.5 || windAngle <= 22.5)
                        windDirection = "N";
                    else if (windAngle > 22.5 && windAngle <= 67.5)
                        windDirection = "NE";
                    else if (windAngle > 67.5 && windAngle <= 112.5)
                        windDirection = "E";
                    else if (windAngle > 112.5 && windAngle <= 157.5)
                        windDirection = "SE";
                    else if (windAngle > 157.5 && windAngle <= 202.5)
                        windDirection = "S";
                    else if (windAngle > 202.5 && windAngle <= 247.5)
                        windDirection = "SW";
                    else if (windAngle > 247.5 && windAngle <= 292.5)
                        windDirection = "W";
                    else if (windAngle > 292.5 && windAngle <= 337.5)
                        windDirection = "NW";

                    updated = (await nwsService.GetTimestamp()).ToString("dd MMM hh:mm tt");
                    location = await nwsService.GetLocation();
                    textDescription = await nwsService.GetTextDescription();
                    temperature = $"{tempF:0} F";
                    dewPoint = $"Dew Point {dewPointF:0} F";
                    relativeHumidity = $"({rh:0}% RH)";
                    wind = $"Wind {windDirection} {windSpeedMPH:0} MPH";

                    if (!string.IsNullOrEmpty(imageSourceUrl))
                        conditionsIcon = ImageSource.FromUri(new Uri(imageSourceUrl));

                    // update UI
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updated)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DewPoint)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextDescription)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelativeHumidity)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wind)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConditionsIcon)));
                });

            Refresh.Execute(null);
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

        string temperature = string.Empty;
        public string Temperature
        {
            get { return temperature; }
            set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(temperature)); }
        }

        string dewPoint = string.Empty;
        public string DewPoint
        {
            get { return dewPoint; }
            set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(dewPoint)); }
        }

        string textDescription = string.Empty;
        public string TextDescription
        {
            get { return textDescription; }
            set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(textDescription)); }
        }

        string relativeHumidity = string.Empty;
        public string RelativeHumidity
        {
            get { return relativeHumidity; }
            set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(relativeHumidity)); }
        }

        string wind = string.Empty;
        public string Wind
        {
            get { return wind; }
            set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(wind)); }
        }

        ImageSource conditionsIcon;
        public ImageSource ConditionsIcon
        {
            get { return conditionsIcon; }
            //set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(conditionsIcon)); }
        }

        // Refresh button binding
        public ICommand Refresh { private set; get; }

    }
}
