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
        private string lastLabelText;
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
            BindingContext = this;

            var stackLayout = new StackLayout()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = 0,
                Spacing = 0
            };

            // add labels
            var labelStackLayout = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Color.LightBlue,
                Padding = 0,
                Spacing = 0
            };
            stackLayout.Children.Add(labelStackLayout);
            for (var i = 0; i < Views.Count(); i++)
            {
                var text = Views.Keys.ElementAt(i);
                var label = new Label
                {
                    Text = text,
                    HorizontalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Color.LightBlue,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = new Thickness(0, 8)
                };
                var gesture = new TapGestureRecognizer();
                gesture.Tapped += Label_Clicked;
                label.GestureRecognizers.Add(gesture);
                labelStackLayout.Children.Add(label);
            }

            // add empty content view
            mainContentView = new ContentView()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = 0,
            };
            stackLayout.Children.Add(mainContentView);

            // add first page
            mainContentView.Content = Views.Values.FirstOrDefault();
            lastLabelText = Views.Keys.FirstOrDefault();

            Content = new ContentView { Content = stackLayout };
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
            mainContentView.Content = Views[lastLabelText];
            if ((mainContentView?.Content as ContentView)?.Content is CollectionView collectionView && selectedItem != null)
                collectionView.ScrollTo(selectedItem, position: ScrollToPosition.Center, animate: false);
        }

        private void Label_Clicked(object sender, EventArgs e)
        {
            var label = (Label)sender;

            if (label.Text != lastLabelText)
            {
                mainContentView.Content = Views[label.Text];
                lastLabelText = label.Text;
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