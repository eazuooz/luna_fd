//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


#if !UNITY_2018_3_OR_NEWER

using GameFramework;
using GameFramework.WebRequest;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class WWWWebRequestAgentHelper : WebRequestAgentHelperBase, IDisposable
    {
        private WWW mWWW = null;
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
                mWWW = new WWW(webRequestUri);
            }
            else
            {
                mWWW = new WWW(webRequestUri, wwwFormInfo.WWWForm);
            }
        }

        public override void Request(string webRequestUri, byte[] postData, object userData)
        {
            if (mWebRequestAgentHelperCompleteEventHandler == null || mWebRequestAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Web request agent helper handler is invalid.");
                return;
            }

            mWWW = new WWW(webRequestUri, postData);
        }

        public override void Reset()
        {
            if (mWWW != null)
            {
                mWWW.Dispose();
                mWWW = null;
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
                if (mWWW != null)
                {
                    mWWW.Dispose();
                    mWWW = null;
                }
            }

            mDisposed = true;
        }

        private void Update()
        {
            if (mWWW == null || !mWWW.isDone)
            {
                return;
            }

            if (!string.IsNullOrEmpty(mWWW.error))
            {
                WebRequestAgentHelperErrorEventArgs webRequestAgentHelperErrorEventArgs = WebRequestAgentHelperErrorEventArgs.Create(mWWW.error);
                mWebRequestAgentHelperErrorEventHandler(this, webRequestAgentHelperErrorEventArgs);
                ReferencePool.Release(webRequestAgentHelperErrorEventArgs);
            }
            else
            {
                WebRequestAgentHelperCompleteEventArgs webRequestAgentHelperCompleteEventArgs = WebRequestAgentHelperCompleteEventArgs.Create(mWWW.bytes);
                mWebRequestAgentHelperCompleteEventHandler(this, webRequestAgentHelperCompleteEventArgs);
                ReferencePool.Release(webRequestAgentHelperCompleteEventArgs);
            }
        }
    }
}

#endif
