using System;
using System.Collections.Generic;
using System.Linq;
using StopLight.Elements;
using StopLight.Models;
using Xamarin.Forms;

namespace StopLight.Pages
{
    public partial class LightSequencePage : ContentPage
    {
        private string Light;
        private string TimingKey;
        private string CommandKey;
        public LightSequencePage(string light)
        {
            InitializeComponent();
            Light = light;
            Title = Light;
            TimingKey = Light.Contains("Walk") ? "wt" : "st";
            CommandKey = GetCommandKey();


            var presets = Preset.GetCurrent();
            var preset = presets.CommandList.FirstOrDefault(c => c.Key == CommandKey);
            var timingPreset = presets.CommandList.FirstOrDefault(c => c.Key == TimingKey);
            var save = false;

            if (preset == null)
            {
                preset = new CommandPreset(CommandKey, CommandPreset.DefaultStatus(CommandKey), null);
                presets.CommandList.Add(preset);
                save = true;
            }

            if (timingPreset == null)
            {
                timingPreset = new CommandPreset(TimingKey, null, CommandPreset.DefaultTiming(TimingKey));
                presets.CommandList.Add(timingPreset);
                save = true;
            }

            if (save)
            {
                Preset.SaveToCurrent(presets);
            }

            var removeCommand = new Command(RemoveSequence);
            for (var i = 0; i < timingPreset.Timing.Count(); i++)
            {
                var status = preset.Status != null &&
                    i < preset.Status.Count ? preset.Status[i] : "f";
                Sequences.Children.Add(new LightSequenceView(removeCommand, timingPreset.Timing[i], status));
            }

        }

        void AddButtonPressed(object sender, EventArgs e)
        {
            var removeCommand = new Command(RemoveSequence);
            Sequences.Children.Add(new LightSequenceView(removeCommand));
        }

        async void SaveButtonPressed(object sender, EventArgs e)
        {
            var presets = Preset.GetCurrent();
            var preset = presets.CommandList.FirstOrDefault(c => c.Key == CommandKey);
            var timingPreset = presets.CommandList.FirstOrDefault(c => c.Key == TimingKey);

            timingPreset.Timing.RemoveAll(t => true);
            preset.Status.RemoveAll(t => true);

            foreach (var child in Sequences.Children)
            {
                if (child is LightSequenceView view)
                {
                    timingPreset.Timing.Add(view.SequenceTime);
                    preset.Status.Add(view.SequenceStatus);
                }   
            }

            Preset.SaveToCurrent(presets);
            await Navigation.PopToRootAsync();
        }

        private async void RemoveSequence(object obj)
        {
            bool answer = await DisplayAlert("Are you sure?", "Would you like to remove this step?", "Yes", "No");
            if (!answer)
            {
                return;
            }

            if (!(obj is ContentView view))
            {
                return;
            }

            Sequences.Children.Remove(view);
        }

        string GetCommandKey()
        {
            switch (Light)
            {
                case "Red":
                    return "r";
                case "Yellow":
                    return "y";
                case "Green":
                    return "g";
                case "Walk":
                    return "wg";
                case "Don't Walk":
                    return "wr";
                default:
                    return "";
            }
        }
    }
}
