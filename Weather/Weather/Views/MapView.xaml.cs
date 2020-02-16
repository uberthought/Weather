using System;
using System.Threading;
using Weather.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Weather.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapView : ContentView
    {
        Pin pin;
        static readonly Distance DefaultRadius = Distance.FromMiles(10);

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
            var location = LocationService.Service.Location;

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

        private void Map_MapClicked(object sender, MapClickedEventArgs e)
        {
            SetPinPosition(e.Position);

            // disable UseDeviceLocation
            ((App)Application.Current).SetUseDeviceLocation(false);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            // enable UseDeviceLocation
            ((App)Application.Current).SetUseDeviceLocation(true);

            LocationService.Service.ResetLocation();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Parent == null && pin != null)
            {
                var location = new Location(pin.Position.Latitude, pin.Position.Longitude);
                LocationService.Service.SetLocation(location);
            }
        }
    }
}