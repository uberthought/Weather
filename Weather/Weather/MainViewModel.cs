using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading;

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
                        forecastLabels = await nwsService.GetForecastLabels();
                        forecastIcons = await nwsService.GetForecastIcons();
                        forecastDescriptions = await nwsService.GetForecastDescriptions();
                        var forecastLows = await nwsService.GetForecastLows();
                        var forecastHighs = await nwsService.GetForecastHighs();

                        if (await nwsService.GetIsForecastDay())
                        {
                            forecastTemperatureLabels = new List<string> { "Hi:", "Low:", "Hi:" };
                            forecastTemperatures = new List<string>
                            {
                                $"{forecastHighs[0]:0}℉",
                                $"{forecastLows[0]:0}℉",
                                $"{forecastHighs[1]:0}℉"
                            };
                            forecastColors = new List<Color>
                            {
                                Color.Red,
                                Color.Blue,
                                Color.Red,
                            };
                            forecastBackgrounds = new List<Color>
                            {
                                Color.LightBlue,
                                Color.DarkGray,
                                Color.LightBlue,
                            };
                        }
                        else
                        {
                            forecastTemperatureLabels = new List<string> { "Low:", "Hi:", "Low:" };
                            forecastTemperatures = new List<string>
                            {
                                $"{forecastLows[0]:0}℉",
                                $"{forecastHighs[0]:0}℉",
                                $"{forecastLows[1]:0}℉"
                            };
                            forecastColors = new List<Color>
                            {
                                Color.Blue,
                                Color.Red,
                                Color.Blue,
                            };
                            forecastBackgrounds = new List<Color>
                            {
                                Color.DarkGray,
                                Color.LightBlue,
                                Color.DarkGray,
                            };
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

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastLabels)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastIcons)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastDescriptions)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastTemperatureLabels)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastTemperatures)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastColors)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastBackgrounds)));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }

                    isRefreshing = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));
                });

            // execute a refresh command now and every hour
            timer = new Timer((o) => { RefreshCommand.Execute(null); }, null, TimeSpan.Zero, TimeSpan.FromHours(1));
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

        List<string> forecastLabels;
        public List<string> ForecastLabels
        {
            get
            {
                if (forecastLabels == null)
                    return new List<string> { "", "", "" };
                return forecastLabels;
            }
        }


        List<string> forecastIcons;
        public List<string> ForecastIcons
        {
            get
            {
                if (forecastIcons == null)
                    return new List<string> { "", "", "" };
                return forecastIcons;
            }
        }

        List<string> forecastDescriptions;
        public List<string> ForecastDescriptions
        {
            get
            {
                if (forecastDescriptions == null)
                    return new List<string> { "", "", "" };
                return forecastDescriptions;
            }
        }

        List<string> forecastTemperatureLabels;
        public List<string> ForecastTemperatureLabels
        {
            get
            {
                if (forecastTemperatureLabels == null)
                    return new List<string> { "", "", "" };
                return forecastTemperatureLabels;
            }
        }

        List<string> forecastTemperatures;
        public List<string> ForecastTemperatures
        {
            get
            {
                if (forecastTemperatures == null)
                    return new List<string> { "", "", "" };
                return forecastTemperatures;
            }
        }

        List<Color> forecastColors;
        public List<Color> ForecastColors
        {
            get
            {
                if (forecastColors == null)
                    return new List<Color> { Color.Black, Color.Black, Color.Black };
                return forecastColors;
            }
        }

        List<Color> forecastBackgrounds;
        public List<Color> ForecastBackgrounds
        {
            get
            {
                if (forecastBackgrounds == null)
                    return new List<Color> { Color.LightBlue, Color.LightBlue, Color.LightBlue };
                return forecastBackgrounds;
            }
        }

        // Refresh

        public ICommand RefreshCommand { private set; get; }

        bool refreshIsEnabled = true;
        public bool RefreshIsEnabled { get => refreshIsEnabled; }

        bool isRefreshing;
        public bool IsRefreshing { get => isRefreshing; }
    }
}
