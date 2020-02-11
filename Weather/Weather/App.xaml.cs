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
        Timer refreshTimer;

        public App()
        {
            InitializeComponent();

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

        public void Refresh()
        {
            LocationService.Service.ResetLocation();
        }

    }
}
