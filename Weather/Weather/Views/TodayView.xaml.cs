using System.ComponentModel;
using Xamarin.Forms;

namespace Weather.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class TodayView : ContentView
    {
        public TodayView()
        {
            InitializeComponent();

            //BindingContext = new TodayViewModel();
        }
    }
}
