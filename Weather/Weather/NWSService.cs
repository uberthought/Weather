using System;
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
            return timestamp.Value;
        }

        public async Task<double> GetTemperature()
        {
            if (temperature == null)
                await Refresh();
            return temperature.Value;
        }

        public async Task<double> GetDewPoint()
        {
            if (dewPoint == null)
                await Refresh();
            return dewPoint.Value;
        }

        public async Task<double> GetRelativeHumidity()
        {
            if (relativeHumidity == null)
                await Refresh();
            return relativeHumidity.Value;
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
            return windSpeed.Value;
        }

        public async Task<double> GetWindDirection()
        {
            if (windDirection == null)
                await Refresh();
            return windDirection.Value;
        }

        public async Task<string> GetConditionsIconUrl()
        {
            if (conditionIconUrl == null)
                await Refresh();
            return conditionIconUrl;
        }

        async Task Refresh()
        {
            // https://forecast.weather.gov/MapClick.php?lat=27.9789&lon=-82.7658&FcstType=dwml
            var uri = new Uri($"{baseAddress}/?lat={queryLatitude}&lon={queryLongitude}&FcstType=dwml");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var root = XElement.Parse(content);

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
            }
        }
    }
}
