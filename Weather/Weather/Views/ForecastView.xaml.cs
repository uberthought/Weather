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
    public partial class ForecastView : ContentView
    {
        public ObservableCollection<string> Items { get; set; }

        public ForecastView()
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

            var mainTabbedView = MainTabbedView.FindMainTabbedView(Parent);

            mainTabbedView.SetContentView(new DetailView(selectedCell.Index));

            collectionView.SelectedItem = null;
        }
    }
}
