using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Weather.Services
{
    class NWSService
    {
        static readonly string baseAddress = "https://api.weather.gov";
        readonly HttpClient client;
        DateTime lastRefresh = DateTime.MinValue;
        void NWSService_LocationChanged(object sender, EventArgs e) => UpdateLocation();

        public bool IsValid => Location != null;

        public event EventHandler DataUpdated;

        public double QueryLatitude { private set; get; }
        public double QueryLongitude { private set; get; }

        public string Location { private set; get; }

        // current conditions
        public double? Temperature { private set; get; }
        public double? WindChill { private set; get; }
        public double? HeatIndex { private set; get; }
        public DateTime? Timestamp { private set; get; }
        public string TextDescription { private set; get; }
        public double? WindDirection { private set; get; }
        public double? WindSpeed { private set; get; }
        public double? DewPoint { private set; get; }
        public double? RelativeHumidity { private set; get; }
        public string ConditionIconUrl { private set; get; }
        public double? Visibility { private set; get; }
        public double? WindGust { private set; get; }
        public double? Pressure { private set; get; }

        // forecast
        public List<string> ForecastLabels { private set; get; }
        public List<string> ForecastIcons { private set; get; }
        public List<string> ForecastDescriptions { private set; get; }
        public List<double> ForecastTemperatures { private set; get; }
        public List<string> WordedForecast { private set; get; }

        static NWSService service;
        static readonly object serviceLock = new object();
        public static NWSService Service
        {
            get
            {
                lock (serviceLock)
                    if (service == null)
                        service = new NWSService();
                return service;
            }
        }

        // station
        string StationId;

        // grid point
        string GridWFO;
        int? GridX;
        int? GridY;

        NWSService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Weather app");

            UpdateLocation();
            LocationService.Service.LocationChanged += NWSService_LocationChanged;
        }

        private void UpdateLocation()
        {
            var location = LocationService.Service.Location;

            if (QueryLatitude != location.Latitude || QueryLongitude != location.Longitude)
            {
                QueryLatitude = location.Latitude;
                QueryLongitude = location.Longitude;

                StationId = null;
                GridWFO = null;
                GridX = null;
                GridY = null;

                Location = null;
                Temperature = null;
                WindChill = null;
                HeatIndex = null;
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
                ForecastTemperatures = null;
                WordedForecast = null;

                lastRefresh = DateTime.MinValue;

                Refresh();
            }
        }

        readonly SemaphoreSlim refreshLock = new SemaphoreSlim(1);
        public async void Refresh()
        {
            await refreshLock.WaitAsync();

            if (!IsValid || DateTime.UtcNow - lastRefresh > TimeSpan.FromMinutes(15))
            {
                try
                {
                    await GetCurrentConditions();
                    await GetForecast();

                    lastRefresh = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }

            refreshLock.Release();

            if (DataUpdated != null)
                DataUpdated.Invoke(this, EventArgs.Empty);
        }

    async Task GetStationFromLocation()
        {
            var uri = new Uri($"{baseAddress}/points/{QueryLatitude},{QueryLongitude}/stations");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);
                Location = (string)jObject["features"][0]["properties"]["name"];
                StationId = (string)jObject["features"][0]["properties"]["stationIdentifier"];
            }
        }

        async Task GetCurrentConditions()
        {
            if (StationId == null)
                await GetStationFromLocation();

            // make network request
            var uri = new Uri($"{baseAddress}/stations/{StationId}/observations/latest?require_qc=false");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);

                var timestampString = (string)jObject["properties"]["timestamp"];
                Timestamp = DateTime.Parse(timestampString);

                double.TryParse((string)jObject["properties"]["dewpoint"]["value"], out double dewpoint);
                double.TryParse((string)jObject["properties"]["relativeHumidity"]["value"], out double relativeHumidity);
                double.TryParse((string)jObject["properties"]["temperature"]["value"], out double temperature);
                double.TryParse((string)jObject["properties"]["windDirection"]["value"], out double windDirection);
                double.TryParse((string)jObject["properties"]["windSpeed"]["value"], out double windSpeed);
                if (double.TryParse((string)jObject["properties"]["windGust"]["value"], out double windGust))
                    WindGust = MetersPerSecondToMPH(windGust);
                if (double.TryParse((string)jObject["properties"]["barometricPressure"]["value"], out double barometricPressure))
                    Pressure = PaToHG(barometricPressure);
                if (double.TryParse((string)jObject["properties"]["visibility"]["value"], out double visibility))
                    Visibility = MToMiles(visibility);
                if (double.TryParse((string)jObject["properties"]["windChill"]["value"], out double windChill))
                    WindChill = CToF(windChill);
                if (double.TryParse((string)jObject["properties"]["heatIndex"]["value"], out double heatIndex))
                    HeatIndex = CToF(heatIndex);

                DewPoint = CToF(dewpoint);
                RelativeHumidity = relativeHumidity;
                Temperature = CToF(temperature);
                WindDirection = windDirection;
                WindSpeed = MetersPerSecondToMPH(windSpeed);

                TextDescription = (string)jObject["properties"]["textDescription"];
                ConditionIconUrl = (string)jObject["properties"]["icon"];
            }
        }

        double CToF(double celcius) => 9.0 / 5.0 * celcius + 32.0;
        double MetersPerSecondToMPH(double metersPerSecond) => metersPerSecond * 2.237;
        double PaToHG(double pascals) => pascals / 3386.0;
        double MToMiles(double meters) => meters / 1609.0;

        async Task GetGridPoint()
        {
            var uri = new Uri($"{baseAddress}/points/{QueryLatitude},{QueryLongitude}");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);
                GridWFO = (string)jObject["properties"]["cwa"];
                GridX = (int)jObject["properties"]["gridX"];
                GridY = (int)jObject["properties"]["gridY"];
            }
        }

        async Task GetForecast()
        {
            if (!GridX.HasValue || !GridY.HasValue || string.IsNullOrWhiteSpace(GridWFO))
                await GetGridPoint();

            // make network request
            var uri = new Uri($"{baseAddress}/gridpoints/{GridWFO}/{GridX},{GridY}/forecast");
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                // extract the needed data from the json string
                var content = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(content);

                ForecastLabels = new List<string>();
                ForecastIcons = new List<string>();
                ForecastDescriptions = new List<string>();
                ForecastTemperatures = new List<double>();
                WordedForecast = new List<string>();

                var periods = (JArray)jObject["properties"]["periods"];

                foreach (var period in periods)
                {
                    ForecastLabels.Add((string)period["name"]);
                    ForecastIcons.Add((string)period["icon"]);
                    ForecastDescriptions.Add((string)period["shortForecast"]);
                    WordedForecast.Add((string)period["detailedForecast"]);
                    double.TryParse((string)period["temperature"], out double temperature);
                    ForecastTemperatures.Add(temperature);
                }
            }
        }
    }
}
