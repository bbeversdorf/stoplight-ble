using System;
using Plugin.BLE.Abstractions.Contracts;

namespace StopLight.Models
{
    public class DeviceListItemViewModel
    {
        public DeviceListItemViewModel()
        {
        }

        public DeviceListItemViewModel(IDevice device)
        {
            Device = device;
        }

        public DeviceListItemViewModel(string name)
        {
            _Name = name;
        }

        public bool IsConnected { get; internal set; }
        public IDevice Device { get; internal set; }
        public string _Name { get; internal set; }

        public string Name => !string.IsNullOrEmpty(_Name) ? _Name : Device.Name;

        internal void Update()
        {
            System.Diagnostics.Debug.WriteLine(Device);
        }
    }
}