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

        public ForecastViewModel()
        {
            var nwsService = NWSService.GetService();
            nwsService.DataUpdated += NwsService_DataUpdated;
        }

        private void NwsService_DataUpdated(object sender, EventArgs e)
        {
            var nwsService = (NWSService)sender;

            if (!nwsService.IsValid)
            {
                forecastCells = null;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastCells)));

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

        List<ForecastCell> forecastCells;
        public List<ForecastCell> ForecastCells { get { return forecastCells; } }
    }
}
