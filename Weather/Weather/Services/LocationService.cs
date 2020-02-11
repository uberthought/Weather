using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Weather
{
    class LocationService
    {
        public event EventHandler LocationChanged;
        public event EventHandler AuthorizedChanged;
        public Location Location { private set; get; }
        public bool LocationAuthorized => !LocationNotAuthorized;


        static Location DefaultLocation = new Location(38.4815847, -100.568576);
        bool LocationNotAuthorized;

        static LocationService service;
        static object serviceLock = new object();
        public static LocationService Service
        {
            get
            {
                lock (serviceLock)
                    if (service == null)
                        service = new LocationService();
                return service;
            }
        }

        private LocationService()
        {
            // get the stored location or use the default if there isn't one
            if (Application.Current.Properties.ContainsKey("Latitude") && Application.Current.Properties.ContainsKey("Longitude"))
            {
                var latitude = (double)Application.Current.Properties["Latitude"];
                var longitude = (double)Application.Current.Properties["Longitude"];
                var location = new Location(latitude, longitude);
                SetLocation(location);
            }
            else
                SetLocation(DefaultLocation);

            if (((App)Application.Current).UseDeviceLocation)
                ResetLocation();
        }

        public async void SetLocation(Location location)
        {
            // if the location actually changed, then notify listeners and save it
            if (Location == null || location.Latitude != Location.Latitude || location.Longitude != Location.Longitude)
            {
                Location = location;
                if (LocationChanged != null)
                    LocationChanged.Invoke(this, EventArgs.Empty);

                Application.Current.Properties["Latitude"] = location.Latitude;
                Application.Current.Properties["Longitude"] = location.Longitude;
                await Application.Current.SavePropertiesAsync();
            }
        }

        public async void ResetLocation()
        {
            // try to get the device location and set it; if we can't do nothing
            Location location = null;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!LocationNotAuthorized)
                {
                    bool repeat = true;
                    var started = DateTime.UtcNow;
                    while (repeat && DateTime.UtcNow - started < TimeSpan.FromSeconds(30))
                    {
                        try
                        {
                            location = await Geolocation.GetLastKnownLocationAsync();
                            if (location == null)
                            {
                                var request = new GeolocationRequest(GeolocationAccuracy.Lowest);
                                location = await Geolocation.GetLocationAsync(request);
                            }
                            LocationNotAuthorized = false;
                            if (AuthorizedChanged != null)
                                AuthorizedChanged.Invoke(this, EventArgs.Empty);
                            repeat = false;
                        }
                        catch (Exception ex)
                        {
                            LocationNotAuthorized = true;
                            if (AuthorizedChanged != null)
                                AuthorizedChanged.Invoke(this, EventArgs.Empty);
                            repeat = ex is FeatureNotEnabledException;
                        }
                        if (repeat)
                            await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            });

            if (location != null)
                SetLocation(location);
        }
    }
}
