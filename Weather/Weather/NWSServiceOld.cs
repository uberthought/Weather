using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Weather
{
    class NWSServiceOld
    {
        static string baseAddress = "https://api.weather.gov";
        static string gridPointsUri = $"{baseAddress}/points/{{0}},{{1}}";
        HttpClient client;
        private double queryLatitude;
        private double queryLongitude;

        // station
        string stationId;
        string stationName;

        // current conditions
        double? temperature;
        DateTime? timestamp;
        string textDescription;
        double? windDirection;
        double? windSpeed;
        double? dewPoint;
        double? relativeHumidity;

        // grid point
        string gridWFO;
        int? gridX;
        int? gridY;

        public NWSServiceOld(double latitude, double longitude)
        {
            this.queryLatitude = latitude;
            this.queryLongitude = longitude;

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Weather app");
        }

        public async Task<string> GetStationName()
        {
            if (stationName == null)
                await GetStationFromLocation();
            return stationName;
        }

        public async Task<DateTime> GetTimestamp()
        {
            if (timestamp == null)
                await GetCurrentConditions();
            return timestamp.Value;
        }

        public async Task<double> GetTemperature()
        {
            if (temperature == null)
                await GetCurrentConditions();
            return temperature.Value;
        }

        public async Task<double> GetDewPoint()
        {
            if (dewPoint == null)
                await GetCurrentConditions();
            return dewPoint.Value;
        }

        public async Task<double> GetRelativeHumidity()
        {
            if (relativeHumidity == null)
                await GetCurrentConditions();
            return relativeHumidity.Value;
        }

        public async Task<string> GetTextDescription()
        {
            if (textDescription == null)
                await GetCurrentConditions();
            return textDescription;
        }

        public async Task<double> GetWindSpeed()
        {
            if (windSpeed == null)
                await GetCurrentConditions();
            return windSpeed.Value;
        }

        public async Task<double> GetWindDirection()
        {
            if (windDirection == null)
                await GetCurrentConditions();
            return windDirection.Value;
        }

        async Task GetStationFromLocation()
        {
            var uri = new Uri($"{baseAddress}/points/{queryLatitude},{queryLongitude}/stations");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);
                stationName = (string)jObject["features"][0]["properties"]["name"];
                stationId = (string)jObject["features"][0]["properties"]["stationIdentifier"];
            }
        }

        async Task GetStationFromGrid()
        {
            if (gridWFO == null || gridX == null || gridY == null)
                await GetGridPoint();

            var uri = new Uri($"{baseAddress}/gridpoints/{gridWFO}/{gridX},{gridY}/stations");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);

                var closestDistance = double.MaxValue;
                foreach (var feature in jObject["features"])
                {
                    var stationLatitude = (double)feature["geometry"]["coordinates"][1];
                    var stationLongitude = (double)feature["geometry"]["coordinates"][0];

                    var x = queryLatitude - stationLatitude;
                    var y = queryLongitude - stationLongitude;
                    var stationDistance = Math.Sqrt(x * x + y * y);
                    if (stationDistance < closestDistance)
                    {
                        closestDistance = stationDistance;
                        stationName = (string)feature["properties"]["name"];
                        stationId = (string)feature["properties"]["stationIdentifier"];
                    }
                }
            }
        }

        async Task GetCurrentConditions()
        {
            if (stationId == null)
                await GetStationFromLocation();
            //await GetStationFromGrid();

            // make network request
            var uri = new Uri($"{baseAddress}/stations/{stationId}/observations/latest?require_qc=false");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);

                var timestampString = (string)jObject["properties"]["timestamp"];
                timestamp = DateTime.Parse(timestampString);

                textDescription = (string)jObject["properties"]["textDescription"];
                dewPoint = (double)jObject["properties"]["dewpoint"]["value"];
                relativeHumidity = (double)jObject["properties"]["relativeHumidity"]["value"];
                temperature = (double)jObject["properties"]["temperature"]["value"];
                windDirection = (double)jObject["properties"]["windDirection"]["value"];
                windSpeed = (double)jObject["properties"]["windSpeed"]["value"];
            }
        }

        async Task GetGridPoint()
        {
            var uri = new Uri($"{baseAddress}/points/{queryLatitude},{queryLongitude}");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);
                gridWFO = (string)jObject["properties"]["cwa"];
                gridX = (int)jObject["properties"]["gridX"];
                gridY = (int)jObject["properties"]["gridY"];
            }
        }

        //async Task<string> GetForecast((string wfo, int x, int y) gridPoint)
        //{
        //    string forecast = "";

        //    // make network request
        //    var uri = new Uri($"{baseAddress}/gridpoints/{gridPoint.wfo}/{gridPoint.x},{gridPoint.y}");
        //    var response = await client.GetAsync(uri);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        // extract the needed data from the json string
        //        var content = await response.Content.ReadAsStringAsync();
        //        var jObject = JObject.Parse(content);
        //        //wfo = (string)jObject["properties"]["cwa"];
        //        //x = (int)jObject["properties"]["gridX"];
        //        //y = (int)jObject["properties"]["gridY"];
        //    }
        //    return forecast;
        //}
    }
}
