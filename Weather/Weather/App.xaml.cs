using System;
using System.Threading;
using Weather.Pages;
using Weather.Services;
using Xamarin.Forms;

namespace Weather
{
    public partial class App : Application
    {
        public bool UseDeviceLocation { private set; get; } = true;

        Timer refreshTimer;

        public App()
        {
            InitializeComponent();

            if (Application.Current.Properties.ContainsKey("UseDeviceLocation"))
                UseDeviceLocation = (bool)Properties["UseDeviceLocation"];

            //MainPage = new MainPage();
            MainPage = new MainAdMobPage();
        }

        public async void SetUseDeviceLocation(bool useDeviceLocation)
        {
            UseDeviceLocation = useDeviceLocation;
            Properties["UseDeviceLocation"] = useDeviceLocation;
            await SavePropertiesAsync();
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

        private void Refresh()
        {
            if (UseDeviceLocation)
                LocationService.Service.ResetLocation();
            NWSServiceJSON.Service.Refresh();
        }

    }
}
