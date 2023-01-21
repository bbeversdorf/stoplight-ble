using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace StopLight.Elements
{
    public partial class LightSequenceView : ContentView
    {
        Command RemoveCommand;

        public double SequenceTime
        {
            get => Timing.SelectedIndex + 1;
        }

        public string SequenceStatus
        {
            get
            {
                switch (Status.SelectedItem)
                {
                    case "On":
                        return "n";
                    case "Blink":
                        return "b";
                    default:
                        return "f";
                }
            }
        }

        public LightSequenceView(Command removeCommand, double timing = 0, string status = "f")
        {
            InitializeComponent();
            RemoveCommand = removeCommand;
            Timing.SelectedIndex = (int) timing - 1;
            Status.SelectedItem = SelectedStatus(status);
        }

        void RemoveButtonPressed(object sender, EventArgs e)
        {
            RemoveCommand.Execute(this);
        }

        string SelectedStatus(string status)
        {
            switch (status)
            {
                case "n":
                    return "On";
                case "b":
                    return "Blink";
                default:
                    return "Off";
            }
        }

    }
}
