using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather
{
    public partial class App : Application
    {
        static Location Location;
        static Task GetLocationTask;
        public static bool LocationNotAuthorized;
        public static Location DefaultLocation = new Location(38.4815847, -100.568576);

        DateTime LastLocationCheck = DateTime.MinValue;

        public App()
        {
            InitializeComponent();

            if (Application.Current.Properties.ContainsKey("Latitude") && Application.Current.Properties.ContainsKey("Longitude"))
            {
                var latitude = (double)Application.Current.Properties["Latitude"];
                var longitude = (double)Application.Current.Properties["Longitude"];
                Location = new Location(latitude, longitude);
            }
            else
                Location = DefaultLocation;

            //MainPage = new MainPage();
            MainPage = new MainAdMobPage();
        }

        protected override async void OnStart()
        {
            LastLocationCheck = DateTime.UtcNow;
            await ResetLocation();
            NWSService.GetService().SetLocation(Location.Latitude, Location.Longitude);
        }

        protected override void OnSleep()
        {
        }

        protected override async void OnResume()
        {
            if (!LocationNotAuthorized && DateTime.UtcNow - LastLocationCheck > TimeSpan.FromMinutes(15))
            {
                LastLocationCheck = DateTime.UtcNow;
                await ResetLocation();
                NWSService.GetService().SetLocation(Location.Latitude, Location.Longitude);
            }
        }

        public static async Task<Location> GetLocation()
        {
            if (GetLocationTask != null && !GetLocationTask.IsCompleted)
                await GetLocationTask;
            if (Location == null)
                Location = DefaultLocation;
            return Location;
        }

        public static async void SetLocation(Location location)
        {
            Location = location;
            Application.Current.Properties["Latitude"] = Location.Latitude;
            Application.Current.Properties["Longitude"] = Location.Longitude;
            await Application.Current.SavePropertiesAsync();
        }

        public static async Task ResetLocation()
        {
            Location location = null;

            GetLocationTask = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (!LocationNotAuthorized)
                    {
                        location = await Geolocation.GetLastKnownLocationAsync();
                        if (location == null)
                        {
                            var request = new GeolocationRequest(GeolocationAccuracy.Lowest);
                            location = await Geolocation.GetLocationAsync(request);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is System.UnauthorizedAccessException)
                        LocationNotAuthorized = true;
                }
            });

            await GetLocationTask;

            if (location != null)
                SetLocation(location);
        }
    }
}
