//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.DataTable;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class DataTableHelperBase : MonoBehaviour, IDataProviderHelper<DataTableBase>, IDataTableHelper
    {
        public abstract bool ReadData(DataTableBase dataTable, string dataTableAssetName, object dataTableAsset, object userData);

        public abstract bool ReadData(DataTableBase dataTable, string dataTableAssetName, byte[] dataTableBytes, int startIndex, int length, object userData);

        public abstract bool ParseData(DataTableBase dataTable, string dataTableString, object userData);

        public abstract bool ParseData(DataTableBase dataTable, byte[] dataTableBytes, int startIndex, int length, object userData);

        public abstract void ReleaseDataAsset(DataTableBase dataTable, object dataTableAsset);
    }
}
