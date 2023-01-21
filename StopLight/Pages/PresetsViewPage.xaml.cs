
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using StopLight.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Linq;
using System;

namespace StopLight.Pages
{
    public partial class PresetsViewPage : ContentPage
    {
        public ObservableCollection<Preset> Items { get; set; }

        public PresetsViewPage()
        {
            InitializeComponent();

            bool hasPresets = Preferences.ContainsKey(Preset.PresetsKey);
            if (hasPresets == false)
            {
                return;
            }

            var presets = Preset.GetPresets();
            Items = new ObservableCollection<Preset>();

            foreach (Preset preset in presets)
            {
                Items.Add(preset);
            }

            PresetListView.ItemsSource = Items;
        }

        public void OnDelete(object sender, EventArgs e)
        {
            var preset = ((MenuItem)sender).CommandParameter;
            if (!(preset is Preset presets))
            {
                return;
            }
            Items.Remove(presets);
            if (Items.Count == 0)
            {
                Preferences.Remove(Preset.PresetsKey);
                return;
            }
            var presetList = Items.ToList();
            Preferences.Set(Preset.PresetsKey, JsonConvert.SerializeObject(presetList));
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (!(e.Item is Preset presets))
            {
                return;
            }

            // Send commands
            Preferences.Set(Preset.CurrentPresetKey, JsonConvert.SerializeObject(presets));
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
            await Navigation.PopAsync();
        }


    }
}
