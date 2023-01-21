using System;
using Plugin.BLE;
using System.Diagnostics;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Text;
using StopLight.Models;
using System.ComponentModel;

namespace StopLight.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            bool hasPresets = Preferences.ContainsKey(Preset.PresetsKey);
            if (hasPresets == false)
            {
                Preset.CreateDefault();
            }

            bool hasCurrentPreset = Preferences.ContainsKey(Preset.CurrentPresetKey);
            if (hasCurrentPreset == false)
            {
                var defaultPreset = Preset.CreateDefault();
                Preset.SaveToCurrent(defaultPreset);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StopLightManagerDevice.Shared.PropertyChanged += StopLightManagerDeviceChanged;
            StopLightManagerDevice.Shared.CheckConnection();
            UpdateDeviceInfo();
        }

        private void StopLightManagerDeviceChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateDeviceInfo();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLightManagerDevice.Shared.PropertyChanged -= StopLightManagerDeviceChanged;
        }

        private void UpdateDeviceInfo()
        {
            if (StopLightManagerDevice.Shared.Device != null)
            {
                BLEDeviceName.Text = "Connected to: " + StopLightManagerDevice.Shared.Device.Name;
            }
            else
            {
                BLEDeviceName.Text = string.Empty;
            }
            StopLightManagerDevice.Shared.UpdateDevice();
            ConnectButton.IsVisible = StopLightManagerDevice.Shared.Device == null;
            DisconnectButton.IsVisible = StopLightManagerDevice.Shared.Device != null;
        }

        async void DisconnectPressed(object sender, EventArgs e)
        {
            StopLightManagerDevice.Shared.CheckConnection();
            if (StopLightManagerDevice.Shared.Device == null)
            {
                UpdateDeviceInfo();
                return;
            }
            await StopLightManagerDevice.Shared.Disconnect();
        }

        async void ConnectPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConnectionPage());
        }

        async void PresetPressed(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PresetsViewPage());
        }

        async void RedTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LightSequencePage("Red"));
        }

        async void YellowTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LightSequencePage("Yellow"));
        }

        async void GreenTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LightSequencePage("Green"));
        }

        async void DontWalkTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LightSequencePage("Don't Walk"));
        }

        async void WalkTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LightSequencePage("Walk"));
        }

        async void CycleTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CyclePage());
        }

        async void SavePresetPressed(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("New preset", "Name");
            var preset = Preset.GetCurrent();
            Preset.SaveNewPreset(preset, result);
        }
    }
}
