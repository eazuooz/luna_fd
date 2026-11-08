//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework.DataTable;

namespace UnityGameFramework.Runtime
{
    public abstract class DataRowBase : IDataRow
    {
        public abstract int Id
        {
            get;
        }

        public virtual bool ParseDataRow(string dataRowString, object userData)
        {
            Log.Warning("Not implemented ParseDataRow(string dataRowString, object userData).");
            return false;
        }

        public virtual bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            Log.Warning("Not implemented ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData).");
            return false;
        }
    }
}
