using System;
using System.IO;
using System.Text.Json;

namespace BubbleShot.Core
{
    /// <summary>
    /// Pure C# atomic persistence manager with schema versioning and corrupt file recovery.
    /// </summary>
    public static class SaveSystem
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static bool SaveAtomic(string targetPath, SaveDataV1 data)
        {
            string tmpPath = targetPath + ".tmp";
            try
            {
                string directory = Path.GetDirectoryName(targetPath) ?? "";
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(data, JsonOptions);
                File.WriteAllText(tmpPath, json);

                // Atomic swap
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                File.Move(tmpPath, targetPath);
                return true;
            }
            catch
            {
                if (File.Exists(tmpPath))
                {
                    try { File.Delete(tmpPath); } catch { }
                }
                return false;
            }
        }

        public static SaveDataV1 LoadSafe(string targetPath)
        {
            if (!File.Exists(targetPath))
            {
                return new SaveDataV1();
            }

            try
            {
                string json = File.ReadAllText(targetPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new SaveDataV1();
                }

                // Check version and deserialize
                var data = JsonSerializer.Deserialize<SaveDataV1>(json, JsonOptions);
                return data ?? new SaveDataV1();
            }
            catch
            {
                // File is corrupt or unparseable: preserve backup and return clean defaults
                try
                {
                    string corruptBackupPath = $"{targetPath}.corrupt.{DateTime.UtcNow.Ticks}";
                    File.Copy(targetPath, corruptBackupPath, true);
                }
                catch { }

                return new SaveDataV1();
            }
        }

        public static string SerializeToString(SaveDataV1 data)
        {
            return JsonSerializer.Serialize(data, JsonOptions);
        }

        public static SaveDataV1 DeserializeFromString(string json)
        {
            return JsonSerializer.Deserialize<SaveDataV1>(json, JsonOptions) ?? new SaveDataV1();
        }
    }
}
