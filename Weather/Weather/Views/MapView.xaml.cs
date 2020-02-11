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
        Timer delayTimer;

        public MapView()
        {
            InitializeComponent();

            UpdateLocation();

            LocationService.Service.AuthorizedChanged += Service_AuthorizedChanged;
            LocationService.Service.LocationChanged += MapView_LocationChanged;
        }

        private void Service_AuthorizedChanged(object sender, EventArgs e) => LocationButton.IsVisible = LocationService.Service.LocationAuthorized;

        private void MapView_LocationChanged(object sender, EventArgs e) => UpdateLocation();

        private void UpdateLocation()
        {
            var locationService = LocationService.Service;
            var location = locationService.Location;

            var position = new Position(location.Latitude, location.Longitude);
            SetPinPosition(position);
        }

        private void SetPinPosition(Position position)
        {
            if (pin == null || pin.Position.Latitude != position.Latitude || pin.Position.Longitude != position.Longitude)
            {
                var radius = map.VisibleRegion?.Radius ?? DefaultRadius;

                var mapSpan = MapSpan.FromCenterAndRadius(position, radius);
                map.MoveToRegion(mapSpan);

                if (pin == null)
                {
                    pin = new Pin() { Label = "", Position = position };
                    map.Pins.Add(pin);
                }
                else
                    pin.Position = position;
            }
        }

        private void map_MapClicked(object sender, MapClickedEventArgs e)
        {
            SetPinPosition(e.Position);

            // if there's a delay timer already, remove it
            if (delayTimer != null)
            {
                delayTimer.Dispose();
                delayTimer = null;
            }

            var location = new Location(e.Position.Latitude, e.Position.Longitude);
            delayTimer = new Timer(DelayedSetLocation, location, TimeSpan.FromSeconds(5), TimeSpan.FromTicks(-1));
        }

        private void DelayedSetLocation(object state) => LocationService.Service.SetLocation((Location)state);

        private void Button_Clicked(object sender, EventArgs e) => LocationService.Service.ResetLocation();
    }
}