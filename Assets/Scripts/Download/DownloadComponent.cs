//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Download;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Download")]
    public sealed class DownloadComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;
        private const int OneMegaBytes = 1024 * 1024;

        private IDownloadManager mDownloadManager = null;
        private EventComponent mEventComponent = null;

        [SerializeField]
        private Transform mInstanceRoot = null;

        [SerializeField]
        private string mDownloadAgentHelperTypeName = "UnityGameFramework.Runtime.UnityWebRequestDownloadAgentHelper";

        [SerializeField]
        private DownloadAgentHelperBase mCustomDownloadAgentHelper = null;

        [SerializeField]
        private int mDownloadAgentHelperCount = 3;

        [SerializeField]
        private float mTimeout = 30f;

        [SerializeField]
        private int mFlushSize = OneMegaBytes;

        public bool Paused
        {
            get
            {
                return mDownloadManager.Paused;
            }
            set
            {
                mDownloadManager.Paused = value;
            }
        }

        public int TotalAgentCount
        {
            get
            {
                return mDownloadManager.TotalAgentCount;
            }
        }

        public int FreeAgentCount
        {
            get
            {
                return mDownloadManager.FreeAgentCount;
            }
        }

        public int WorkingAgentCount
        {
            get
            {
                return mDownloadManager.WorkingAgentCount;
            }
        }

        public int WaitingTaskCount
        {
            get
            {
                return mDownloadManager.WaitingTaskCount;
            }
        }

        public float Timeout
        {
            get
            {
                return mDownloadManager.Timeout;
            }
            set
            {
                mDownloadManager.Timeout = mTimeout = value;
            }
        }

        public int FlushSize
        {
            get
            {
                return mDownloadManager.FlushSize;
            }
            set
            {
                mDownloadManager.FlushSize = mFlushSize = value;
            }
        }

        public float CurrentSpeed
        {
            get
            {
                return mDownloadManager.CurrentSpeed;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mDownloadManager = GameFrameworkEntry.GetModule<IDownloadManager>();
            if (mDownloadManager == null)
            {
                Log.Fatal("Download manager is invalid.");
                return;
            }

            mDownloadManager.DownloadStart += OnDownloadStart;
            mDownloadManager.DownloadUpdate += OnDownloadUpdate;
            mDownloadManager.DownloadSuccess += OnDownloadSuccess;
            mDownloadManager.DownloadFailure += OnDownloadFailure;
            mDownloadManager.FlushSize = mFlushSize;
            mDownloadManager.Timeout = mTimeout;
        }

        private void Start()
        {
            mEventComponent = GameEntry.GetComponent<EventComponent>();
            if (mEventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            if (mInstanceRoot == null)
            {
                mInstanceRoot = new GameObject("Download Agent Instances").transform;
                mInstanceRoot.SetParent(gameObject.transform);
                mInstanceRoot.localScale = Vector3.one;
            }

            for (int i = 0; i < mDownloadAgentHelperCount; i++)
            {
                AddDownloadAgentHelper(i);
            }
        }

        public TaskInfo GetDownloadInfo(int serialId)
        {
            return mDownloadManager.GetDownloadInfo(serialId);
        }

        public TaskInfo[] GetDownloadInfos(string tag)
        {
            return mDownloadManager.GetDownloadInfos(tag);
        }

        public void GetDownloadInfos(string tag, List<TaskInfo> results)
        {
            mDownloadManager.GetDownloadInfos(tag, results);
        }

        public TaskInfo[] GetAllDownloadInfos()
        {
            return mDownloadManager.GetAllDownloadInfos();
        }

        public void GetAllDownloadInfos(List<TaskInfo> results)
        {
            mDownloadManager.GetAllDownloadInfos(results);
        }

        public int AddDownload(string downloadPath, string downloadUri)
        {
            return AddDownload(downloadPath, downloadUri, null, DefaultPriority, null);
        }

        public int AddDownload(string downloadPath, string downloadUri, string tag)
        {
            return AddDownload(downloadPath, downloadUri, tag, DefaultPriority, null);
        }

        public int AddDownload(string downloadPath, string downloadUri, int priority)
        {
            return AddDownload(downloadPath, downloadUri, null, priority, null);
        }

        public int AddDownload(string downloadPath, string downloadUri, object userData)
        {
            return AddDownload(downloadPath, downloadUri, null, DefaultPriority, userData);
        }

        public int AddDownload(string downloadPath, string downloadUri, string tag, int priority)
        {
            return AddDownload(downloadPath, downloadUri, tag, priority, null);
        }

        public int AddDownload(string downloadPath, string downloadUri, string tag, object userData)
        {
            return AddDownload(downloadPath, downloadUri, tag, DefaultPriority, userData);
        }

        public int AddDownload(string downloadPath, string downloadUri, int priority, object userData)
        {
            return AddDownload(downloadPath, downloadUri, null, priority, userData);
        }

        public int AddDownload(string downloadPath, string downloadUri, string tag, int priority, object userData)
        {
            return mDownloadManager.AddDownload(downloadPath, downloadUri, tag, priority, userData);
        }

        public bool RemoveDownload(int serialId)
        {
            return mDownloadManager.RemoveDownload(serialId);
        }

        public int RemoveDownloads(string tag)
        {
            return mDownloadManager.RemoveDownloads(tag);
        }

        public int RemoveAllDownloads()
        {
            return mDownloadManager.RemoveAllDownloads();
        }

        private void AddDownloadAgentHelper(int index)
        {
            DownloadAgentHelperBase downloadAgentHelper = Helper.CreateHelper(mDownloadAgentHelperTypeName, mCustomDownloadAgentHelper, index);
            if (downloadAgentHelper == null)
            {
                Log.Error("Can not create download agent helper.");
                return;
            }

            downloadAgentHelper.name = Utility.Text.Format("Download Agent Helper - {0}", index);
            Transform transform = downloadAgentHelper.transform;
            transform.SetParent(mInstanceRoot);
            transform.localScale = Vector3.one;

            mDownloadManager.AddDownloadAgentHelper(downloadAgentHelper);
        }

        private void OnDownloadStart(object sender, GameFramework.Download.DownloadStartEventArgs e)
        {
            mEventComponent.Fire(this, DownloadStartEventArgs.Create(e));
        }

        private void OnDownloadUpdate(object sender, GameFramework.Download.DownloadUpdateEventArgs e)
        {
            mEventComponent.Fire(this, DownloadUpdateEventArgs.Create(e));
        }

        private void OnDownloadSuccess(object sender, GameFramework.Download.DownloadSuccessEventArgs e)
        {
            mEventComponent.Fire(this, DownloadSuccessEventArgs.Create(e));
        }

        private void OnDownloadFailure(object sender, GameFramework.Download.DownloadFailureEventArgs e)
        {
            Log.Warning("Download failure, download serial id '{0}', download path '{1}', download uri '{2}', error message '{3}'.", e.SerialId, e.DownloadPath, e.DownloadUri, e.ErrorMessage);
            mEventComponent.Fire(this, DownloadFailureEventArgs.Create(e));
        }
    }
}
