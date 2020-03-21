using System.Collections.ObjectModel;
using System.Linq;
using Weather.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForecastView : ContentView
    {
        public ObservableCollection<string> Items { get; set; }

        public ForecastView()
        {
            InitializeComponent();

            BindingContext = new ForecastViewModel();
        }

        void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = (CollectionView)sender;

            if (!(e.CurrentSelection.FirstOrDefault() is ForecastCell selectedCell))
                return;

            var mainTabbedView = MainTabbedView.FindMainTabbedView(Parent);

            mainTabbedView.SetContentView(new DetailView(selectedCell.Index));

            collectionView.SelectedItem = null;
        }
    }
}
