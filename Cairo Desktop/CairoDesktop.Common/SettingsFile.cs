using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace CairoDesktop.Common
{
    public class SettingsFile<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static JsonSerializerOptions options = new JsonSerializerOptions()
        {
            IgnoreReadOnlyProperties = true,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private string _fileName;

        private T _settings;
        public T Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
                SaveToFile();
            }
        }

        public SettingsFile(string fileName, T defaultSettings)
        {
            _fileName = fileName;
            _settings = defaultSettings;

            if (!LoadFromFile())
            {
                ShellLogger.Info("SettingsFile: Using default settings");
            }
        }

        private bool LoadFromFile()
        {
            try
            {
                string jsonString = File.ReadAllText(_fileName);
                _settings = JsonSerializer.Deserialize<T>(jsonString);

                return true;
            }
            catch (Exception ex)
            {
                ShellLogger.Error($"SettingsFile: Error loading settings file: {ex.Message}");
                return false;
            }
        }

        private void SaveToFile()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_fileName));

                string jsonString = JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(_fileName, jsonString);
            }
            catch (Exception ex)
            {
                ShellLogger.Error($"SettingsFile: Error saving settings file: {ex.Message}");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
