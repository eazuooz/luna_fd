//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework.UI;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class UIFormHelperBase : MonoBehaviour, IUIFormHelper
    {
        public abstract object InstantiateUIForm(object uiFormAsset);

        public abstract IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData);

        public abstract void ReleaseUIForm(object uiFormAsset, object uiFormInstance);
    }
}
