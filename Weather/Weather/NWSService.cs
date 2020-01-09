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
    class NWSService
    {
        static string baseAddress = "https://api.weather.gov";
        static string gridPointsUri = $"{baseAddress}/points/{{0}},{{1}}";
        HttpClient client = new HttpClient();

        public async Task<(string wfo, int x, int y)> GetGridPoints(float latitude, float longitude)
        {
            string wfo = "";
            int x = 0;
            int y = 0;

            // make network request
            var uri = new Uri($"{baseAddress}/points/{latitude},{longitude}");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject= JObject.Parse(content);
                wfo = (string)jObject["properties"]["cwa"];
                x = (int)jObject["properties"]["gridX"];
                y = (int)jObject["properties"]["gridY"];
            }

            return (wfo, x, y);
        }

        public async Task<string> GetCurrentConditions((string wfo, int x, int y) gridPoint)
        {
            string conditons = "";

            // make network request
            var uri = new Uri($"{baseAddress}/gridpoints/{gridPoint.wfo}/{gridPoint.x},{gridPoint.y}");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);
                //wfo = (string)jObject["properties"]["cwa"];
                //x = (int)jObject["properties"]["gridX"];
                //y = (int)jObject["properties"]["gridY"];
            }

            return conditons;
        }

        string GetForecast((string wfo, int x, int y) gridPoint)
        {
            string forecast = "";

            return forecast;
        }
    }
}
