//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Setting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Setting")]
    public sealed class SettingComponent : GameFrameworkComponent
    {
        private ISettingManager mSettingManager = null;

        [SerializeField]
        private string mSettingHelperTypeName = "UnityGameFramework.Runtime.DefaultSettingHelper";

        [SerializeField]
        private SettingHelperBase mCustomSettingHelper = null;

        public int Count
        {
            get
            {
                return mSettingManager.Count;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mSettingManager = GameFrameworkEntry.GetModule<ISettingManager>();
            if (mSettingManager == null)
            {
                Log.Fatal("Setting manager is invalid.");
                return;
            }

            SettingHelperBase settingHelper = Helper.CreateHelper(mSettingHelperTypeName, mCustomSettingHelper);
            if (settingHelper == null)
            {
                Log.Error("Can not create setting helper.");
                return;
            }

            settingHelper.name = "Setting Helper";
            Transform transform = settingHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            mSettingManager.SetSettingHelper(settingHelper);
        }

        private void Start()
        {
            if (!mSettingManager.Load())
            {
                Log.Error("Load settings failure.");
            }
        }

        public void Save()
        {
            mSettingManager.Save();
        }

        public string[] GetAllSettingNames()
        {
            return mSettingManager.GetAllSettingNames();
        }

        public void GetAllSettingNames(List<string> results)
        {
            mSettingManager.GetAllSettingNames(results);
        }

        public bool HasSetting(string settingName)
        {
            return mSettingManager.HasSetting(settingName);
        }

        public void RemoveSetting(string settingName)
        {
            mSettingManager.RemoveSetting(settingName);
        }

        public void RemoveAllSettings()
        {
            mSettingManager.RemoveAllSettings();
        }

        public bool GetBool(string settingName)
        {
            return mSettingManager.GetBool(settingName);
        }

        public bool GetBool(string settingName, bool defaultValue)
        {
            return mSettingManager.GetBool(settingName, defaultValue);
        }

        public void SetBool(string settingName, bool value)
        {
            mSettingManager.SetBool(settingName, value);
        }

        public int GetInt(string settingName)
        {
            return mSettingManager.GetInt(settingName);
        }

        public int GetInt(string settingName, int defaultValue)
        {
            return mSettingManager.GetInt(settingName, defaultValue);
        }

        public void SetInt(string settingName, int value)
        {
            mSettingManager.SetInt(settingName, value);
        }

        public float GetFloat(string settingName)
        {
            return mSettingManager.GetFloat(settingName);
        }

        public float GetFloat(string settingName, float defaultValue)
        {
            return mSettingManager.GetFloat(settingName, defaultValue);
        }

        public void SetFloat(string settingName, float value)
        {
            mSettingManager.SetFloat(settingName, value);
        }

        public string GetString(string settingName)
        {
            return mSettingManager.GetString(settingName);
        }

        public string GetString(string settingName, string defaultValue)
        {
            return mSettingManager.GetString(settingName, defaultValue);
        }

        public void SetString(string settingName, string value)
        {
            mSettingManager.SetString(settingName, value);
        }

        public T GetObject<T>(string settingName)
        {
            return mSettingManager.GetObject<T>(settingName);
        }

        public object GetObject(Type objectType, string settingName)
        {
            return mSettingManager.GetObject(objectType, settingName);
        }

        public T GetObject<T>(string settingName, T defaultObj)
        {
            return mSettingManager.GetObject(settingName, defaultObj);
        }

        public object GetObject(Type objectType, string settingName, object defaultObj)
        {
            return mSettingManager.GetObject(objectType, settingName, defaultObj);
        }

        public void SetObject<T>(string settingName, T obj)
        {
            mSettingManager.SetObject(settingName, obj);
        }

        public void SetObject(string settingName, object obj)
        {
            mSettingManager.SetObject(settingName, obj);
        }
    }
}
