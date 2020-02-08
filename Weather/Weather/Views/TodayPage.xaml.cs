using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Weather
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class TodayPage : ContentPage
    {
        public TodayPage()
        {
            InitializeComponent();

            BindingContext = new TodayViewModel();

            var tapGesture = new TapGestureRecognizer() { Command = new Command(execute: async () =>
            {
                var nwsService = NWSService.GetService();

                if (nwsService.IsValid)
                    await Navigation.PushAsync(new CurrentConditionsPage());
            })};
            CurrentConditionsStackLayout.GestureRecognizers.Add(tapGesture);
        }

        async void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MapPage());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var todayViewModel = BindingContext as TodayViewModel;

            if (todayViewModel != null && !todayViewModel.IsRefreshing)
                todayViewModel.Refresh();
        }

        async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCell = e.CurrentSelection.FirstOrDefault() as ForecastCell;

            if (selectedCell == null)
                return;

            await Navigation.PushAsync(new DetailPage(selectedCell.Index));
        }
    }
}
