using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace Weather
{
    class DetailViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DetailViewModel(int index)
        {
            // we will never get to this point without the service being populated, so don't bother with the callback
            var nwsService = NWSService.GetService();

            var isLow = index % 2 != 0;
            if (nwsService.ForecastLabels[0] == "Tonight")
                isLow = !isLow;
            var temperature = isLow ? nwsService.ForecastLows[(int)(index / 2)] : nwsService.ForecastHighs[(int)(index / 2)];

            Label = nwsService.ForecastLabels[index];
            Icon = ImageSource.FromUri(new Uri(nwsService.ForecastIcons[index]));
            Description = nwsService.ForecastDescriptions[index];
            TemperatureLabel = isLow ? "Low:" : "Hi:";
            TemperatureColor = isLow ? Color.Blue : Color.Red;
            Temperature = $"{temperature:0}℉";
            DetailText = nwsService.WordedForecast[index];
            BackgroundColor = isLow ? Color.DarkGray : Color.LightBlue;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TemperatureLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TemperatureColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DetailText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundColor)));
        }

        public string Label { private set; get; }
        public ImageSource Icon { private set; get; }
        public string Description { private set; get; }
        public string TemperatureLabel { private set; get; }
        public Color TemperatureColor { private set; get; }
        public string Temperature { private set; get; }
        public string DetailText { private set; get; }
        public Color BackgroundColor { private set; get; }
    }
}
