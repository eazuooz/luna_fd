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
using System.IO;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class DefaultSettingHelper : SettingHelperBase
    {
        private const string SettingFileName = "GameFrameworkSetting.dat";

        private string mFilePath = null;
        private DefaultSetting mSettings = null;
        private DefaultSettingSerializer mSerializer = null;

        public override int Count
        {
            get
            {
                return mSettings != null ? mSettings.Count : 0;
            }
        }

        public string FilePath
        {
            get
            {
                return mFilePath;
            }
        }

        public DefaultSetting Setting
        {
            get
            {
                return mSettings;
            }
        }

        public DefaultSettingSerializer Serializer
        {
            get
            {
                return mSerializer;
            }
        }

        public override bool Load()
        {
            try
            {
                if (!File.Exists(mFilePath))
                {
                    return true;
                }

                using (FileStream fileStream = new FileStream(mFilePath, FileMode.Open, FileAccess.Read))
                {
                    mSerializer.Deserialize(fileStream);
                    return true;
                }
            }
            catch (Exception exception)
            {
                Log.Warning("Load settings failure with exception '{0}'.", exception);
                return false;
            }
        }

        public override bool Save()
        {
            try
            {
                using (FileStream fileStream = new FileStream(mFilePath, FileMode.Create, FileAccess.Write))
                {
                    return mSerializer.Serialize(fileStream, mSettings);
                }
            }
            catch (Exception exception)
            {
                Log.Warning("Save settings failure with exception '{0}'.", exception);
                return false;
            }
        }

        public override string[] GetAllSettingNames()
        {
            return mSettings.GetAllSettingNames();
        }

        public override void GetAllSettingNames(List<string> results)
        {
            mSettings.GetAllSettingNames(results);
        }

        public override bool HasSetting(string settingName)
        {
            return mSettings.HasSetting(settingName);
        }

        public override bool RemoveSetting(string settingName)
        {
            return mSettings.RemoveSetting(settingName);
        }

        public override void RemoveAllSettings()
        {
            mSettings.RemoveAllSettings();
        }

        public override bool GetBool(string settingName)
        {
            return mSettings.GetBool(settingName);
        }

        public override bool GetBool(string settingName, bool defaultValue)
        {
            return mSettings.GetBool(settingName, defaultValue);
        }

        public override void SetBool(string settingName, bool value)
        {
            mSettings.SetBool(settingName, value);
        }

        public override int GetInt(string settingName)
        {
            return mSettings.GetInt(settingName);
        }

        public override int GetInt(string settingName, int defaultValue)
        {
            return mSettings.GetInt(settingName, defaultValue);
        }

        public override void SetInt(string settingName, int value)
        {
            mSettings.SetInt(settingName, value);
        }

        public override float GetFloat(string settingName)
        {
            return mSettings.GetFloat(settingName);
        }

        public override float GetFloat(string settingName, float defaultValue)
        {
            return mSettings.GetFloat(settingName, defaultValue);
        }

        public override void SetFloat(string settingName, float value)
        {
            mSettings.SetFloat(settingName, value);
        }

        public override string GetString(string settingName)
        {
            return mSettings.GetString(settingName);
        }

        public override string GetString(string settingName, string defaultValue)
        {
            return mSettings.GetString(settingName, defaultValue);
        }

        public override void SetString(string settingName, string value)
        {
            mSettings.SetString(settingName, value);
        }

        public override T GetObject<T>(string settingName)
        {
            return Utility.Json.ToObject<T>(GetString(settingName));
        }

        public override object GetObject(Type objectType, string settingName)
        {
            return Utility.Json.ToObject(objectType, GetString(settingName));
        }

        public override T GetObject<T>(string settingName, T defaultObj)
        {
            string json = GetString(settingName, null);
            if (json == null)
            {
                return defaultObj;
            }

            return Utility.Json.ToObject<T>(json);
        }

        public override object GetObject(Type objectType, string settingName, object defaultObj)
        {
            string json = GetString(settingName, null);
            if (json == null)
            {
                return defaultObj;
            }

            return Utility.Json.ToObject(objectType, json);
        }

        public override void SetObject<T>(string settingName, T obj)
        {
            SetString(settingName, Utility.Json.ToJson(obj));
        }

        public override void SetObject(string settingName, object obj)
        {
            SetString(settingName, Utility.Json.ToJson(obj));
        }

        private void Awake()
        {
            mFilePath = Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath, SettingFileName));
            mSettings = new DefaultSetting();
            mSerializer = new DefaultSettingSerializer();
            mSerializer.RegisterSerializeCallback(0, SerializeDefaultSettingCallback);
            mSerializer.RegisterDeserializeCallback(0, DeserializeDefaultSettingCallback);
        }

        private bool SerializeDefaultSettingCallback(Stream stream, DefaultSetting defaultSetting)
        {
            mSettings.Serialize(stream);
            return true;
        }

        private DefaultSetting DeserializeDefaultSettingCallback(Stream stream)
        {
            mSettings.Deserialize(stream);
            return mSettings;
        }
    }
}
