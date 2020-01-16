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
            RefreshCommand = new Command(
                execute: async () =>
                {
                    isRefreshing = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));

                    try
                    {
                        // get the device's location
                        var deviceLocation = await Geolocation.GetLocationAsync();

                        // create NWS request object
                        var nwsService = new NWSService(deviceLocation.Latitude, deviceLocation.Longitude);

                        requested = "Last Requested: " + DateTime.Now.ToString("dd MMM hh:mm tt");
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Requested)));

                        // request weather data
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

                        // update current condition
                        updated = "Forecast Updated: " + timestamp.ToString("dd MMM hh:mm tt");
                        location = locationString;
                        textDescription = descriptionString;
                        temperature = $"{tempF:0}℉";
                        dewPoint = $"Dew Point {dewPointF:0}℉";
                        relativeHumidity = $"({rh:0}% RH)";
                        wind = $"Wind {windDirection} {windSpeedMPH:0} MPH";
                        if (!string.IsNullOrEmpty(imageSourceUrl))
                            conditionsIcon = ImageSource.FromUri(new Uri(imageSourceUrl));

                        // update forecast
                        var forecastLabels = await nwsService.GetForecastLabels();
                        var forecastIcons = await nwsService.GetForecastIcons();
                        var forecastDescriptions = await nwsService.GetForecastDescriptions();
                        var forecastLows = await nwsService.GetForecastLows();
                        var forecastHighs = await nwsService.GetForecastHighs();

                        var forecastCount = new List<int>
                        {
                            forecastLabels.Count(),
                            forecastIcons.Count(),
                            forecastDescriptions.Count(),
                            forecastLows.Count() * 2,
                            forecastHighs.Count() * 2
                        }.Min();

                        forecastCells = new List<ForecastCell>();
                        for (var i = 0; i < forecastCount; i++)
                        {
                            var isLow = (forecastLabels[0] == "Tonight" && i % 2 == 0);
                            var temperature = isLow ? forecastLows[(int)(i / 2)] : forecastHighs[(int)(i / 2)];

                            forecastCells.Add(new ForecastCell
                            {
                                Label = forecastLabels[i],
                                Icon = ImageSource.FromUri(new Uri(forecastIcons[i])),
                                Description = forecastDescriptions[i],
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
                });

            // execute a refresh command now and every hour
            timer = new Timer((o) =>
            {
            MainThread.BeginInvokeOnMainThread(() => { RefreshCommand.Execute(null); } );
            }, null, TimeSpan.Zero, TimeSpan.FromHours(1));
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
