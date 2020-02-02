﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Weather
{
    class CurrentConditionsViewModel
    {
        public CurrentConditionsViewModel()
        {
            var nwsService = NWSService.GetService();

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
            Temperature = $"{nwsService.Temperature:0}℉";
            DewPoint = $"Dew Point {nwsService.DewPoint:0}℉";
            RelativeHumidity = $"Humidity {nwsService.RelativeHumidity:0}%";
            Wind = $"Wind {windDirection} {nwsService.WindSpeed:0} MPH";
            if (!string.IsNullOrEmpty(nwsService.ConditionIconUrl))
                ConditionsIcon = ImageSource.FromUri(new Uri(nwsService.ConditionIconUrl));
            Gust = $"Gust {nwsService.Gust:0} MPH";
            Visibility = $"Visibility {nwsService.Visibility:0} Miles";
            Pressure = $"Pressure {nwsService.Pressure} HG";
        }

        public string Location { private set; get; }
        public ImageSource ConditionsIcon { private set; get; }

        public string TextDescription { private set; get; }
        public string Temperature { private set; get; }
        public string Wind { private set; get; }
        public string DewPoint { private set; get; }
        public string RelativeHumidity { private set; get; }
        public string Gust { private set; get; }
        public string Visibility { private set; get; }
        public string Pressure { private set; get; }
    }
}