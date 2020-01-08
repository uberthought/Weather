using System;
using System.Collections.Generic;
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
            string wfo;
            int x;
            int y;

            // make network request
            var uri = new Uri($"{baseAddress}/points/{latitude},{longitude}");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            }

            // fake the data
            wfo = "TBW";
            x = 57;
            y = 96;

            return (wfo, x, y);
        }

        string GetForecast(string wfo, int x, int y)
        {
            string forecast = null;

            return forecast;
        }
    }
}
