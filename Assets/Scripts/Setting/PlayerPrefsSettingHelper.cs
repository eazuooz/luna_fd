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
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class PlayerPrefsSettingHelper : SettingHelperBase
    {
        public override int Count
        {
            get
            {
                return -1;
            }
        }

        public override bool Load()
        {
            return true;
        }

        public override bool Save()
        {
            PlayerPrefs.Save();
            return true;
        }

        public override string[] GetAllSettingNames()
        {
            Log.Warning("GetAllSettingNames is not supported.");
            return null;
        }

        public override void GetAllSettingNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            Log.Warning("GetAllSettingNames is not supported.");
        }

        public override bool HasSetting(string settingName)
        {
            return PlayerPrefs.HasKey(settingName);
        }

        public override bool RemoveSetting(string settingName)
        {
            if (!PlayerPrefs.HasKey(settingName))
            {
                return false;
            }

            PlayerPrefs.DeleteKey(settingName);
            return true;
        }

        public override void RemoveAllSettings()
        {
            PlayerPrefs.DeleteAll();
        }

        public override bool GetBool(string settingName)
        {
            return PlayerPrefs.GetInt(settingName) != 0;
        }

        public override bool GetBool(string settingName, bool defaultValue)
        {
            return PlayerPrefs.GetInt(settingName, defaultValue ? 1 : 0) != 0;
        }

        public override void SetBool(string settingName, bool value)
        {
            PlayerPrefs.SetInt(settingName, value ? 1 : 0);
        }

        public override int GetInt(string settingName)
        {
            return PlayerPrefs.GetInt(settingName);
        }

        public override int GetInt(string settingName, int defaultValue)
        {
            return PlayerPrefs.GetInt(settingName, defaultValue);
        }

        public override void SetInt(string settingName, int value)
        {
            PlayerPrefs.SetInt(settingName, value);
        }

        public override float GetFloat(string settingName)
        {
            return PlayerPrefs.GetFloat(settingName);
        }

        public override float GetFloat(string settingName, float defaultValue)
        {
            return PlayerPrefs.GetFloat(settingName, defaultValue);
        }

        public override void SetFloat(string settingName, float value)
        {
            PlayerPrefs.SetFloat(settingName, value);
        }

        public override string GetString(string settingName)
        {
            return PlayerPrefs.GetString(settingName);
        }

        public override string GetString(string settingName, string defaultValue)
        {
            return PlayerPrefs.GetString(settingName, defaultValue);
        }

        public override void SetString(string settingName, string value)
        {
            PlayerPrefs.SetString(settingName, value);
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
            PlayerPrefs.SetString(settingName, Utility.Json.ToJson(obj));
        }

        public override void SetObject(string settingName, object obj)
        {
            PlayerPrefs.SetString(settingName, Utility.Json.ToJson(obj));
        }
    }
}
