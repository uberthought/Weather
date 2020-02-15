using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainTabbedView : ContentView
    {
        private string lastButtonText;
        private readonly ContentView mainContentView;

        private readonly Dictionary<string, ContentView> Views = new Dictionary<string, ContentView>
        {
            { "Today", new TodayView() },
            { "Conditions", new ConditionsView() },
            { "Forecast", new ForecastView() },
            { "Location", new MapView() }
        };

        public MainTabbedView()
        {
            // setup grid
            var grid = new Grid
            {
                ColumnSpacing = 0,
                RowSpacing = 0,
                RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = 60 },
                        new RowDefinition { Height = GridLength.Auto }
                    },
            };

            // add buttons
            for (var i = 0; i < Views.Count(); i++)
            {
                var buttonText = Views.Keys.ElementAt(i);
                var button = new Button
                {
                    Text = buttonText,
                    BorderWidth = 0,
                    Padding = 0,
                };
                button.Clicked += Button_Clicked;
                grid.Children.Add(button, i, 0);
            }

            // add empty content view
            mainContentView = new ContentView();
            grid.Children.Add(mainContentView, 0, 4, 1, 3);

            // add first page
            mainContentView.Content = Views.Values.FirstOrDefault();
            lastButtonText = Views.Keys.FirstOrDefault();

            Content = new ContentView { Content = grid };

            BindingContext = this;
        }

        object selectedItem;

        public void SetContentView(ContentView contentView)
        {
            if ((mainContentView?.Content as ContentView)?.Content is CollectionView collectionView)
                selectedItem = collectionView.SelectedItem;
            mainContentView.Content = contentView;
        }

        public void ResetContentView()
        {
            mainContentView.Content = Views[lastButtonText];
            if ((mainContentView?.Content as ContentView)?.Content is CollectionView collectionView && selectedItem != null)
                collectionView.ScrollTo(selectedItem, position: ScrollToPosition.Center, animate: false);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.Text != lastButtonText)
            {
                mainContentView.Content = Views[button.Text];
                lastButtonText = button.Text;
            }
        }

        public static MainTabbedView FindMainTabbedView(Element parent)
        {
            MainTabbedView mainTabbedView = null;
            while (mainTabbedView == null && parent != null)
            {
                mainTabbedView = parent as MainTabbedView;
                parent = parent.Parent;
            }
            return mainTabbedView;
        }
    }
}