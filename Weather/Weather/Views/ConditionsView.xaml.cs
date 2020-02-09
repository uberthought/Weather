using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather
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