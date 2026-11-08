//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.WebRequest;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Web Request")]
    public sealed class WebRequestComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private IWebRequestManager mWebRequestManager = null;
        private EventComponent mEventComponent = null;

        [SerializeField]
        private Transform mInstanceRoot = null;

        [SerializeField]
        private string mWebRequestAgentHelperTypeName = "UnityGameFramework.Runtime.UnityWebRequestAgentHelper";

        [SerializeField]
        private WebRequestAgentHelperBase mCustomWebRequestAgentHelper = null;

        [SerializeField]
        private int mWebRequestAgentHelperCount = 1;

        [SerializeField]
        private float mTimeout = 30f;

        public int TotalAgentCount
        {
            get
            {
                return mWebRequestManager.TotalAgentCount;
            }
        }

        public int FreeAgentCount
        {
            get
            {
                return mWebRequestManager.FreeAgentCount;
            }
        }

        public int WorkingAgentCount
        {
            get
            {
                return mWebRequestManager.WorkingAgentCount;
            }
        }

        public int WaitingTaskCount
        {
            get
            {
                return mWebRequestManager.WaitingTaskCount;
            }
        }

        public float Timeout
        {
            get
            {
                return mWebRequestManager.Timeout;
            }
            set
            {
                mWebRequestManager.Timeout = mTimeout = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mWebRequestManager = GameFrameworkEntry.GetModule<IWebRequestManager>();
            if (mWebRequestManager == null)
            {
                Log.Fatal("Web request manager is invalid.");
                return;
            }

            mWebRequestManager.Timeout = mTimeout;
            mWebRequestManager.WebRequestStart += OnWebRequestStart;
            mWebRequestManager.WebRequestSuccess += OnWebRequestSuccess;
            mWebRequestManager.WebRequestFailure += OnWebRequestFailure;
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
                mInstanceRoot = new GameObject("Web Request Agent Instances").transform;
                mInstanceRoot.SetParent(gameObject.transform);
                mInstanceRoot.localScale = Vector3.one;
            }

            for (int i = 0; i < mWebRequestAgentHelperCount; i++)
            {
                AddWebRequestAgentHelper(i);
            }
        }

        public TaskInfo GetWebRequestInfo(int serialId)
        {
            return mWebRequestManager.GetWebRequestInfo(serialId);
        }

        public TaskInfo[] GetWebRequestInfos(string tag)
        {
            return mWebRequestManager.GetWebRequestInfos(tag);
        }

        public void GetAllWebRequestInfos(string tag, List<TaskInfo> results)
        {
            mWebRequestManager.GetAllWebRequestInfos(tag, results);
        }

        public TaskInfo[] GetAllWebRequestInfos()
        {
            return mWebRequestManager.GetAllWebRequestInfos();
        }

        public void GetAllWebRequestInfos(List<TaskInfo> results)
        {
            mWebRequestManager.GetAllWebRequestInfos(results);
        }

        public int AddWebRequest(string webRequestUri)
        {
            return AddWebRequest(webRequestUri, null, null, null, DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData)
        {
            return AddWebRequest(webRequestUri, postData, null, null, DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, WWWForm wwwForm)
        {
            return AddWebRequest(webRequestUri, null, wwwForm, null, DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, string tag)
        {
            return AddWebRequest(webRequestUri, null, null, tag, DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, int priority)
        {
            return AddWebRequest(webRequestUri, null, null, null, priority, null);
        }

        public int AddWebRequest(string webRequestUri, object userData)
        {
            return AddWebRequest(webRequestUri, null, null, null, DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, string tag)
        {
            return AddWebRequest(webRequestUri, postData, null, tag, DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, WWWForm wwwForm, string tag)
        {
            return AddWebRequest(webRequestUri, null, wwwForm, tag, DefaultPriority, null);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, int priority)
        {
            return AddWebRequest(webRequestUri, postData, null, null, priority, null);
        }

        public int AddWebRequest(string webRequestUri, WWWForm wwwForm, int priority)
        {
            return AddWebRequest(webRequestUri, null, wwwForm, null, priority, null);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, object userData)
        {
            return AddWebRequest(webRequestUri, postData, null, null, DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, WWWForm wwwForm, object userData)
        {
            return AddWebRequest(webRequestUri, null, wwwForm, null, DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, string tag, int priority)
        {
            return AddWebRequest(webRequestUri, null, null, tag, priority, null);
        }

        public int AddWebRequest(string webRequestUri, string tag, object userData)
        {
            return AddWebRequest(webRequestUri, null, null, tag, DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, null, null, null, priority, userData);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, string tag, int priority)
        {
            return AddWebRequest(webRequestUri, postData, null, tag, priority, null);
        }

        public int AddWebRequest(string webRequestUri, WWWForm wwwForm, string tag, int priority)
        {
            return AddWebRequest(webRequestUri, null, wwwForm, tag, priority, null);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, string tag, object userData)
        {
            return AddWebRequest(webRequestUri, postData, null, tag, DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, WWWForm wwwForm, string tag, object userData)
        {
            return AddWebRequest(webRequestUri, null, wwwForm, tag, DefaultPriority, userData);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, postData, null, null, priority, userData);
        }

        public int AddWebRequest(string webRequestUri, WWWForm wwwForm, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, null, wwwForm, null, priority, userData);
        }

        public int AddWebRequest(string webRequestUri, string tag, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, null, null, tag, priority, userData);
        }

        public int AddWebRequest(string webRequestUri, byte[] postData, string tag, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, postData, null, tag, priority, userData);
        }

        public int AddWebRequest(string webRequestUri, WWWForm wwwForm, string tag, int priority, object userData)
        {
            return AddWebRequest(webRequestUri, null, wwwForm, tag, priority, userData);
        }

        public bool RemoveWebRequest(int serialId)
        {
            return mWebRequestManager.RemoveWebRequest(serialId);
        }

        public int RemoveWebRequests(string tag)
        {
            return mWebRequestManager.RemoveWebRequests(tag);
        }

        public int RemoveAllWebRequests()
        {
            return mWebRequestManager.RemoveAllWebRequests();
        }

        private void AddWebRequestAgentHelper(int index)
        {
            WebRequestAgentHelperBase webRequestAgentHelper = Helper.CreateHelper(mWebRequestAgentHelperTypeName, mCustomWebRequestAgentHelper, index);
            if (webRequestAgentHelper == null)
            {
                Log.Error("Can not create web request agent helper.");
                return;
            }

            webRequestAgentHelper.name = Utility.Text.Format("Web Request Agent Helper - {0}", index);
            Transform transform = webRequestAgentHelper.transform;
            transform.SetParent(mInstanceRoot);
            transform.localScale = Vector3.one;

            mWebRequestManager.AddWebRequestAgentHelper(webRequestAgentHelper);
        }

        private int AddWebRequest(string webRequestUri, byte[] postData, WWWForm wwwForm, string tag, int priority, object userData)
        {
            return mWebRequestManager.AddWebRequest(webRequestUri, postData, tag, priority, WWWFormInfo.Create(wwwForm, userData));
        }

        private void OnWebRequestStart(object sender, GameFramework.WebRequest.WebRequestStartEventArgs e)
        {
            mEventComponent.Fire(this, WebRequestStartEventArgs.Create(e));
        }

        private void OnWebRequestSuccess(object sender, GameFramework.WebRequest.WebRequestSuccessEventArgs e)
        {
            mEventComponent.Fire(this, WebRequestSuccessEventArgs.Create(e));
        }

        private void OnWebRequestFailure(object sender, GameFramework.WebRequest.WebRequestFailureEventArgs e)
        {
            Log.Warning("Web request failure, web request serial id '{0}', web request uri '{1}', error message '{2}'.", e.SerialId, e.WebRequestUri, e.ErrorMessage);
            mEventComponent.Fire(this, WebRequestFailureEventArgs.Create(e));
        }
    }
}
