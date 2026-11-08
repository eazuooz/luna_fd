//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


#if !UNITY_2018_3_OR_NEWER

using GameFramework;
using GameFramework.Download;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class WWWDownloadAgentHelper : DownloadAgentHelperBase, IDisposable
    {
        private WWW mWWW = null;
        private int mLastDownloadedSize = 0;
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

            mWWW = new WWW(downloadUri);
        }

        public override void Download(string downloadUri, long fromPosition, object userData)
        {
            if (mDownloadAgentHelperUpdateBytesEventHandler == null || mDownloadAgentHelperUpdateLengthEventHandler == null || mDownloadAgentHelperCompleteEventHandler == null || mDownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "Range", Utility.Text.Format("bytes={0}-", fromPosition) }
            };

            mWWW = new WWW(downloadUri, null, header);
        }

        public override void Download(string downloadUri, long fromPosition, long toPosition, object userData)
        {
            if (mDownloadAgentHelperUpdateBytesEventHandler == null || mDownloadAgentHelperUpdateLengthEventHandler == null || mDownloadAgentHelperCompleteEventHandler == null || mDownloadAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Download agent helper handler is invalid.");
                return;
            }

            Dictionary<string, string> header = new Dictionary<string, string>
            {
                { "Range", Utility.Text.Format("bytes={0}-{1}", fromPosition, toPosition) }
            };

            mWWW = new WWW(downloadUri, null, header);
        }

        public override void Reset()
        {
            if (mWWW != null)
            {
                mWWW.Dispose();
                mWWW = null;
            }

            mLastDownloadedSize = 0;
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
            if (mWWW == null)
            {
                return;
            }

            int deltaLength = mWWW.bytesDownloaded - mLastDownloadedSize;
            if (deltaLength > 0)
            {
                mLastDownloadedSize = mWWW.bytesDownloaded;
                DownloadAgentHelperUpdateLengthEventArgs downloadAgentHelperUpdateLengthEventArgs = DownloadAgentHelperUpdateLengthEventArgs.Create(deltaLength);
                mDownloadAgentHelperUpdateLengthEventHandler(this, downloadAgentHelperUpdateLengthEventArgs);
                ReferencePool.Release(downloadAgentHelperUpdateLengthEventArgs);
            }

            if (mWWW == null)
            {
                return;
            }

            if (!mWWW.isDone)
            {
                return;
            }

            if (!string.IsNullOrEmpty(mWWW.error))
            {
                DownloadAgentHelperErrorEventArgs dodwnloadAgentHelperErrorEventArgs = DownloadAgentHelperErrorEventArgs.Create(mWWW.error.StartsWith(RangeNotSatisfiableErrorCode.ToString(), StringComparison.Ordinal), mWWW.error);
                mDownloadAgentHelperErrorEventHandler(this, dodwnloadAgentHelperErrorEventArgs);
                ReferencePool.Release(dodwnloadAgentHelperErrorEventArgs);
            }
            else
            {
                byte[] bytes = mWWW.bytes;
                DownloadAgentHelperUpdateBytesEventArgs downloadAgentHelperUpdateBytesEventArgs = DownloadAgentHelperUpdateBytesEventArgs.Create(bytes, 0, bytes.Length);
                mDownloadAgentHelperUpdateBytesEventHandler(this, downloadAgentHelperUpdateBytesEventArgs);
                ReferencePool.Release(downloadAgentHelperUpdateBytesEventArgs);

                DownloadAgentHelperCompleteEventArgs downloadAgentHelperCompleteEventArgs = DownloadAgentHelperCompleteEventArgs.Create(bytes.LongLength);
                mDownloadAgentHelperCompleteEventHandler(this, downloadAgentHelperCompleteEventArgs);
                ReferencePool.Release(downloadAgentHelperCompleteEventArgs);
            }
        }
    }
}

#endif
