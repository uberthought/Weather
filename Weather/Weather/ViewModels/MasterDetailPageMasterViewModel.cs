﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Weather
{
    public partial class MasterDetailPageMaster
    {
        class MasterDetailPageMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<MasterDetailViewMenuItem> MenuItems { get; set; }

            public MasterDetailPageMasterViewModel()
            {
                MenuItems = new ObservableCollection<MasterDetailViewMenuItem>(new[]
                {
                    new MasterDetailViewMenuItem { Id = 0, Title = "Today", TargetType=typeof(TodayView) },
                    new MasterDetailViewMenuItem { Id = 1, Title = "Current Conditions", TargetType=typeof(ConditionsView) },
                    new MasterDetailViewMenuItem { Id = 2, Title = "Forecast", TargetType=typeof(ForecastView) },
                    new MasterDetailViewMenuItem { Id = 3, Title = "Set Location", TargetType=typeof(MapView) },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}