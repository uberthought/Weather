using Android.Content;
using Android.Gms.Ads;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Weather;
using Weather.Droid;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(AdMobView), typeof(AdMobViewRenderer))]
namespace Weather.Droid
{
    public class AdMobViewRenderer : ViewRenderer<AdMobView, AdView>
    {
        public AdMobViewRenderer(Context context) : base(context) { }

        AdView adView;

        private AdView CreateAdView()
        {
            if (Element.Width <= 0 || Element.Height <= 0)
                return null;
            //var adSize = AdSize.Banner;
            //var adSize = AdSize.SmartBanner;
            var adSize = new AdSize((int)Element.Width, (int)Element.Height);
            adView = new AdView(Context)
            {
                AdSize = adSize,
#if DEBUG
                AdUnitId = "ca-app-pub-3940256099942544/6300978111", // fake ad id
#else
                AdUnitId = "ca-app-pub-2856204202654056/3976768602", // real ad id
#endif
                LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };

            adView.LoadAd(new AdRequest.Builder().Build());

            return adView;
        }

        // This doesn't work with MasterDetailPage
        //protected override void OnElementChanged(ElementChangedEventArgs<AdMobView> e)
        //{
        //    base.OnElementChanged(e);

        //    if (e.NewElement != null && Control == null)
        //        SetNativeControl(CreateAdView());
        //}

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            // wait until the view has been sized before creating the AdView
            if ((e.PropertyName == "Width" || e.PropertyName == "Height") && Element.Width > 1 && Element.Height > 1)
                SetNativeControl(CreateAdView());
        }
    }
}
