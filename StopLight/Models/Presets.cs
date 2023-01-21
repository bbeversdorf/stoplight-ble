using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace StopLight.Models
{
    public class Preset
    {
        public static string CurrentPresetKey = "current_preset_key";
        public static string PresetsKey = "presets_key";
        public string Name { get; set; }
        public List<CommandPreset> CommandList { get; set; }

        public Preset()
        {
            Name = "";
            CommandList = new List<CommandPreset>();
        }

        public Preset(string name, List<CommandPreset> presets)
        {
            Name = name;
            CommandList = presets;
        }

        public static void SaveDefault()
        {
            var preset = CreateDefault();
            var presetList = new List<Preset>
            {
                preset
            };
            Preferences.Set(PresetsKey, JsonConvert.SerializeObject(presetList));
        }

        public static Preset CreateDefault()
        {
            var preset = new CommandPreset("y", new List<string> { "o" }, new List<double> { 1 });
            var cycle = new CommandPreset("cycle", null, new List<double> { 30 });
            var blinkSpeed = new CommandPreset("blinkSpeed", null, new List<double> { 0.25 });
            var presets = new Preset("Yellow only preset", new List<CommandPreset>
            {
                preset,
                cycle,
                blinkSpeed
            });
            return presets;
        }

        public static List<Preset> GetPresets()
        {
            var presetString = Preferences.Get(PresetsKey, "");
            return JsonConvert.DeserializeObject<List<Preset>>(presetString);
        }

        public static void SaveNewPreset(Preset preset, string result)
        {
            preset.Name = result;
            var currentPresets = GetPresets() ?? new List<Preset>();
            currentPresets.Add(preset);
            Preferences.Set(PresetsKey, JsonConvert.SerializeObject(currentPresets));
        }

        public static void SaveToCurrent(Preset preset)
        {
            // Filter out empty
            var commands = preset.CommandList.FindAll(p => (p.Status != null &&
                p.Status.Any()) || (p.Timing != null && p.Timing.Any()));
            preset.CommandList = commands;
            Preferences.Set(CurrentPresetKey, JsonConvert.SerializeObject(preset));
            StopLightManagerDevice.Shared.Send(preset);
        }

        public static Preset GetCurrent()
        {
            var presetString = Preferences.Get(CurrentPresetKey, "");
            var presets = JsonConvert.DeserializeObject<Preset>(presetString);
            return presets;
        }
    }

    public class CommandPreset
    {
        public string Key { get; set; }
        public List<string> Status { get; set; }
        public List<double> Timing { get; set; }

        public CommandPreset()
        {
            Key = string.Empty;
            Status = new List<string>();
            Timing = new List<double>();
        }

        public CommandPreset(string key, List<string> status, List<double> time)
        {
            Key = key;
            Status = status;
            Timing = time;
        }

        public string Encode()
        {
            switch (Key)
            {
                case "st":
                case "wt":
                    var timings = string.Join(";", Timing.ToArray());
                    return $"{Key}_{timings}";
                case "blinkSpeed":
                case "cycle":
                    return $"{Key}_{Timing.FirstOrDefault()}";
                // r, y, g, wr, wg
                default:
                    string statuses = string.Join(";", Status.Select(s => $"{s}"));
                    return $"{Key}_{statuses}";
            }
        }

        public static List<string> DefaultStatus(string key)
        {
            switch (key)
            {
                case "r":
                    return new List<string> { "f", "f", "n" };
                case "y":
                    return new List<string> { "f", "n", "n" };
                case "g":
                    return new List<string> { "n", "f", "f" };
                case "wg":
                    return new List<string> { "f", "b", "n" };
                case "wr":
                    return new List<string> { "n", "f", "f" };
            }
            return new List<string> { "f", "f", "f" };
        }

        public static List<double> DefaultTiming(string key)
        {
            return key == "wt" ? new List<double>
                {
                    6, 12, 20
                } : new List<double>
                {
                    10, 13, 20
                };
        }
    }
}