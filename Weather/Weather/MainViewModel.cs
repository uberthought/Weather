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
                    DisableRefresh();

                    // get the device's location
                    var deviceLocation = await Geolocation.GetLocationAsync();

                    // create NWS request object
                    var nwsService = new NWSService(deviceLocation.Latitude, deviceLocation.Longitude);

                    // request weather data
                    try
                    {
                        var tempF = (await nwsService.GetTemperature());
                        var dewPointF = (await nwsService.GetDewPoint());
                        var rh = await nwsService.GetRelativeHumidity();
                        var windSpeedMPH = (await nwsService.GetWindSpeed());
                        var windAngle = await nwsService.GetWindDirection();
                        var imageSourceUrl = await nwsService.GetConditionsIconUrl();
                        var timestamp = await nwsService.GetTimestamp();
                        var locationString = await nwsService.GetLocation();
                        var descriptionString = await nwsService.GetTextDescription();

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

                        // update labels
                        updated = timestamp.ToString("dd MMM hh:mm tt");
                        location = locationString;
                        textDescription = descriptionString;
                        temperature = $"{tempF:0} F";
                        dewPoint = $"Dew Point {dewPointF:0} F";
                        relativeHumidity = $"({rh:0}% RH)";
                        wind = $"Wind {windDirection} {windSpeedMPH:0} MPH";

                        if (!string.IsNullOrEmpty(imageSourceUrl))
                            conditionsIcon = ImageSource.FromUri(new Uri(imageSourceUrl));

                        // tell UI to update
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updated)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DewPoint)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextDescription)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelativeHumidity)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wind)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConditionsIcon)));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }

                    EnableRefresh();
                });

            Refresh.Execute(null);
        }

        string updated = "";
        public String Updated
        {
            get { return updated; }
            //set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(updated)); }
        }

        string location = "";
        public String Location
        {
            get { return location; }
        }

        string temperature = "";
        public string Temperature
        {
            get { return temperature; }
        }

        string dewPoint = "";
        public string DewPoint
        {
            get { return dewPoint; }
        }

        string textDescription = "";
        public string TextDescription
        {
            get { return textDescription; }
        }

        string relativeHumidity = "";
        public string RelativeHumidity
        {
            get { return relativeHumidity; }
        }

        string wind = "";
        public string Wind
        {
            get { return wind; }
        }

        ImageSource conditionsIcon;
        public ImageSource ConditionsIcon
        {
            get { return conditionsIcon; }
        }

        // Refresh button
        public ICommand Refresh { private set; get; }

        bool refreshIsEnabled = true;
        public bool RefreshIsEnabled
        {
            get { return refreshIsEnabled; }
        }

        void EnableRefresh()
        {
            refreshIsEnabled = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefreshIsEnabled)));
        }

        void DisableRefresh()
        {
            refreshIsEnabled = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefreshIsEnabled)));
        }
    }
}
