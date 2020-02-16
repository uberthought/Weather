using System;
using System.ComponentModel;
using Weather.Services;
using Xamarin.Forms;

namespace Weather.ViewModels
{
    class ConditionsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ConditionsViewModel()
        {
            NWSService.Service.DataUpdated += NwsService_DataUpdated;
        }

        void NwsService_DataUpdated(object sender, EventArgs e)
        {
            Location = "Current Condition Unavailable";
            TextDescription = null;
            Temperature = null;
            DewPoint = null;
            RelativeHumidity = null;
            Wind = null;
            ConditionsIcon = null;
            Gust = null;
            Visibility = null;
            Pressure = null;
            WindChill = null;
            HeatIndex = null;

            var nwsService = NWSService.Service;

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
                Location = nwsService.Location;
                TextDescription = nwsService.TextDescription;
                Temperature = $"{nwsService.Temperature:F0}℉";
                DewPoint = $"Dew Point {nwsService.DewPoint:F0}℉";
                RelativeHumidity = $"Humidity {nwsService.RelativeHumidity:F0}%";
                Wind = $"Wind {windDirection} {nwsService.WindSpeed:F1} MPH";
                if (!string.IsNullOrEmpty(nwsService.ConditionIconUrl))
                    ConditionsIcon = ImageSource.FromUri(new Uri(nwsService.ConditionIconUrl));

                ShowGust = nwsService.WindGust.HasValue;
                Gust = $"Gust {nwsService.WindGust:F1} MPH";
        
                ShowVisibility = nwsService.Visibility.HasValue;
                Visibility = $"Visibility {nwsService.Visibility:F1} Miles";

                ShowPressure = nwsService.Pressure.HasValue;
                Pressure = $"Pressure {nwsService.Pressure:F2} HG";

                ShowWindChill = nwsService.WindChill.HasValue;
                WindChill = $"Wind Chill {nwsService.WindChill:F0}℉";

                ShowHeatIndex = nwsService.HeatIndex.HasValue;
                HeatIndex = $"Heat Index {nwsService.HeatIndex:F0}℉";
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Location)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temperature)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DewPoint)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RelativeHumidity)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wind)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConditionsIcon)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowGust)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Gust)));
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visibility)));
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowPressure)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pressure)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowWindChill)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindChill)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowHeatIndex)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeatIndex)));
        }

        public string Location { private set; get; }
        public ImageSource ConditionsIcon { private set; get; }

        public string TextDescription { private set; get; }
        public string Temperature { private set; get; }
        public string Wind { private set; get; }
        public string DewPoint { private set; get; }
        public string RelativeHumidity { private set; get; }
        public bool ShowGust { private set; get; }
        public string Gust { private set; get; }
        
        public bool ShowVisibility { private set; get; }
        public string Visibility { private set; get; }

        public bool ShowPressure { private set; get; }
        public string Pressure { private set; get; }

        public bool ShowWindChill { private set; get; }
        public string WindChill { private set; get; }

        public bool ShowHeatIndex { private set; get; }
        public string HeatIndex { private set; get; }
    }
}
