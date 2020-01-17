using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Weather
{
    class NWSService
    {
        static string baseAddress = "https://forecast.weather.gov/MapClick.php";
        HttpClient client;
        DateTime lastRefresh = DateTime.MinValue;

        public double QueryLatitude { private set; get; }
        public double QueryLongitude { private set; get; }

        public string Location { private set; get; }

        // current conditions
        public double? Temperature { private set; get; }
        public DateTime? Timestamp { private set; get; }
        public string TextDescription { private set; get; }
        public double? WindDirection { private set; get; }
        public double? WindSpeed { private set; get; }
        public double? DewPoint { private set; get; }
        public double? RelativeHumidity { private set; get; }
        public string ConditionIconUrl { private set; get; }
        public double? Visibility { private set; get; }
        public double? Gust { private set; get; }
        public double? Pressure { private set; get; }

        // forecast
        public List<string> ForecastLabels { private set; get; }
        public List<string> ForecastIcons { private set; get; }
        public List<string> ForecastDescriptions { private set; get; }
        public List<double> ForecastLows { private set; get; }
        public List<double> ForecastHighs { private set; get; }

        public List<string> WordedForecast { private set; get; }

        static NWSService service;
        public static NWSService GetService()
        {
            if (service == null)
                service = new NWSService();
            return service;
        }

        public async Task SetLocation(double latitude, double longitude)
        {
            if (QueryLatitude != latitude || QueryLongitude != longitude)
            {
                QueryLatitude = latitude;
                QueryLongitude = longitude;

                Location = null;
                Temperature = null;
                Timestamp = null;
                TextDescription = null;
                WindDirection = null;
                WindSpeed = null;
                DewPoint = null;
                RelativeHumidity = null;
                ConditionIconUrl = null;
                ForecastLabels = null;
                ForecastIcons = null;
                ForecastDescriptions = null;
                ForecastLows = null;
                ForecastHighs = null;
                WordedForecast = null;

                lastRefresh = DateTime.MinValue;
            }
            await Refresh();
        }

        NWSService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Weather app");
        }

        public bool IsValid => Location != null;

        async Task Refresh()
        {
            if (IsValid && DateTime.UtcNow - lastRefresh < TimeSpan.FromMinutes(15))
                return;
            await Task.Delay(100);
            try
            {
                // https://forecast.weather.gov/MapClick.php?lat=27.9789&lon=-82.7658&FcstType=dwml
                var uri = new Uri($"{baseAddress}/?lat={QueryLatitude}&lon={QueryLongitude}&unit=0&lg=english&FcstType=dwml");
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    // extract the needed data from the json string
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Contains("<title>Forecast Error</title>"))
                        return;
                    var root = XElement.Parse(content);

                    // current conditions

                    var current = from el in root.Elements("data")
                                  where (string)el.Attribute("type") == "current observations"
                                  select el;
                    Location = current.Elements("location")
                        .SelectMany(el => el.Elements("area-description"))
                        .FirstOrDefault().Value;

                    var timestampString = current.Elements("time-layout")
                        .SelectMany(el => el.Elements("start-valid-time"))
                        .FirstOrDefault().Value;

                    var temperatureString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("temperature"))
                        .Where(el => (string)el.Attribute("type") == "apparent")
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    var dewPointString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("temperature"))
                        .Where(el => (string)el.Attribute("type") == "dew point")
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    var relativeHumidityString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("humidity"))
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    TextDescription = current.Elements("parameters")
                        .SelectMany(el => el.Elements("weather"))
                        .SelectMany(el => el.Elements("weather-conditions"))
                        .Select(el => el.Attribute("weather-summary"))
                        .FirstOrDefault().Value;
                    if (TextDescription == "NA")
                        TextDescription = "";

                    var windSpeedString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("wind-speed"))
                        .Where(el => (string)el.Attribute("type") == "sustained")
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    var windDirectionString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("direction"))
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    ConditionIconUrl = current.Elements("parameters")
                        .SelectMany(el => el.Elements("conditions-icon"))
                        .SelectMany(el => el.Elements("icon-link"))
                        .FirstOrDefault().Value;
                    if (ConditionIconUrl == "NULL")
                        ConditionIconUrl = "";
                    else
                        ConditionIconUrl = ConditionIconUrl.Replace("http://", "https://");

                    Timestamp = DateTime.Parse(timestampString);
                    Temperature = double.Parse(temperatureString);
                    DewPoint = double.Parse(dewPointString);
                    RelativeHumidity = double.Parse(relativeHumidityString);
                    if (windSpeedString == "NA")
                        WindSpeed = 0;
                    else
                        WindSpeed = double.Parse(windSpeedString) * 1.15077945;
                    if (windDirectionString == "NA")
                        WindDirection = 0;
                    else
                        WindDirection = double.Parse(windDirectionString);

                    var gustString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("wind-speed"))
                        .Where(el => (string)el.Attribute("type") == "gust")
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    if (gustString == "NA")
                        Gust = 0;
                    else
                        Gust = double.Parse(gustString) * 1.15077945;

                    var visibilityString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("weather"))
                        .SelectMany(el => el.Elements("weather-conditions"))
                        .SelectMany(el => el.Elements("value"))
                        .SelectMany(el => el.Elements("visibility"))
                        .FirstOrDefault().Value;

                    if (string.IsNullOrEmpty(visibilityString) || visibilityString == "NA")
                        Visibility = 0;
                    else
                        Visibility = double.Parse(visibilityString);

                    var pressureString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("pressure"))
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    if (string.IsNullOrEmpty(pressureString) || pressureString == "NA")
                        Pressure = 0;
                    else
                        Pressure = double.Parse(pressureString);

                    // forecast

                    var forecast = from el in root.Elements("data")
                                   where (string)el.Attribute("type") == "forecast"
                                   select el;

                    ForecastLabels = forecast.Elements("time-layout")
                        .SelectMany(el => el.Elements("start-valid-time"))
                        .Select(el => el.Attribute("period-name"))
                        .Where(el => el != null)
                        .Select(el => el.Value)
                        .Take(14)
                        .ToList();

                    ForecastIcons = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("conditions-icon"))
                        .SelectMany(el => el.Elements("icon-link"))
                        .Select(el => el.Value)
                        .Select(s => s.Replace("http://", "https://"))
                        .ToList();

                    ForecastDescriptions = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("weather"))
                        .SelectMany(el => el.Elements("weather-conditions"))
                        .Select(el => el.Attribute("weather-summary"))
                        .Select(el => el.Value)
                        .ToList();

                    ForecastLows = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("temperature"))
                        .Where(el => (string)el.Attribute("type") == "minimum")
                        .SelectMany(el => el.Elements("value"))
                        .Select(el => el.Value)
                        .Select(s => double.Parse(s))
                        .ToList();

                    ForecastHighs = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("temperature"))
                        .Where(el => (string)el.Attribute("type") == "maximum")
                        .SelectMany(el => el.Elements("value"))
                        .Select(el => el.Value)
                        .Select(s => double.Parse(s))
                        .ToList();

                    WordedForecast = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("wordedForecast"))
                        .SelectMany(el => el.Elements("text"))
                        .Select(el => el.Value)
                        .ToList();

                    lastRefresh = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
