//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Download;
using System;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif
using Utility = GameFramework.Utility;

namespace UnityGameFramework.Runtime
{
    public partial class UnityWebRequestDownloadAgentHelper : DownloadAgentHelperBase, IDisposable
    {
        private const int CachedBytesLength = 0x1000;
        private readonly byte[] mCachedBytes = new byte[CachedBytesLength];

        private UnityWebRequest mUnityWebRequest = null;
        private bool mDisposed = false;

        private EventHandler<DownloadAgentHelperUpdateBytesEventArgs> mDownloadAgentHelperUpdateBytesEventHandler = null;
        private EventHandler<DownloadAgentHelperUpdateLengthEventArgs> mDownloadAgentHelperUpdateLengthEventHandler = null;
        private EventHandler<DownloadAgentHelperCompleteEventArgs> mDownloadAgentHelperCompleteEventHandler = null;
        private EventHandler<DownloadAgentHelperErrorEventArgs> mDownloadAgentHelperErrorEventHandler = null;

        public override event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes
        {
            add
            {
                mDownloadAgentHelperUpdateBytesEventHandler += value;
            }
            remove
            {
                mDownloadAgentHelperUpdateBytesEventHandler -= value;
            }
        }

        public override event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength
        {
            add
            {
                mDownloadAgentHelperUpdateLengthEventHandler += value;
            }
            remove
            {
                mDownloadAgentHelperUpdateLengthEventHandler -= value;
            }
        }

        public override event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete
        {
            add
            {
                mDownloadAgentHelperCompleteEventHandler += value;
            }
            remove
            {
                mDownloadAgentHelperCompleteEventHandler -= value;
            }
        }

        public override event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError
        {
            add
            {
                mDownloadAgentHelperErrorEventHandler += value;
            }
            remove
            {
                mDownloadAgentHelperErrorEventHandler -= value;
            }
        }

        public override void Download(string downloadUri, object userData)
        {
            if (mDownloadAgentHelperUpdateBytesEventHandler == null || mDownloadAgentHelperUpdateLengthEventHandler == null || mDownloadAgentHelperCompleteEventHandler == null || mDownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            mUnityWebRequest = new UnityWebRequest(downloadUri);
            mUnityWebRequest.downloadHandler = new DownloadHandler(this);
#if UNITY_2017_2_OR_NEWER
            mUnityWebRequest.SendWebRequest();
#else
            mUnityWebRequest.Send();
#endif
        }

        public override void Download(string downloadUri, long fromPosition, object userData)
        {
            if (mDownloadAgentHelperUpdateBytesEventHandler == null || mDownloadAgentHelperUpdateLengthEventHandler == null || mDownloadAgentHelperCompleteEventHandler == null || mDownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            mUnityWebRequest = new UnityWebRequest(downloadUri);
            mUnityWebRequest.SetRequestHeader("Range", Utility.Text.Format("bytes={0}-", fromPosition));
            mUnityWebRequest.downloadHandler = new DownloadHandler(this);
#if UNITY_2017_2_OR_NEWER
            mUnityWebRequest.SendWebRequest();
#else
            mUnityWebRequest.Send();
#endif
        }

        public override void Download(string downloadUri, long fromPosition, long toPosition, object userData)
        {
            if (mDownloadAgentHelperUpdateBytesEventHandler == null || mDownloadAgentHelperUpdateLengthEventHandler == null || mDownloadAgentHelperCompleteEventHandler == null || mDownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            mUnityWebRequest = new UnityWebRequest(downloadUri);
            mUnityWebRequest.SetRequestHeader("Range", Utility.Text.Format("bytes={0}-{1}", fromPosition, toPosition));
            mUnityWebRequest.downloadHandler = new DownloadHandler(this);
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
                mUnityWebRequest.Abort();
                mUnityWebRequest.Dispose();
                mUnityWebRequest = null;
            }

            Array.Clear(mCachedBytes, 0, CachedBytesLength);
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
            if (mUnityWebRequest == null)
            {
                return;
            }

            if (!mUnityWebRequest.isDone)
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
                DownloadAgentHelperErrorEventArgs downloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(mUnityWebRequest.responseCode == RangeNotSatisfiableErrorCode, mUnityWebRequest.error);
                mDownloadAgentHelperErrorEventHandler(this, downloadAgentHelperErrorEventArgs);
                ReferencePool.Release(downloadAgentHelperErrorEventArgs);
            }
            else
            {
                DownloadAgentHelperCompleteEventArgs downloadAgentHelperCompleteEventArgs = DownloadAgentHelperCompleteEventArgs.Create((long)mUnityWebRequest.downloadedBytes);
                mDownloadAgentHelperCompleteEventHandler(this, downloadAgentHelperCompleteEventArgs);
                ReferencePool.Release(downloadAgentHelperCompleteEventArgs);
            }
        }
    }
}
