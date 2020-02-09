using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForecastPage : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public ForecastPage()
        {
            InitializeComponent();

            BindingContext = new ViewModels.ForecastViewModel();
        }

        async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;
            var selectedCell = e.CurrentSelection.FirstOrDefault() as ForecastCell;

            if (selectedCell == null)
                return;

            await Navigation.PushModalAsync(new DetailPage(selectedCell.Index));

            collectionView.SelectedItem = null;
        }
    }
}
