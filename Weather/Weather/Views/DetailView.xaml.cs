using Weather.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailView : ContentView
    {
        public DetailView(int index)
        {
            InitializeComponent();

            BindingContext = new DetailViewModel(index);

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                var mainTabbedView = MainTabbedView.FindMainTabbedView(Parent);
                mainTabbedView.ResetContentView();
            };
            scrollView.GestureRecognizers.Add(tapGestureRecognizer);
        }
    }
}