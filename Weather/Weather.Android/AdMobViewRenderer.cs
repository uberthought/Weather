using Android.Content;
using Android.Gms.Ads;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Weather;
using Weather.Droid;

[assembly: ExportRenderer(typeof(AdMobView), typeof(AdMobViewRenderer))]
namespace Weather.Droid
{
    public class AdMobViewRenderer : ViewRenderer<AdMobView, AdView>
    {
        public AdMobViewRenderer(Context context) : base(context) { }

        private AdView CreateAdView()
        {
            var adView = new AdView(Context)
            {
                AdSize = AdSize.SmartBanner,
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

        protected override void OnElementChanged(ElementChangedEventArgs<AdMobView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && Control == null)
                SetNativeControl(CreateAdView());
        }
    }
}
