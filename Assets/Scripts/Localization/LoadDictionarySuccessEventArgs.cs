//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class LoadDictionarySuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDictionarySuccessEventArgs).GetHashCode();

        public LoadDictionarySuccessEventArgs()
        {
            DictionaryAssetName = null;
            Duration = 0f;
            UserData = null;
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public string DictionaryAssetName
        {
            get;
            private set;
        }

        public float Duration
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static LoadDictionarySuccessEventArgs Create(ReadDataSuccessEventArgs e)
        {
            LoadDictionarySuccessEventArgs loadDictionarySuccessEventArgs = ReferencePool.Acquire<LoadDictionarySuccessEventArgs>();
            loadDictionarySuccessEventArgs.DictionaryAssetName = e.DataAssetName;
            loadDictionarySuccessEventArgs.Duration = e.Duration;
            loadDictionarySuccessEventArgs.UserData = e.UserData;
            return loadDictionarySuccessEventArgs;
        }

        public override void Clear()
        {
            DictionaryAssetName = null;
            Duration = 0f;
            UserData = null;
        }
    }
}
