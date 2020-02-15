using Xamarin.Forms;

namespace Weather.ViewModels
{
    public class ForecastCell
    {
        public int Index { get; set; }
        public string Label { get; set; }
        public ImageSource Icon { get; set; }
        public string Description { get; set; }
        public string TemperatureLabel { get; set; }
        public string Temperature { get; set; }
        public Color TemperatureColor { get; set; }
        public Color BackgroundColor { get; set; }
    }
}
