using StopLight.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace StopLight.Pages
{
    public partial class ConnectionPage : ContentPage
    {
        Guid PreviousGuid;
        private IBluetoothLE BluetoothLE = CrossBluetoothLE.Current;
        IAdapter Adapter = CrossBluetoothLE.Current.Adapter;
        private CancellationTokenSource ScanTokenSource;
        private CancellationTokenSource ConnectTokenSource;
        public ObservableCollection<DeviceListItemViewModel> Devices { get; set; } = new ObservableCollection<DeviceListItemViewModel>();

        public ConnectionPage()
        {
            InitializeComponent();
            DeviceListView.ItemsSource = Devices;
        }

        protected override void OnAppearing()
        {
            foreach (IDevice device in Adapter.DiscoveredDevices)
            {
                AddOrUpdateDevice(device);
            }
            BluetoothLE.StateChanged += BLEStateChanged;

            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    try
                    {
                        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                        if (status != PermissionStatus.Granted)
                        {
                            var permissionResult = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                            if (permissionResult != PermissionStatus.Granted)
                            {
                                Debug.WriteLine("Difficult person");
                                return;
                            }
                            else
                            {
                                await StartScanAsync().ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await StartScanAsync().ConfigureAwait(false);
                        }
                    }
                    catch (Exception e)
                    {
                        await StartScanAsync().ConfigureAwait(false);
                        Debug.WriteLine(e);
                    }
                }
            });
        }

        protected override void OnDisappearing()
        {
            BluetoothLE.StateChanged -= BLEStateChanged;
            CleanupScanCancellationToken();
        }

        private void BLEStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            Debug.WriteLine($"The bluetooth state changed to {e.NewState}");
        }

        private async Task StartScanAsync()
        {
            foreach (var connectedDevice in Adapter.ConnectedDevices)
            {
                //update rssi for already connected evices (so tha 0 is not shown in the list)
                try
                {
                    await connectedDevice.UpdateRssiAsync();
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Failed to update RSSI for {connectedDevice.Name}");
                }


                AddOrUpdateDevice(connectedDevice);
            }

            ScanTokenSource = new CancellationTokenSource();
            Adapter.ScanMode = ScanMode.LowLatency;
            Adapter.ScanTimeout = 600;
            Adapter.DeviceDiscovered += AdapterDeviceDiscovered;
            Adapter.ScanTimeoutElapsed += AdapterScanTimeoutElapsed;
            await Adapter.StartScanningForDevicesAsync(ScanTokenSource.Token);
        }

        private void AdapterDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            AddOrUpdateDevice(e.Device);
        }

        private void AddOrUpdateDevice(IDevice device)
        {
            if (string.IsNullOrEmpty(device.Name))
            {
                return;
            }


            var vm = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
            if (vm != null)
            {
                vm.Update();
            }
            else
            {
                Devices.Add(new DeviceListItemViewModel(device));
            }
        }

        private void AdapterScanTimeoutElapsed(object sender, EventArgs e)
        {
            Debug.WriteLine("Cant stop oh did stop.");
            CleanupScanCancellationToken();
        }

        private void CleanupScanCancellationToken()
        {
            try
            {
                ScanTokenSource.Cancel();
                ScanTokenSource.Dispose();
                ScanTokenSource = null;
            }
            catch { }
        }

        private async void HandleSelectedDevice(object sender, ItemTappedEventArgs e)
        {
            if (!(sender is ListView list))
            {
                return;
            }
            if (!(list.SelectedItem is DeviceListItemViewModel device))
            {
                return;
            }

            if (device.IsConnected)
            {
                // option1 - Update RSSI
                //  await device.Device.UpdateRssiAsync();
                //  await _userDialogs.AlertAsync($"Failed to update rssi. Exception: {ex.Message}");

                // option2 - Show Services
                //  DeviceIdKey, device.Device.Id.ToString()

                // option3 - Disconnect
                //  

                DisconnectDevice(device);
            }
            else
            {
                await ConnectDeviceAsync(device);
            }

        }

        private async Task<bool> ConnectDeviceAsync(DeviceListItemViewModel device, bool showPrompt = true)
        {
            if (ConnectTokenSource != null)
            {
                return false;
            }

            ConnectTokenSource = new CancellationTokenSource();
            try
            {
                var connectParameters = new ConnectParameters(autoConnect: false, forceBleTransport: true);
                await Adapter.ConnectToDeviceAsync(device.Device, connectParameters, ConnectTokenSource.Token);

                PreviousGuid = device.Device.Id;
                await Navigation.PopToRootAsync();
                return true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                ConnectTokenSource.Dispose();
                ConnectTokenSource = null;
            }
        }

        private async void DisconnectDevice(DeviceListItemViewModel device)
        {
            try
            {
                if (!device.IsConnected)
                    return;

                await Adapter.DisconnectDeviceAsync(device.Device);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "Disconnect error");
            }
            finally
            {
                device.Update();
            }
        }

        void DidPressDisconnect(System.Object sender, System.EventArgs e)
        {
            Task.Run(async () =>
            {
                foreach (IDevice device in Adapter.ConnectedDevices)
                {
                    await Adapter.DisconnectDeviceAsync(device);
                }
            });
        }

        void DidPressScan(System.Object sender, System.EventArgs e)
        {
            Task.Run(async () =>
            {
                await StartScanAsync();
            });
        }        
    }
}
