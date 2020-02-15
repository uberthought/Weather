using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Weather.Services;
using Xamarin.Forms;

namespace Weather.ViewModels
{
    class ForecastViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ForecastViewModel()
        {
            NWSService.Service.DataUpdated += NwsService_DataUpdated;
        }

        private void NwsService_DataUpdated(object sender, EventArgs e)
        {
            ForecastCells = new List<ForecastCell>();

            var nwsService = NWSService.Service;

            if (nwsService.IsValid)
            {
                // update forecast
                for (var i = 0; i < nwsService.ForecastLabels.Count(); i++)
                {
                    var isLow = i % 2 != 0;
                    if (nwsService.ForecastLabels[0] == "Tonight")
                        isLow = !isLow;

                    ForecastCells.Add(new ForecastCell
                    {
                        Index = i,
                        Label = nwsService.ForecastLabels[i],
                        Icon = ImageSource.FromUri(new Uri(nwsService.ForecastIcons[i])),
                        Description = nwsService.ForecastDescriptions[i],
                        TemperatureLabel = isLow ? "Low:" : "Hi:",
                        Temperature = $"{nwsService.ForecastTemperatures[i]:F0}℉",
                        TemperatureColor = isLow ? Color.Blue : Color.Red,
                        BackgroundColor = isLow ? Color.DarkGray : Color.LightBlue
                    });
                }
            }

            // tell UI to update
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastCells)));
        }

        public List<ForecastCell> ForecastCells { private set; get; }
    }
}
