using System;
using Xamarin.Forms;

namespace StopLight.Elements
{
    public class OutlineLabel: Label
    {

        public OutlineLabel()
        {
        }

        public static readonly BindableProperty StrokeColorProperty = BindableProperty.CreateAttached("StrokeColor", typeof(string), typeof(OutlineLabel), "");
        public string StrokeColor
        {
            get { return base.GetValue(StrokeColorProperty).ToString(); }
            set { base.SetValue(StrokeColorProperty, value); }
        }

        public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.CreateAttached("StrokeThickness", typeof(int), typeof(OutlineLabel), 0);
        public int StrokeThickness
        {
            get { return (int)base.GetValue(StrokeThicknessProperty); }
            set { base.SetValue(StrokeThicknessProperty, value); }
        }
    }
}