using System;
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
        static Location DefaultLocation = new Location(38.4815847, -100.568576);

        public App()
        {
            InitializeComponent();

            if (Application.Current.Properties.ContainsKey("DefaultLocation"))
                DefaultLocation = Application.Current.Properties["DefaultLocation"] as Location;

            MainPage = new MasterDetailPageDetail();
        }

        protected override async void OnStart()
        {
            await ResetLocation();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static async Task<Location> GetLocation()
        {
            await GetLocationTask;
            return Location;
        }

        public static void SetLocation(Location location)
        {
            DefaultLocation = location;
            Application.Current.Properties["DefaultLocation"] = Location;
            Location = location;
        }

        public static async Task<Location> ResetLocation()
        {
            GetLocationTask = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (LocationNotAuthorized)
                        Location = DefaultLocation;
                    else
                        Location = await Geolocation.GetLocationAsync();
                }
                catch (Exception ex)
                {
                    if (ex is System.UnauthorizedAccessException)
                        LocationNotAuthorized = true;
                    Location = DefaultLocation;
                }
            });

            await GetLocationTask;
            Application.Current.Properties["DefaultLocation"] = Location;
            return Location;
        }
    }
}
