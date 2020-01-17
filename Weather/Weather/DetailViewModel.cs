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
            var nwsService = NWSService.GetService();

            var isLow = index % 2 != 0;
            if (nwsService.ForecastLabels[0] == "Tonight")
                isLow = !isLow;
            var temperature = isLow ? nwsService.ForecastLows[(int)(index / 2)] : nwsService.ForecastHighs[(int)(index / 2)];
            var iconUrl = nwsService.ForecastIcons[index].Replace("medium", "large");

            Label = nwsService.ForecastLabels[index];
            Icon = ImageSource.FromUri(new Uri(iconUrl));
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
