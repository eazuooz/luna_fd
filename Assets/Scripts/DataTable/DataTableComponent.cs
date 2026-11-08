//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.DataTable;
using GameFramework.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Data Table")]
    public sealed class DataTableComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private IDataTableManager mDataTableManager = null;
        private EventComponent mEventComponent = null;

        [SerializeField]
        private bool mEnableLoadDataTableUpdateEvent = false;

        [SerializeField]
        private bool mEnableLoadDataTableDependencyAssetEvent = false;

        [SerializeField]
        private string mDataTableHelperTypeName = "UnityGameFramework.Runtime.DefaultDataTableHelper";

        [SerializeField]
        private DataTableHelperBase mCustomDataTableHelper = null;

        [SerializeField]
        private int mCachedBytesSize = 0;

        public int Count
        {
            get
            {
                return mDataTableManager.Count;
            }
        }

        public int CachedBytesSize
        {
            get
            {
                return mDataTableManager.CachedBytesSize;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mDataTableManager = GameFrameworkEntry.GetModule<IDataTableManager>();
            if (mDataTableManager == null)
            {
                Log.Fatal("Data table manager is invalid.");
                return;
            }
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            mEventComponent = GameEntry.GetComponent<EventComponent>();
            if (mEventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            mDataTableManager.SetResourceManager(GameEntry.GetComponent<ResourceComponent>().ResourceManager);

            DataTableHelperBase dataTableHelper = Helper.CreateHelper(mDataTableHelperTypeName, mCustomDataTableHelper);
            if (dataTableHelper == null)
            {
                Log.Error("Can not create data table helper.");
                return;
            }

            dataTableHelper.name = "Data Table Helper";
            Transform transform = dataTableHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            mDataTableManager.SetDataProviderHelper(dataTableHelper);
            mDataTableManager.SetDataTableHelper(dataTableHelper);
            if (mCachedBytesSize > 0)
            {
                EnsureCachedBytesSize(mCachedBytesSize);
            }
        }

        public void EnsureCachedBytesSize(int ensureSize)
        {
            mDataTableManager.EnsureCachedBytesSize(ensureSize);
        }

        public void FreeCachedBytes()
        {
            mDataTableManager.FreeCachedBytes();
        }

        public bool HasDataTable<T>() where T : IDataRow
        {
            return mDataTableManager.HasDataTable<T>();
        }

        public bool HasDataTable(Type dataRowType)
        {
            return mDataTableManager.HasDataTable(dataRowType);
        }

        public bool HasDataTable<T>(string name) where T : IDataRow
        {
            return mDataTableManager.HasDataTable<T>(name);
        }

        public bool HasDataTable(Type dataRowType, string name)
        {
            return mDataTableManager.HasDataTable(dataRowType, name);
        }

        public IDataTable<T> GetDataTable<T>() where T : IDataRow
        {
            return mDataTableManager.GetDataTable<T>();
        }

        public DataTableBase GetDataTable(Type dataRowType)
        {
            return mDataTableManager.GetDataTable(dataRowType);
        }

        public IDataTable<T> GetDataTable<T>(string name) where T : IDataRow
        {
            return mDataTableManager.GetDataTable<T>(name);
        }

        public DataTableBase GetDataTable(Type dataRowType, string name)
        {
            return mDataTableManager.GetDataTable(dataRowType, name);
        }

        public DataTableBase[] GetAllDataTables()
        {
            return mDataTableManager.GetAllDataTables();
        }

        public void GetAllDataTables(List<DataTableBase> results)
        {
            mDataTableManager.GetAllDataTables(results);
        }

        public IDataTable<T> CreateDataTable<T>() where T : class, IDataRow, new()
        {
            return CreateDataTable<T>(null);
        }

        public DataTableBase CreateDataTable(Type dataRowType)
        {
            return CreateDataTable(dataRowType, null);
        }

        public IDataTable<T> CreateDataTable<T>(string name) where T : class, IDataRow, new()
        {
            IDataTable<T> dataTable = mDataTableManager.CreateDataTable<T>(name);
            DataTableBase dataTableBase = (DataTableBase)dataTable;
            dataTableBase.ReadDataSuccess += OnReadDataSuccess;
            dataTableBase.ReadDataFailure += OnReadDataFailure;

            if (mEnableLoadDataTableUpdateEvent)
            {
                dataTableBase.ReadDataUpdate += OnReadDataUpdate;
            }

            if (mEnableLoadDataTableDependencyAssetEvent)
            {
                dataTableBase.ReadDataDependencyAsset += OnReadDataDependencyAsset;
            }

            return dataTable;
        }

        public DataTableBase CreateDataTable(Type dataRowType, string name)
        {
            DataTableBase dataTable = mDataTableManager.CreateDataTable(dataRowType, name);
            dataTable.ReadDataSuccess += OnReadDataSuccess;
            dataTable.ReadDataFailure += OnReadDataFailure;

            if (mEnableLoadDataTableUpdateEvent)
            {
                dataTable.ReadDataUpdate += OnReadDataUpdate;
            }

            if (mEnableLoadDataTableDependencyAssetEvent)
            {
                dataTable.ReadDataDependencyAsset += OnReadDataDependencyAsset;
            }

            return dataTable;
        }

        public bool DestroyDataTable<T>() where T : IDataRow, new()
        {
            return mDataTableManager.DestroyDataTable<T>();
        }

        public bool DestroyDataTable(Type dataRowType)
        {
            return mDataTableManager.DestroyDataTable(dataRowType);
        }

        public bool DestroyDataTable<T>(string name) where T : IDataRow
        {
            return mDataTableManager.DestroyDataTable<T>(name);
        }

        public bool DestroyDataTable(Type dataRowType, string name)
        {
            return mDataTableManager.DestroyDataTable(dataRowType, name);
        }

        public bool DestroyDataTable<T>(IDataTable<T> dataTable) where T : IDataRow
        {
            return mDataTableManager.DestroyDataTable(dataTable);
        }

        public bool DestroyDataTable(DataTableBase dataTable)
        {
            return mDataTableManager.DestroyDataTable(dataTable);
        }

        private void OnReadDataSuccess(object sender, ReadDataSuccessEventArgs e)
        {
            mEventComponent.Fire(this, LoadDataTableSuccessEventArgs.Create(e));
        }

        private void OnReadDataFailure(object sender, ReadDataFailureEventArgs e)
        {
            Log.Warning("Load data table failure, asset name '{0}', error message '{1}'.", e.DataAssetName, e.ErrorMessage);
            mEventComponent.Fire(this, LoadDataTableFailureEventArgs.Create(e));
        }

        private void OnReadDataUpdate(object sender, ReadDataUpdateEventArgs e)
        {
            mEventComponent.Fire(this, LoadDataTableUpdateEventArgs.Create(e));
        }

        private void OnReadDataDependencyAsset(object sender, ReadDataDependencyAssetEventArgs e)
        {
            mEventComponent.Fire(this, LoadDataTableDependencyAssetEventArgs.Create(e));
        }
    }
}
