using Weather.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConditionsView : ContentView
    {
        public ConditionsView()
        {
            InitializeComponent();

            BindingContext = new ConditionsViewModel();
        }
    }
}