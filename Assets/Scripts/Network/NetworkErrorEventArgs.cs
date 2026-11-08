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
using System.Net.Sockets;

namespace UnityGameFramework.Runtime
{
    public sealed class NetworkErrorEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(NetworkErrorEventArgs).GetHashCode();

        public NetworkErrorEventArgs()
        {
            NetworkChannel = null;
            ErrorCode = NetworkErrorCode.Unknown;
            ErrorMessage = null;
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

        public NetworkErrorCode ErrorCode
        {
            get;
            private set;
        }

        public SocketError SocketErrorCode
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public static NetworkErrorEventArgs Create(GameFramework.Network.NetworkErrorEventArgs e)
        {
            NetworkErrorEventArgs networkErrorEventArgs = ReferencePool.Acquire<NetworkErrorEventArgs>();
            networkErrorEventArgs.NetworkChannel = e.NetworkChannel;
            networkErrorEventArgs.ErrorCode = e.ErrorCode;
            networkErrorEventArgs.SocketErrorCode = e.SocketErrorCode;
            networkErrorEventArgs.ErrorMessage = e.ErrorMessage;
            return networkErrorEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
            ErrorCode = NetworkErrorCode.Unknown;
            ErrorMessage = null;
        }
    }
}
