//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework.Download;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class DownloadAgentHelperBase : MonoBehaviour, IDownloadAgentHelper
    {
        protected const int RangeNotSatisfiableErrorCode = 416;

        public abstract event EventHandler<DownloadAgentHelperUpdateBytesEventArgs> DownloadAgentHelperUpdateBytes;

        public abstract event EventHandler<DownloadAgentHelperUpdateLengthEventArgs> DownloadAgentHelperUpdateLength;

        public abstract event EventHandler<DownloadAgentHelperCompleteEventArgs> DownloadAgentHelperComplete;

        public abstract event EventHandler<DownloadAgentHelperErrorEventArgs> DownloadAgentHelperError;

        public abstract void Download(string downloadUri, object userData);

        public abstract void Download(string downloadUri, long fromPosition, object userData);

        public abstract void Download(string downloadUri, long fromPosition, long toPosition, object userData);

        public abstract void Reset();
    }
}
