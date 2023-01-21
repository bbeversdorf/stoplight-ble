using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StopLight.Models;
using Xamarin.Forms;

namespace StopLight.Pages
{
    public partial class CyclePage : ContentPage
    {
        public ObservableCollection<PickerCycle> CycleItems { get; set; }
        public CyclePage()
        {
            InitializeComponent();
            CycleItems = new ObservableCollection<PickerCycle>
            {
                new PickerCycle(10, "10 seconds"),
                new PickerCycle(20, "20 seconds"),
                new PickerCycle(30, "30 seconds"),
                new PickerCycle(45, "45 seconds"),
                new PickerCycle(60, "60 seconds")
            };
            var preset = Preset.GetCurrent();
            var cycle = preset.CommandList.FirstOrDefault(c => c.Key == "cycle");
            if (cycle == null)
            {
                cycle = new CommandPreset("cycle", null, new List<double> { 30 });
                preset.CommandList.Add(cycle);
                Preset.SaveToCurrent(preset);
            }
            var blinkSpeed = preset.CommandList.FirstOrDefault(c => c.Key == "blinkSpeed");

            if (blinkSpeed == null)
            {
                blinkSpeed = new CommandPreset("blinkSpeed", null, new List<double> { 0.25 });
                preset.CommandList.Add(blinkSpeed);
                Preset.SaveToCurrent(preset);
            }

            var cycleTime = cycle.Timing.First();
            var selectedItem = CycleItems.FirstOrDefault(item => item.key == cycleTime);
            var blinkSpeedTime = blinkSpeed.Timing.First();
            BlinkSpeed.Text = blinkSpeedTime.ToString();

            CyclePicker.ItemsSource = CycleItems;
            CyclePicker.SelectedItem = selectedItem;
        }

        async void ButtonPressed(object sender, EventArgs e)
        {
            var preset = Preset.GetCurrent();
            var cycle = preset.CommandList.FirstOrDefault(c => c.Key == "cycle");
            var blinkSpeed = preset.CommandList.FirstOrDefault(c => c.Key == "blinkSpeed");
            cycle.Timing.RemoveAll(t => true);
            blinkSpeed.Timing.RemoveAll(t => true);

            var cyclePickerText = CyclePicker.SelectedIndex == -1 ? 30 : CycleItems.ElementAt(CyclePicker.SelectedIndex).key;
            cycle.Timing.Add(Convert.ToDouble(cyclePickerText));

            var blinkSpeedText = string.IsNullOrEmpty(BlinkSpeed.Text) ? ".25" : BlinkSpeed.Text;
            blinkSpeed.Timing.Add(Convert.ToDouble(blinkSpeedText));

            Preset.SaveToCurrent(preset);
            await Navigation.PopToRootAsync();
        }
    }

    public class PickerCycle
    {
        public double key;
        public string value;

        public PickerCycle(int key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }
}
