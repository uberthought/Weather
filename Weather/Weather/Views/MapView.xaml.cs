using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Weather
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapView : ContentView
    {
        Pin pin;
        static Distance DefaultRadius = Distance.FromMiles(10);

        public MapView()
        {
            InitializeComponent();

            Refresh();
        }

        async void Refresh()
        {
            var location = await App.GetLocation();
            var center = new Position(location.Latitude, location.Longitude);
            var mapSpan = MapSpan.FromCenterAndRadius(center, DefaultRadius);
            map.MoveToRegion(mapSpan);

            pin = new Pin() { Label = "", Position = center };
            map.Pins.Add(pin);

            LocationButton.IsVisible = !App.LocationNotAuthorized;
        }

        Timer delayTimer;

        private void map_MapClicked(object sender, MapClickedEventArgs e)
        {
            pin.Position = e.Position;

            var location = new Location();
            location.Latitude = e.Position.Latitude;
            location.Longitude = e.Position.Longitude;

            App.SetLocation(location);

            var radius = map.VisibleRegion?.Radius ?? DefaultRadius;
            var mapSpan = MapSpan.FromCenterAndRadius(e.Position, radius);
            map.MoveToRegion(mapSpan);

            SetNWSLocation(location);
        }

        private void DelayedSetLocation(object state)
        {
            var location = (Location)state;

            var nwsService = NWSService.GetService();
            nwsService.SetLocation(location.Latitude, location.Longitude);
        }

        private void SetNWSLocation(Location location)
        {
            if (delayTimer != null)
                delayTimer.Dispose();
            delayTimer = new Timer(DelayedSetLocation, location, TimeSpan.FromSeconds(5), TimeSpan.FromTicks(-1));
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await App.ResetLocation();
            var location = await App.GetLocation();

            App.SetLocation(location);

            var center = new Position(location.Latitude, location.Longitude);
            var radius = map.VisibleRegion?.Radius ?? DefaultRadius;
            var mapSpan = MapSpan.FromCenterAndRadius(center, radius);
            map.MoveToRegion(mapSpan);

            pin.Position = center;

            SetNWSLocation(location);
        }
    }
}