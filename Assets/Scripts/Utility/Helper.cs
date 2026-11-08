//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public static class Helper
    {
        public static T CreateHelper<T>(string helperTypeName, T customHelper) where T : MonoBehaviour
        {
            return CreateHelper(helperTypeName, customHelper, 0);
        }

        public static T CreateHelper<T>(string helperTypeName, T customHelper, int index) where T : MonoBehaviour
        {
            T helper = null;
            if (!string.IsNullOrEmpty(helperTypeName))
            {
                System.Type helperType = Utility.Assembly.GetType(helperTypeName);
                if (helperType == null)
                {
                    Log.Warning("Can not find helper type '{0}'.", helperTypeName);
                    return null;
                }

                if (!typeof(T).IsAssignableFrom(helperType))
                {
                    Log.Warning("Type '{0}' is not assignable from '{1}'.", typeof(T).FullName, helperType.FullName);
                    return null;
                }

                helper = (T)new GameObject().AddComponent(helperType);
            }
            else if (customHelper == null)
            {
                Log.Warning("You must set custom helper with '{0}' type first.", typeof(T).FullName);
                return null;
            }
            else if (customHelper.gameObject.InScene())
            {
                helper = index > 0 ? Object.Instantiate(customHelper) : customHelper;
            }
            else
            {
                helper = Object.Instantiate(customHelper);
            }

            return helper;
        }
    }
}
