using System;
using Foundation;
using StopLight.Elements;
using StopLight.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(OutlineLabel), typeof(OutlineLabelRenderer))]
namespace StopLight.iOS.Renderers
{
    public class OutlineLabelRenderer: LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {   
            base.OnElementChanged(e);

            OutlineLabel myCustomLabel = Element as OutlineLabel;

            if (Control != null)
            {
                UIStringAttributes strokeTextAttributes = new UIStringAttributes();
                if (string.IsNullOrEmpty(Element.TextColor.ToString()) == false && Element.TextColor.R != -1)
                {
                    Control.TextColor = Element.TextColor.ToUIColor();
                }
                if (string.IsNullOrEmpty(myCustomLabel.StrokeColor))
                {

                    return;
                }

                ColorTypeConverter converter = new ColorTypeConverter();
                Color color = (Color)(converter.ConvertFromInvariantString(myCustomLabel.StrokeColor));
                strokeTextAttributes.StrokeColor = color.ToUIColor();
                strokeTextAttributes.StrokeWidth = (float)-0.5 * myCustomLabel.StrokeThickness;
                strokeTextAttributes.ForegroundColor = Element.TextColor.ToUIColor();
                Control.AttributedText = new NSAttributedString(Control.Text, strokeTextAttributes);
            }
        }
    }
}