//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework.WebRequest;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class WebRequestAgentHelperBase : MonoBehaviour, IWebRequestAgentHelper
    {
        public abstract event EventHandler<WebRequestAgentHelperCompleteEventArgs> WebRequestAgentHelperComplete;

        public abstract event EventHandler<WebRequestAgentHelperErrorEventArgs> WebRequestAgentHelperError;

        public abstract void Request(string webRequestUri, object userData);

        public abstract void Request(string webRequestUri, byte[] postData, object userData);

        public abstract void Reset();
    }
}
