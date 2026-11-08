//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityGameFramework.Runtime
{
    public sealed class DefaultSetting
    {
        private readonly SortedDictionary<string, string> mSettings = new SortedDictionary<string, string>(StringComparer.Ordinal);

        public DefaultSetting()
        {
        }

        public int Count
        {
            get
            {
                return mSettings.Count;
            }
        }

        public string[] GetAllSettingNames()
        {
            int index = 0;
            string[] allSettingNames = new string[mSettings.Count];
            foreach (KeyValuePair<string, string> setting in mSettings)
            {
                allSettingNames[index++] = setting.Key;
            }

            return allSettingNames;
        }

        public void GetAllSettingNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, string> setting in mSettings)
            {
                results.Add(setting.Key);
            }
        }

        public bool HasSetting(string settingName)
        {
            return mSettings.ContainsKey(settingName);
        }

        public bool RemoveSetting(string settingName)
        {
            return mSettings.Remove(settingName);
        }

        public void RemoveAllSettings()
        {
            mSettings.Clear();
        }

        public bool GetBool(string settingName)
        {
            string value = null;
            if (!mSettings.TryGetValue(settingName, out value))
            {
                Log.Warning("Setting '{0}' is not exist.", settingName);
                return false;
            }

            return int.Parse(value, CultureInfo.InvariantCulture) != 0;
        }

        public bool GetBool(string settingName, bool defaultValue)
        {
            string value = null;
            if (!mSettings.TryGetValue(settingName, out value))
            {
                return defaultValue;
            }

            return int.Parse(value, CultureInfo.InvariantCulture) != 0;
        }

        public void SetBool(string settingName, bool value)
        {
            mSettings[settingName] = value ? "1" : "0";
        }

        public int GetInt(string settingName)
        {
            string value = null;
            if (!mSettings.TryGetValue(settingName, out value))
            {
                Log.Warning("Setting '{0}' is not exist.", settingName);
                return 0;
            }

            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        public int GetInt(string settingName, int defaultValue)
        {
            string value = null;
            if (!mSettings.TryGetValue(settingName, out value))
            {
                return defaultValue;
            }

            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        public void SetInt(string settingName, int value)
        {
            mSettings[settingName] = value.ToString(CultureInfo.InvariantCulture);
        }

        public float GetFloat(string settingName)
        {
            string value = null;
            if (!mSettings.TryGetValue(settingName, out value))
            {
                Log.Warning("Setting '{0}' is not exist.", settingName);
                return 0f;
            }

            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        public float GetFloat(string settingName, float defaultValue)
        {
            string value = null;
            if (!mSettings.TryGetValue(settingName, out value))
            {
                return defaultValue;
            }

            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        public void SetFloat(string settingName, float value)
        {
            mSettings[settingName] = value.ToString(CultureInfo.InvariantCulture);
        }

        public string GetString(string settingName)
        {
            string value = null;
            if (!mSettings.TryGetValue(settingName, out value))
            {
                Log.Warning("Setting '{0}' is not exist.", settingName);
                return null;
            }

            return value;
        }

        public string GetString(string settingName, string defaultValue)
        {
            string value = null;
            if (!mSettings.TryGetValue(settingName, out value))
            {
                return defaultValue;
            }

            return value;
        }

        public void SetString(string settingName, string value)
        {
            mSettings[settingName] = value;
        }

        public void Serialize(Stream stream)
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
            {
                binaryWriter.Write7BitEncodedInt32(mSettings.Count);
                foreach (KeyValuePair<string, string> setting in mSettings)
                {
                    binaryWriter.Write(setting.Key);
                    binaryWriter.Write(setting.Value);
                }
            }
        }

        public void Deserialize(Stream stream)
        {
            // Validate the whole file before replacing live settings. A malformed header
            // must trigger the caller's backup recovery, not look like an empty save.
            var candidate = new Dictionary<string, string>();
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
            {
                int settingCount = binaryReader.Read7BitEncodedInt32();
                const int maximumSettingCount = 65536;
                if (settingCount < 0 || settingCount > maximumSettingCount)
                    throw new InvalidDataException("Setting count is outside the supported range.");
                // Even two empty strings require at least two length-prefix bytes.
                if (stream.CanSeek && settingCount > (stream.Length - stream.Position) / 2)
                    throw new InvalidDataException("Setting count exceeds the remaining file data.");
                for (int i = 0; i < settingCount; i++)
                    candidate.Add(binaryReader.ReadString(), binaryReader.ReadString());
                if (binaryReader.BaseStream.ReadByte() != -1)
                    throw new InvalidDataException("Unexpected trailing data in settings file.");
            }
            mSettings.Clear();
            foreach (var setting in candidate) mSettings.Add(setting.Key, setting.Value);
        }
    }
}
