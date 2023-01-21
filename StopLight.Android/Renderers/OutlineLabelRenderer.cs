using Android.Content;
using StopLight.Elements;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using StopLight.Droid.Renderers;
using StopLight.Droid.Elements;

[assembly: ExportRenderer(typeof(OutlineLabel), typeof(OutlineLabelRenderer))]
namespace StopLight.Droid.Renderers
{
    public class OutlineLabelRenderer : LabelRenderer
    {
        Context context;
        public OutlineLabelRenderer(Context context) : base(context)
        {
            this.context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            OutlineLabel customLabel = (OutlineLabel)Element;
            var StrokeTextViewColor = customLabel.StrokeColor;

            int StrokeThickness = customLabel.StrokeThickness;
            if (Control != null)
            {

                StrokeTextView strokeTextView = new StrokeTextView(context, Control.TextSize, StrokeTextViewColor, StrokeThickness);
                strokeTextView.Text = e.NewElement.Text;
                strokeTextView.SetTextColor(Control.TextColors);

                SetNativeControl(strokeTextView);
            }
        }
    }
}
