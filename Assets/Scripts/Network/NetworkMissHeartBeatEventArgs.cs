//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Event;
using GameFramework.Network;

namespace UnityGameFramework.Runtime
{
    public sealed class NetworkMissHeartBeatEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(NetworkMissHeartBeatEventArgs).GetHashCode();

        public NetworkMissHeartBeatEventArgs()
        {
            NetworkChannel = null;
            MissCount = 0;
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        public int MissCount
        {
            get;
            private set;
        }

        public static NetworkMissHeartBeatEventArgs Create(GameFramework.Network.NetworkMissHeartBeatEventArgs e)
        {
            NetworkMissHeartBeatEventArgs networkMissHeartBeatEventArgs = ReferencePool.Acquire<NetworkMissHeartBeatEventArgs>();
            networkMissHeartBeatEventArgs.NetworkChannel = e.NetworkChannel;
            networkMissHeartBeatEventArgs.MissCount = e.MissCount;
            return networkMissHeartBeatEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
            MissCount = 0;
        }
    }
}
