//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Config;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class DefaultConfigHelper : ConfigHelperBase
    {
        private static readonly string[] ColumnSplitSeparator = new string[] { "\t" };
        private static readonly string BytesAssetExtension = ".bytes";
        private const int ColumnCount = 4;

        private ResourceComponent mResourceComponent = null;

        public override bool ReadData(IConfigManager configManager, string configAssetName, object configAsset, object userData)
        {
            TextAsset configTextAsset = configAsset as TextAsset;
            if (configTextAsset != null)
            {
                if (configAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
                {
                    return configManager.ParseData(configTextAsset.bytes, userData);
                }
                else
                {
                    return configManager.ParseData(configTextAsset.text, userData);
                }
            }

            Log.Warning("Config asset '{0}' is invalid.", configAssetName);
            return false;
        }

        public override bool ReadData(IConfigManager configManager, string configAssetName, byte[] configBytes, int startIndex, int length, object userData)
        {
            if (configAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
            {
                return configManager.ParseData(configBytes, startIndex, length, userData);
            }
            else
            {
                return configManager.ParseData(Utility.Converter.GetString(configBytes, startIndex, length), userData);
            }
        }

        public override bool ParseData(IConfigManager configManager, string configString, object userData)
        {
            try
            {
                int position = 0;
                string configLineString = null;
                while ((configLineString = configString.ReadLine(ref position)) != null)
                {
                    if (configLineString[0] == '#')
                    {
                        continue;
                    }

                    string[] splitedLine = configLineString.Split(ColumnSplitSeparator, StringSplitOptions.None);
                    if (splitedLine.Length != ColumnCount)
                    {
                        Log.Warning("Can not parse config line string '{0}' which column count is invalid.", configLineString);
                        return false;
                    }

                    string configName = splitedLine[1];
                    string configValue = splitedLine[3];
                    if (!configManager.AddConfig(configName, configValue))
                    {
                        Log.Warning("Can not add config with config name '{0}' which may be invalid or duplicate.", configName);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse config string with exception '{0}'.", exception);
                return false;
            }
        }

        public override bool ParseData(IConfigManager configManager, byte[] configBytes, int startIndex, int length, object userData)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(configBytes, startIndex, length, false))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                    {
                        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                        {
                            string configName = binaryReader.ReadString();
                            string configValue = binaryReader.ReadString();
                            if (!configManager.AddConfig(configName, configValue))
                            {
                                Log.Warning("Can not add config with config name '{0}' which may be invalid or duplicate.", configName);
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse config bytes with exception '{0}'.", exception);
                return false;
            }
        }

        public override void ReleaseDataAsset(IConfigManager configManager, object configAsset)
        {
            mResourceComponent.UnloadAsset(configAsset);
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
