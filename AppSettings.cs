using System;
using System.IO;
using System.Text.Json;

namespace TrayPasswordGenerator
{
    public sealed class AppSettings
    {
        public bool UseNumbers  { get; set; } = true;
        public bool UseUppercase{ get; set; } = true;
        public SpecialMode SpecialCharactersMode { get; set; } = SpecialMode.Safe;
        public int  PasswordLength { get; set; } = 16;
        public string StaticPrefix { get; set; } = string.Empty;

        public enum SpecialMode { None, Safe, All }

        private const string FILE = "settings.json";

        public static AppSettings Load()
        {
            try
            {
                string path = GetPath();
                if (File.Exists(path))
                    return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new();
            }
            catch { /* игнорируем */ }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(GetPath(),
                    JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* игнорируем */ }
        }

        private static string GetPath()
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TrayPasswordGenerator");

            Directory.CreateDirectory(dir);
            return Path.Combine(dir, FILE);
        }
    }
}
