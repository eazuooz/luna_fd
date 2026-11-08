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
    public sealed class NetworkClosedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(NetworkClosedEventArgs).GetHashCode();

        public NetworkClosedEventArgs()
        {
            NetworkChannel = null;
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

        public static NetworkClosedEventArgs Create(GameFramework.Network.NetworkClosedEventArgs e)
        {
            NetworkClosedEventArgs networkClosedEventArgs = ReferencePool.Acquire<NetworkClosedEventArgs>();
            networkClosedEventArgs.NetworkChannel = e.NetworkChannel;
            return networkClosedEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
        }
    }
}
