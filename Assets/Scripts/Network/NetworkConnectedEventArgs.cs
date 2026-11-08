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
    public sealed class NetworkConnectedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(NetworkConnectedEventArgs).GetHashCode();

        public NetworkConnectedEventArgs()
        {
            NetworkChannel = null;
            UserData = null;
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

        public object UserData
        {
            get;
            private set;
        }

        public static NetworkConnectedEventArgs Create(GameFramework.Network.NetworkConnectedEventArgs e)
        {
            NetworkConnectedEventArgs networkConnectedEventArgs = ReferencePool.Acquire<NetworkConnectedEventArgs>();
            networkConnectedEventArgs.NetworkChannel = e.NetworkChannel;
            networkConnectedEventArgs.UserData = e.UserData;
            return networkConnectedEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
            UserData = null;
        }
    }
}
