using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading;
using System.Linq;

namespace Weather
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Timer timer;

        public MainViewModel()
        {
            // setup Refresh button binding
            RefreshCommand = new Command(execute: Refresh);

            // execute a refresh command now and every hour
            timer = new Timer((o) =>
            {
            MainThread.BeginInvokeOnMainThread(() => { RefreshCommand.Execute(null); } );
            }, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        }

        public async void Refresh()
        {
            if (isRefreshing)
                return;
            isRefreshing = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));

            try
            {
                // get the device's location
                var location = await App.GetLocation();

                // create NWS request object and get forecast
                var nwsService = await NWSService.GetForecast(location.Latitude, location.Longitude);

                requested = "Last Requested: " + DateTime.Now.ToString("dd MMM hh:mm tt");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Requested)));

                if (nwsService.Location == null)
                {
                    this.location = "Forecast Unavailable";
                    updated = null;
                    temperature = null;
                    dewPoint = null;
                    textDescription = null;
                    relativeHumidity = null;
                    wind = null;
                    conditionsIcon = null;
                    forecastCells = null;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updated)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DewPoint)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextDescription)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelativeHumidity)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wind)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConditionsIcon)));

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastCells)));

                    isRefreshing = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));

                    return;
                }

                var windDirection = "";
                if (nwsService.WindDirection > 337.5 || nwsService.WindDirection <= 22.5)
                    windDirection = "N";
                else if (nwsService.WindDirection > 22.5 && nwsService.WindDirection <= 67.5)
                    windDirection = "NE";
                else if (nwsService.WindDirection > 67.5 && nwsService.WindDirection <= 112.5)
                    windDirection = "E";
                else if (nwsService.WindDirection > 112.5 && nwsService.WindDirection <= 157.5)
                    windDirection = "SE";
                else if (nwsService.WindDirection > 157.5 && nwsService.WindDirection <= 202.5)
                    windDirection = "S";
                else if (nwsService.WindDirection > 202.5 && nwsService.WindDirection <= 247.5)
                    windDirection = "SW";
                else if (nwsService.WindDirection > 247.5 && nwsService.WindDirection <= 292.5)
                    windDirection = "W";
                else if (nwsService.WindDirection > 292.5 && nwsService.WindDirection <= 337.5)
                    windDirection = "NW";

                // update current condition
                updated = "Forecast Updated: " + nwsService.Timestamp?.ToString("dd MMM hh:mm tt");
                this.location = nwsService.Location;
                textDescription = nwsService.TextDescription;
                temperature = $"{nwsService.Temperature:0}℉";
                dewPoint = $"Dew Point {nwsService.DewPoint:0}℉";
                relativeHumidity = $"({nwsService.RelativeHumidity:0}% RH)";
                wind = $"Wind {windDirection} {nwsService.WindSpeed:0} MPH";
                if (!string.IsNullOrEmpty(nwsService.ConditionIconUrl))
                    conditionsIcon = ImageSource.FromUri(new Uri(nwsService.ConditionIconUrl));

                // update forecast
                var forecastCount = new List<int>
                        {
                            nwsService.ForecastLabels.Count(),
                            nwsService.ForecastIcons.Count(),
                            nwsService.ForecastDescriptions.Count(),
                            nwsService.ForecastLows.Count() * 2,
                            nwsService.ForecastHighs.Count() * 2
                        }.Min();

                forecastCells = new List<ForecastCell>();
                for (var i = 0; i < forecastCount; i++)
                {
                    var isLow = (nwsService.ForecastLabels[0] == "Tonight" && i % 2 == 0);
                    var temperature = isLow ? nwsService.ForecastLows[(int)(i / 2)] : nwsService.ForecastHighs[(int)(i / 2)];

                    forecastCells.Add(new ForecastCell
                    {
                        Label = nwsService.ForecastLabels[i],
                        Icon = ImageSource.FromUri(new Uri(nwsService.ForecastIcons[i])),
                        Description = nwsService.ForecastDescriptions[i],
                        TemperatureLabel = isLow ? "Low:" : "Hi:",
                        Temperature = $"{temperature:0}℉",
                        TemperatureColor = isLow ? Color.Blue : Color.Red,
                        BackgroundColor = isLow ? Color.DarkGray : Color.LightBlue
                    });
                }

                // tell UI to update
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updated)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DewPoint)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextDescription)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelativeHumidity)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wind)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConditionsIcon)));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastCells)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            isRefreshing = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));
        }

        string updated = "";
        public String Updated
        {
            get { return updated; }
            //set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(updated)); }
        }

        string requested = "";
        public string Requested { get => requested; }

        // current conditions

        string location = "";
        public String Location { get => location; }

        string temperature = "";
        public string Temperature { get => temperature; }

        string dewPoint = "";
        public string DewPoint { get => dewPoint; }

        string textDescription = "";
        public string TextDescription { get => textDescription; }

        string relativeHumidity = "";
        public string RelativeHumidity { get => relativeHumidity; }

        string wind = "";
        public string Wind { get => wind; }

        ImageSource conditionsIcon;
        public ImageSource ConditionsIcon { get => conditionsIcon; }

        // forecast

        List<ForecastCell> forecastCells;
        public List<ForecastCell> ForecastCells { get { return forecastCells; } }

        // Refresh

        public ICommand RefreshCommand { private set; get; }

        bool refreshIsEnabled = true;
        public bool RefreshIsEnabled { get => refreshIsEnabled; }

        bool isRefreshing;
        public bool IsRefreshing { get => isRefreshing; }
    }
}
