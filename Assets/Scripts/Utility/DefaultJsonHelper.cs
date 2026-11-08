//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class DefaultJsonHelper : Utility.Json.IJsonHelper
    {
        public string ToJson(object obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public T ToObject<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        public object ToObject(Type objectType, string json)
        {
            return JsonUtility.FromJson(json, objectType);
        }
    }
}
