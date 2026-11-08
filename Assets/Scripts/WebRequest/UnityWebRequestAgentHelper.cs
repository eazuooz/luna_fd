//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.WebRequest;
using System;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif
using Utility = GameFramework.Utility;

namespace UnityGameFramework.Runtime
{
    public class UnityWebRequestAgentHelper : WebRequestAgentHelperBase, IDisposable
    {
        private UnityWebRequest mUnityWebRequest = null;
        private bool mDisposed = false;

        private EventHandler<WebRequestAgentHelperCompleteEventArgs> mWebRequestAgentHelperCompleteEventHandler = null;
        private EventHandler<WebRequestAgentHelperErrorEventArgs> mWebRequestAgentHelperErrorEventHandler = null;

        public override event EventHandler<WebRequestAgentHelperCompleteEventArgs> WebRequestAgentHelperComplete
        {
            add
            {
                mWebRequestAgentHelperCompleteEventHandler += value;
            }
            remove
            {
                mWebRequestAgentHelperCompleteEventHandler -= value;
            }
        }

        public override event EventHandler<WebRequestAgentHelperErrorEventArgs> WebRequestAgentHelperError
        {
            add
            {
                mWebRequestAgentHelperErrorEventHandler += value;
            }
            remove
            {
                mWebRequestAgentHelperErrorEventHandler -= value;
            }
        }

        public override void Request(string webRequestUri, object userData)
        {
            if (mWebRequestAgentHelperCompleteEventHandler == null || mWebRequestAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Web request agent helper handler is invalid.");
                return;
            }

            WWWFormInfo wwwFormInfo = (WWWFormInfo)userData;
            if (wwwFormInfo.WWWForm == null)
            {
                mUnityWebRequest = UnityWebRequest.Get(webRequestUri);
            }
            else
            {
                mUnityWebRequest = UnityWebRequest.Post(webRequestUri, wwwFormInfo.WWWForm);
            }

#if UNITY_2017_2_OR_NEWER
            mUnityWebRequest.SendWebRequest();
#else
            mUnityWebRequest.Send();
#endif
        }

        public override void Request(string webRequestUri, byte[] postData, object userData)
        {
            if (mWebRequestAgentHelperCompleteEventHandler == null || mWebRequestAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Web request agent helper handler is invalid.");
                return;
            }

            mUnityWebRequest = UnityWebRequest.PostWwwForm(webRequestUri, Utility.Converter.GetString(postData));
#if UNITY_2017_2_OR_NEWER
            mUnityWebRequest.SendWebRequest();
#else
            mUnityWebRequest.Send();
#endif
        }

        public override void Reset()
        {
            if (mUnityWebRequest != null)
            {
                mUnityWebRequest.Dispose();
                mUnityWebRequest = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (mDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (mUnityWebRequest != null)
                {
                    mUnityWebRequest.Dispose();
                    mUnityWebRequest = null;
                }
            }

            mDisposed = true;
        }

        private void Update()
        {
            if (mUnityWebRequest == null || !mUnityWebRequest.isDone)
            {
                return;
            }

            bool isError = false;
#if UNITY_2020_2_OR_NEWER
            isError = mUnityWebRequest.result != UnityWebRequest.Result.Success;
#elif UNITY_2017_1_OR_NEWER
            isError = mUnityWebRequest.isNetworkError || mUnityWebRequest.isHttpError;
#else
            isError = mUnityWebRequest.isError;
#endif
            if (isError)
            {
                WebRequestAgentHelperErrorEventArgs webRequestAgentHelperErrorEventArgs = WebRequestAgentHelperErrorEventArgs.Create(mUnityWebRequest.error);
                mWebRequestAgentHelperErrorEventHandler(this, webRequestAgentHelperErrorEventArgs);
                ReferencePool.Release(webRequestAgentHelperErrorEventArgs);
            }
            else if (mUnityWebRequest.downloadHandler.isDone)
            {
                WebRequestAgentHelperCompleteEventArgs webRequestAgentHelperCompleteEventArgs = WebRequestAgentHelperCompleteEventArgs.Create(mUnityWebRequest.downloadHandler.data);
                mWebRequestAgentHelperCompleteEventHandler(this, webRequestAgentHelperCompleteEventArgs);
                ReferencePool.Release(webRequestAgentHelperCompleteEventArgs);
            }
        }
    }
}
