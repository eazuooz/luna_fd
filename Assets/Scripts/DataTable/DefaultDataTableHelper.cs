//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.DataTable;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class DefaultDataTableHelper : DataTableHelperBase
    {
        private static readonly string BytesAssetExtension = ".bytes";

        private ResourceComponent mResourceComponent = null;

        public override bool ReadData(DataTableBase dataTable, string dataTableAssetName, object dataTableAsset, object userData)
        {
            TextAsset dataTableTextAsset = dataTableAsset as TextAsset;
            if (dataTableTextAsset != null)
            {
                if (dataTableAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
                {
                    return dataTable.ParseData(dataTableTextAsset.bytes, userData);
                }
                else
                {
                    return dataTable.ParseData(dataTableTextAsset.text, userData);
                }
            }

            Log.Warning("Data table asset '{0}' is invalid.", dataTableAssetName);
            return false;
        }

        public override bool ReadData(DataTableBase dataTable, string dataTableAssetName, byte[] dataTableBytes, int startIndex, int length, object userData)
        {
            if (dataTableAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
            {
                return dataTable.ParseData(dataTableBytes, startIndex, length, userData);
            }
            else
            {
                return dataTable.ParseData(Utility.Converter.GetString(dataTableBytes, startIndex, length), userData);
            }
        }

        public override bool ParseData(DataTableBase dataTable, string dataTableString, object userData)
        {
            try
            {
                int position = 0;
                string dataRowString = null;
                while ((dataRowString = dataTableString.ReadLine(ref position)) != null)
                {
                    if (dataRowString[0] == '#')
                    {
                        continue;
                    }

                    if (!dataTable.AddDataRow(dataRowString, userData))
                    {
                        Log.Warning("Can not parse data row string '{0}'.", dataRowString);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse data table string with exception '{0}'.", exception);
                return false;
            }
        }

        public override bool ParseData(DataTableBase dataTable, byte[] dataTableBytes, int startIndex, int length, object userData)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(dataTableBytes, startIndex, length, false))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                    {
                        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                        {
                            int dataRowBytesLength = binaryReader.Read7BitEncodedInt32();
                            if (!dataTable.AddDataRow(dataTableBytes, (int)binaryReader.BaseStream.Position, dataRowBytesLength, userData))
                            {
                                Log.Warning("Can not parse data row bytes.");
                                return false;
                            }

                            binaryReader.BaseStream.Position += dataRowBytesLength;
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary bytes with exception '{0}'.", exception);
                return false;
            }
        }

        public override void ReleaseDataAsset(DataTableBase dataTable, object dataTableAsset)
        {
            mResourceComponent.UnloadAsset(dataTableAsset);
        }

        private void Start()
        {
            mResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            if (mResourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }
    }
}
