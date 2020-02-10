using System;
using System.Linq;
using System.Threading;
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
        public bool AppLocationNotAuthorized;
        public static Location DefaultLocation = new Location(38.4815847, -100.568576);

        Timer refreshTimer;
        Timer delayTimer;

        public static bool LocationNotAuthorized => ((App)Application.Current).AppLocationNotAuthorized;
        public static Task<Location> GetLocation() => ((App)Application.Current).AppGetLocation();
        public static void SetLocation(Location location) => ((App)Application.Current).AppSetLocation(location);
        public static Task ResetLocation() => ((App)Application.Current).AppResetLocation();

        public App()
        {
            InitializeComponent();

            // get the stored location or use the default if there isn't one
            if (Application.Current.Properties.ContainsKey("Latitude") && Application.Current.Properties.ContainsKey("Longitude"))
            {
                var latitude = (double)Application.Current.Properties["Latitude"];
                var longitude = (double)Application.Current.Properties["Longitude"];
                Location = new Location(latitude, longitude);
            }
            else
                Location = DefaultLocation;

            NWSService.GetService().SetLocation(Location.Latitude, Location.Longitude);

            //MainPage = new MainPage();
            MainPage = new MainAdMobPage();
        }

        protected override void OnStart()
        {
            StartRefreshTimer();
        }

        protected override void OnSleep()
        {
            StopRefreshTimer();
        }

        protected override void OnResume()
        {
            StartRefreshTimer();
        }

        private void StartRefreshTimer()
        {
            // execute a refresh command now and every hour
            refreshTimer = new Timer((o) => { Refresh(); }, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        private void StopRefreshTimer()
        {
            refreshTimer.Dispose();
            refreshTimer = null;
        }

        public async void Refresh()
        {
            // check for a location change
            var lastLocation = Location;
            await AppResetLocation();

            // if the location changed, the NWSService will be refreshed by AppResetLocation()
            // if it didn't change, we still need to refresh NWSService
            if (lastLocation.Latitude == Location.Latitude || lastLocation.Longitude == Location.Longitude)
                NWSService.GetService().Refresh();
        }

        private async Task<Location> AppGetLocation()
        {
            // if we're trying to get the device location, wait for it to complete
            if (GetLocationTask != null && !GetLocationTask.IsCompleted)
                await GetLocationTask;

            // if the location is still null, use the default location
            if (Location == null)
                Location = DefaultLocation;

            return Location;
        }

        private async void AppSetLocation(Location location, bool immediate = false)
        {
            // if the location actually changed, then save it and refresh the NWSService
            if (location.Latitude != Location.Latitude || location.Longitude != Location.Longitude)
            {
                Location = location;
                Application.Current.Properties["Latitude"] = Location.Latitude;
                Application.Current.Properties["Longitude"] = Location.Longitude;
                await Application.Current.SavePropertiesAsync();

                SetNWSLocation(Location, immediate);
            }
        }

        private void SetNWSLocation(Location location, bool immediate)
        {
            // if there's a delay timer already, remove it
            if (delayTimer != null)
                delayTimer.Dispose();

            // if we want it done immediately, then do so, otherwise start a delay timer to call DelayedSetLocation() after 5 seconds
            if (immediate)
                DelayedSetLocation(location);
            else
                delayTimer = new Timer(DelayedSetLocation, location, TimeSpan.FromSeconds(5), TimeSpan.FromTicks(-1));
        }

        private void DelayedSetLocation(object state)
        {
            var location = (Location)state;

            // set the NWSService location and tell it to refresh
            var nwsService = NWSService.GetService();
            nwsService.SetLocation(location.Latitude, location.Longitude);
            nwsService.Refresh();
        }

        private async Task AppResetLocation()
        {
            // try to get the device location and set it; if we can't do nothing
            Location location = null;

            GetLocationTask = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!AppLocationNotAuthorized)
                {
                    try
                    {
                        location = await Geolocation.GetLastKnownLocationAsync();
                        if (location == null)
                        {
                            var request = new GeolocationRequest(GeolocationAccuracy.Lowest);
                            location = await Geolocation.GetLocationAsync(request);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is System.UnauthorizedAccessException)
                            AppLocationNotAuthorized = true;
                    }
                }
            });

            await GetLocationTask;

            if (location != null)
                AppSetLocation(location, true);
        }
    }
}
