//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework.Setting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class SettingHelperBase : MonoBehaviour, ISettingHelper
    {
        public abstract int Count
        {
            get;
        }

        public abstract bool Load();

        public abstract bool Save();

        public abstract string[] GetAllSettingNames();

        public abstract void GetAllSettingNames(List<string> results);

        public abstract bool HasSetting(string settingName);

        public abstract bool RemoveSetting(string settingName);

        public abstract void RemoveAllSettings();

        public abstract bool GetBool(string settingName);

        public abstract bool GetBool(string settingName, bool defaultValue);

        public abstract void SetBool(string settingName, bool value);

        public abstract int GetInt(string settingName);

        public abstract int GetInt(string settingName, int defaultValue);

        public abstract void SetInt(string settingName, int value);

        public abstract float GetFloat(string settingName);

        public abstract float GetFloat(string settingName, float defaultValue);

        public abstract void SetFloat(string settingName, float value);

        public abstract string GetString(string settingName);

        public abstract string GetString(string settingName, string defaultValue);

        public abstract void SetString(string settingName, string value);

        public abstract T GetObject<T>(string settingName);

        public abstract object GetObject(Type objectType, string settingName);

        public abstract T GetObject<T>(string settingName, T defaultObj);

        public abstract object GetObject(Type objectType, string settingName, object defaultObj);

        public abstract void SetObject<T>(string settingName, T obj);

        public abstract void SetObject(string settingName, object obj);
    }
}
