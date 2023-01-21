using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Xamarin.Essentials;

namespace StopLight.Models
{
    public class StopLightManagerDevice: INotifyPropertyChanged
    {
        IDevice _device;
        ICharacteristic notifyCharacteristic;
        ICharacteristic writeCharacteristic;
        IAdapter Adapter = CrossBluetoothLE.Current.Adapter;

        public static StopLightManagerDevice Shared = new StopLightManagerDevice();

        public event PropertyChangedEventHandler PropertyChanged;

        private StopLightManagerDevice()
        {
            if (Adapter.ConnectedDevices.Count > 0)
            {
                Device = Adapter.ConnectedDevices[0];
            }
        }

        public IDevice Device
        {
            get => _device;
            set
            {
                if (_device?.Id == value?.Id)
                {
                    return;
                }

                _device = value;
                UpdateDevice();
                NotifyPropertyChanged();
            }
        }

        public string Name => Device?.Name;

        public void CheckConnection()
        {
            if (Device == null && Adapter.ConnectedDevices.Count > 0)
            {
                Device = Adapter.ConnectedDevices[0];
            }
            else if (Device != null && Adapter.ConnectedDevices.Count == 0)
            {
                Device = null;
            }
        }

        public void UpdateDevice()
        {
            if (Device != null)
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var service = await Device.GetServiceAsync(Guid.Parse("6e400001-b5a3-f393-e0a9-e50e24dcca9e"));
                    var characteristics = await service.GetCharacteristicsAsync();
                    notifyCharacteristic = await service.GetCharacteristicAsync(Guid.Parse("6e400003-b5a3-f393-e0a9-e50e24dcca9e"));
                    writeCharacteristic = await service.GetCharacteristicAsync(Guid.Parse("6e400002-b5a3-f393-e0a9-e50e24dcca9e"));
                    notifyCharacteristic.ValueUpdated += NotifyCharacteristicValueUpdated;

                    await notifyCharacteristic.StartUpdatesAsync();
                });
            }
            else
            {
                RemoveNotifyCallback();
                notifyCharacteristic = null;
                writeCharacteristic = null;
            }
        }

        public async Task Send(string command)
        {
            if (writeCharacteristic == null)
            {
                return;
            }

            await writeCharacteristic.WriteAsync(Encoding.ASCII.GetBytes(command));
        }

        public async void Send(Preset preset)
        {
            foreach (var command in preset.CommandList)
            {
                await Send(command.Encode()).ConfigureAwait(false);
            }
        }

        public async Task Disconnect()
        {
            try
            {
                await Adapter.DisconnectDeviceAsync(Device);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "Disconnect error");
            }
            finally
            {
                Device = null;
            }
        }

        private void RemoveNotifyCallback()
        {
            if (notifyCharacteristic != null)
            {
                notifyCharacteristic.ValueUpdated -= NotifyCharacteristicValueUpdated;
            }
        }

        private void NotifyCharacteristicValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            var bytes = e.Characteristic.Value;
            var output = Encoding.ASCII.GetString(bytes);
            Debug.Write(output);
            //var old = Results.Text;
            //Results.Text = output + old;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
