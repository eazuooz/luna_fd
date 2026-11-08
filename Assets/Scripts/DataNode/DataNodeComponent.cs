//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.DataNode;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Data Node")]
    public sealed class DataNodeComponent : GameFrameworkComponent
    {
        private IDataNodeManager mDataNodeManager = null;

        public IDataNode Root
        {
            get
            {
                return mDataNodeManager.Root;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mDataNodeManager = GameFrameworkEntry.GetModule<IDataNodeManager>();
            if (mDataNodeManager == null)
            {
                Log.Fatal("Data node manager is invalid.");
                return;
            }
        }

        private void Start()
        {
        }

        public T GetData<T>(string path) where T : Variable
        {
            return mDataNodeManager.GetData<T>(path);
        }

        public Variable GetData(string path)
        {
            return mDataNodeManager.GetData(path);
        }

        public T GetData<T>(string path, IDataNode node) where T : Variable
        {
            return mDataNodeManager.GetData<T>(path, node);
        }

        public Variable GetData(string path, IDataNode node)
        {
            return mDataNodeManager.GetData(path, node);
        }

        public void SetData<T>(string path, T data) where T : Variable
        {
            mDataNodeManager.SetData(path, data);
        }

        public void SetData(string path, Variable data)
        {
            mDataNodeManager.SetData(path, data);
        }

        public void SetData<T>(string path, T data, IDataNode node) where T : Variable
        {
            mDataNodeManager.SetData(path, data, node);
        }

        public void SetData(string path, Variable data, IDataNode node)
        {
            mDataNodeManager.SetData(path, data, node);
        }

        public IDataNode GetNode(string path)
        {
            return mDataNodeManager.GetNode(path);
        }

        public IDataNode GetNode(string path, IDataNode node)
        {
            return mDataNodeManager.GetNode(path, node);
        }

        public IDataNode GetOrAddNode(string path)
        {
            return mDataNodeManager.GetOrAddNode(path);
        }

        public IDataNode GetOrAddNode(string path, IDataNode node)
        {
            return mDataNodeManager.GetOrAddNode(path, node);
        }

        public void RemoveNode(string path)
        {
            mDataNodeManager.RemoveNode(path);
        }

        public void RemoveNode(string path, IDataNode node)
        {
            mDataNodeManager.RemoveNode(path, node);
        }

        public void Clear()
        {
            mDataNodeManager.Clear();
        }
    }
}
