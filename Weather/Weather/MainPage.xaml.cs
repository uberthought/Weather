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
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MapPage());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var mainViewModel = BindingContext as MainViewModel;

            if (mainViewModel != null && !mainViewModel.IsRefreshing)
                mainViewModel.Refresh();
        }
    }
}
