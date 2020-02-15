﻿using System;
using System.ComponentModel;
using Weather.Services;
using Xamarin.Forms;

namespace Weather.ViewModels
{
    public class TodayViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public TodayViewModel()
        {
            NWSServiceJSON.Service.DataUpdated += NwsService_DataUpdated;
        }

        private void NwsService_DataUpdated(object sender, EventArgs e)
        {
            Location = "Forecast Unavailable";
            Updated = null;
            Temperature = null;
            DewPoint = null;
            TextDescription = null;
            RelativeHumidity = null;
            Wind = null;
            ConditionsIcon = null;

            ForecastLabel = null;
            ForecastIcon = null;
            ForecastDescription = null;
            ForecastTemperatureLabel = null;
            ForecastTemperatureColor = Color.Blue;
            ForecastTemperature = null;
            ForecastDetailText = null;

            var nwsService = NWSServiceJSON.Service;

            if (nwsService.IsValid)
            {
                var windDirection = "";
                if (nwsService.WindDirection > 337.5 || nwsService.WindDirection <= 22.5)
                    windDirection = "N";
                else if (nwsService.WindDirection > 22.5 && nwsService.WindDirection <= 67.5)
                    windDirection = "NE";
                else if (nwsService.WindDirection > 67.5 && nwsService.WindDirection <= 112.5)
                    windDirection = "E";
                else if (nwsService.WindDirection > 112.5 && nwsService.WindDirection <= 157.5)
                    windDirection = "SE";
                else if (nwsService.WindDirection > 157.5 && nwsService.WindDirection <= 202.5)
                    windDirection = "S";
                else if (nwsService.WindDirection > 202.5 && nwsService.WindDirection <= 247.5)
                    windDirection = "SW";
                else if (nwsService.WindDirection > 247.5 && nwsService.WindDirection <= 292.5)
                    windDirection = "W";
                else if (nwsService.WindDirection > 292.5 && nwsService.WindDirection <= 337.5)
                    windDirection = "NW";

                // update current condition
                Updated = "Forecast Updated: " + nwsService.Timestamp?.ToString("dd MMM hh:mm tt");
                Location = nwsService.Location;
                TextDescription = nwsService.TextDescription;
                Temperature = $"{nwsService.Temperature:F0}℉";
                DewPoint = $"Dew Point {nwsService.DewPoint:F0}℉";
                RelativeHumidity = $"Humidity {nwsService.RelativeHumidity:F0}%";
                Wind = $"Wind {windDirection} {nwsService.WindSpeed:F1} MPH";
                if (!string.IsNullOrEmpty(nwsService.ConditionIconUrl))
                    ConditionsIcon = ImageSource.FromUri(new Uri(nwsService.ConditionIconUrl));

                // update forecast
                var isLow = nwsService.ForecastLabels[0] == "Tonight";
                ForecastLabel = nwsService.ForecastLabels[0];
                ForecastIcon = ImageSource.FromUri(new Uri(nwsService.ForecastIcons[0]));
                ForecastDescription = nwsService.ForecastDescriptions[0];
                ForecastTemperatureLabel = isLow ? "Low:" : "Hi:";
                ForecastTemperatureColor = isLow ? Color.Blue : Color.Red;
                ForecastTemperature = $"{nwsService.ForecastTemperatures[0]:F0}℉";
                ForecastDetailText = nwsService.WordedForecast[0];
            }

            // tell UI to update
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updated)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DewPoint)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelativeHumidity)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wind)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConditionsIcon)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastIcon)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastTemperatureLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastTemperatureColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastTemperature)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ForecastDetailText)));
        }

        public String Updated { private set; get; }

        // current conditions

        public String Location { private set; get; }
        public string Temperature { private set; get; }
        public string DewPoint { private set; get; }
        public string TextDescription { private set; get; }
        public string RelativeHumidity { private set; get; }
        public string Wind { private set; get; }
        public ImageSource ConditionsIcon { private set; get; }

        // forecast
        public string ForecastLabel { private set; get; }
        public ImageSource ForecastIcon { private set; get; }
        public string ForecastDescription { private set; get; }
        public string ForecastTemperatureLabel { private set; get; }
        public string ForecastTemperature { private set; get; }
        public Color ForecastTemperatureColor { private set; get; }
        public string ForecastDetailText { private set; get; }
    }
}
