using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading;
using Xamarin.Essentials;

namespace Weather.ViewModels
{
    class ForecastViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Timer timer;

        public ForecastViewModel()
        {
            // setup Refresh button binding
            RefreshCommand = new Command(execute: Refresh);

            // execute a refresh command now and every hour
            timer = new Timer((o) =>
            {
                MainThread.BeginInvokeOnMainThread(() => { RefreshCommand.Execute(null); });
            }, null, TimeSpan.Zero , TimeSpan.FromHours(1));

        }

        public async void Refresh()
        {
            if (isRefreshing)
                return;
            isRefreshing = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));

            try
            {
                // get the device's location
                var location = await App.GetLocation();

                // create NWS request object and get forecast
                var nwsService = NWSService.GetService();
                await nwsService.SetLocation(location.Latitude, location.Longitude);

                if (!nwsService.IsValid)
                {
                    forecastCells = null;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastCells)));

                    isRefreshing = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));

                    return;
                }

                // update forecast
                var forecastCount = new List<int>
                        {
                            nwsService.ForecastLabels.Count(),
                            nwsService.ForecastIcons.Count(),
                            nwsService.ForecastDescriptions.Count(),
                            nwsService.ForecastLows.Count() * 2,
                            nwsService.ForecastHighs.Count() * 2
                        }.Min();

                forecastCells = new List<ForecastCell>();
                for (var i = 0; i < forecastCount; i++)
                {
                    var isLow = i % 2 != 0;
                    if (nwsService.ForecastLabels[0] == "Tonight")
                        isLow = !isLow;
                    var temperature = isLow ? nwsService.ForecastLows[(int)(i / 2)] : nwsService.ForecastHighs[(int)(i / 2)];

                    forecastCells.Add(new ForecastCell
                    {
                        Index = i,
                        Label = nwsService.ForecastLabels[i],
                        Icon = ImageSource.FromUri(new Uri(nwsService.ForecastIcons[i])),
                        Description = nwsService.ForecastDescriptions[i],
                        TemperatureLabel = isLow ? "Low:" : "Hi:",
                        Temperature = $"{temperature:0}℉",
                        TemperatureColor = isLow ? Color.Blue : Color.Red,
                        BackgroundColor = isLow ? Color.DarkGray : Color.LightBlue
                    });
                }

                // tell UI to update
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastCells)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            isRefreshing = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));
        }

        public ICommand RefreshCommand { private set; get; }

        List<ForecastCell> forecastCells;
        public List<ForecastCell> ForecastCells { get { return forecastCells; } }

        bool isRefreshing;
        public bool IsRefreshing { get => isRefreshing; }
    }
}
