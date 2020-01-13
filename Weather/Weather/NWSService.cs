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
        double queryLatitude;
        double queryLongitude;

        string location;

        // current conditions
        double? temperature;
        DateTime? timestamp;
        string textDescription;
        double? windDirection;
        double? windSpeed;
        double? dewPoint;
        double? relativeHumidity;
        string conditionIconUrl;

        // forecast
        List<string> forecastLabels;
        List<string> forecastIcons;
        List<string> forecastDescriptions;
        bool? isForecastDay;
        List<double> forecastLows;
        List<double> forecastHighs;

        public NWSService(double latitude, double longitude)
        {
            this.queryLatitude = latitude;
            this.queryLongitude = longitude;

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Weather app");
        }

        public async Task<string> GetLocation()
        {
            if (location == null)
                await Refresh();
            return location;
        }

        public async Task<DateTime> GetTimestamp()
        {
            if (timestamp == null)
                await Refresh();
            return timestamp.GetValueOrDefault();
        }

        public async Task<double> GetTemperature()
        {
            if (temperature == null)
                await Refresh();
            return temperature.GetValueOrDefault();
        }

        public async Task<double> GetDewPoint()
        {
            if (dewPoint == null)
                await Refresh();
            return dewPoint.GetValueOrDefault();
        }

        public async Task<double> GetRelativeHumidity()
        {
            if (relativeHumidity == null)
                await Refresh();
            return relativeHumidity.GetValueOrDefault();
        }

        public async Task<string> GetTextDescription()
        {
            if (textDescription == null)
                await Refresh();
            return textDescription;
        }

        public async Task<double> GetWindSpeed()
        {
            if (windSpeed == null)
                await Refresh();
            return windSpeed.GetValueOrDefault();
        }

        public async Task<double> GetWindDirection()
        {
            if (windDirection == null)
                await Refresh();
            return windDirection.GetValueOrDefault();
        }

        public async Task<string> GetConditionsIconUrl()
        {
            if (conditionIconUrl == null)
                await Refresh();
            return conditionIconUrl;
        }

        public async Task<List<string>> GetForecastLabels()
        {
            if (forecastLabels == null)
                await Refresh();
            return forecastLabels;
        }

        public async Task<List<string>> GetForecastIcons()
        {
            if (forecastIcons == null)
                await Refresh();
            return forecastIcons;
        }

        public async Task<List<string>> GetForecastDescriptions()
        {
            if (forecastDescriptions == null)
                await Refresh();
            return forecastDescriptions;
        }

        public async Task<bool> GetIsForecastDay()
        {
            if (isForecastDay == null)
                await Refresh();
            return isForecastDay.GetValueOrDefault();
        }

        public async Task<List<double>> GetForecastLows()
        {
            if (forecastLows == null)
                await Refresh();
            return forecastLows;
        }

        public async Task<List<double>> GetForecastHighs()
        {
            if (forecastHighs == null)
                await Refresh();
            return forecastHighs;
        }

        async Task Refresh()
        {
            try
            {
                // https://forecast.weather.gov/MapClick.php?lat=27.9789&lon=-82.7658&FcstType=dwml
                var uri = new Uri($"{baseAddress}/?lat={queryLatitude}&lon={queryLongitude}&FcstType=dwml");
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    // extract the needed data from the json string
                    var content = await response.Content.ReadAsStringAsync();
                    var root = XElement.Parse(content);

                    // current conditions

                    var current = from el in root.Elements("data")
                                  where (string)el.Attribute("type") == "current observations"
                                  select el;
                    location = current.Elements("location")
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

                    textDescription = current.Elements("parameters")
                        .SelectMany(el => el.Elements("weather"))
                        .SelectMany(el => el.Elements("weather-conditions"))
                        .Select(el => el.Attribute("weather-summary"))
                        .FirstOrDefault().Value;
                    if (textDescription == "NA")
                        textDescription = "";

                    var windSpeedString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("wind-speed"))
                        .Where(el => (string)el.Attribute("type") == "sustained")
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    var windDirectionString = current.Elements("parameters")
                        .SelectMany(el => el.Elements("direction"))
                        .SelectMany(el => el.Elements("value"))
                        .FirstOrDefault().Value;

                    conditionIconUrl = current.Elements("parameters")
                        .SelectMany(el => el.Elements("conditions-icon"))
                        .SelectMany(el => el.Elements("icon-link"))
                        .FirstOrDefault().Value;
                    if (conditionIconUrl == "NULL")
                        conditionIconUrl = "";
                    else
                    {
                        conditionIconUrl = conditionIconUrl.Replace("http://", "https://");
                        //conditionIconUrl = conditionIconUrl.Replace("medium", "large");
                    }

                    timestamp = DateTime.Parse(timestampString);
                    temperature = double.Parse(temperatureString);
                    dewPoint = double.Parse(dewPointString);
                    relativeHumidity = double.Parse(relativeHumidityString);
                    if (windSpeedString == "NA")
                        windSpeed = 0;
                    else
                        windSpeed = double.Parse(windSpeedString) * 1.15077945;
                    if (windDirectionString == "NA")
                        windDirection = 0;
                    else
                        windDirection = double.Parse(windDirectionString);

                    // forecast

                    var forecast = from el in root.Elements("data")
                                   where (string)el.Attribute("type") == "forecast"
                                   select el;

                    forecastLabels = forecast.Elements("time-layout")
                        .SelectMany(el => el.Elements("start-valid-time"))
                        .Select(el => el.Attribute("period-name"))
                        .Select(el => el.Value)
                        .Take(14)
                        .ToList();

                    forecastIcons = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("conditions-icon"))
                        .SelectMany(el => el.Elements("icon-link"))
                        .Select(el => el.Value)
                        .Select(s => s.Replace("http://", "https://"))
                        .ToList();

                    forecastDescriptions = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("weather"))
                        .SelectMany(el => el.Elements("weather-conditions"))
                        .Select(el => el.Attribute("weather-summary"))
                        .Select(el => el.Value)
                        .ToList();

                    isForecastDay = (forecastLabels[1] == "Tonight");

                    forecastLows = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("temperature"))
                        .Where(el => (string)el.Attribute("type") == "minimum")
                        .SelectMany(el => el.Elements("value"))
                        .Select(el => el.Value)
                        .Select(s => double.Parse(s))
                        .ToList();

                    forecastHighs = forecast.Elements("parameters")
                        .SelectMany(el => el.Elements("temperature"))
                        .Where(el => (string)el.Attribute("type") == "maximum")
                        .SelectMany(el => el.Elements("value"))
                        .Select(el => el.Value)
                        .Select(s => double.Parse(s))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
