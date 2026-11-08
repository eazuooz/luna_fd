//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Network;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Network")]
    public sealed class NetworkComponent : GameFrameworkComponent
    {
        private INetworkManager mNetworkManager = null;
        private EventComponent mEventComponent = null;

        public int NetworkChannelCount
        {
            get
            {
                return mNetworkManager.NetworkChannelCount;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mNetworkManager = GameFrameworkEntry.GetModule<INetworkManager>();
            if (mNetworkManager == null)
            {
                Log.Fatal("Network manager is invalid.");
                return;
            }

            mNetworkManager.NetworkConnected += OnNetworkConnected;
            mNetworkManager.NetworkClosed += OnNetworkClosed;
            mNetworkManager.NetworkMissHeartBeat += OnNetworkMissHeartBeat;
            mNetworkManager.NetworkError += OnNetworkError;
            mNetworkManager.NetworkCustomError += OnNetworkCustomError;
        }

        private void Start()
        {
            mEventComponent = GameEntry.GetComponent<EventComponent>();
            if (mEventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }
        }

        public bool HasNetworkChannel(string name)
        {
            return mNetworkManager.HasNetworkChannel(name);
        }

        public INetworkChannel GetNetworkChannel(string name)
        {
            return mNetworkManager.GetNetworkChannel(name);
        }

        public INetworkChannel[] GetAllNetworkChannels()
        {
            return mNetworkManager.GetAllNetworkChannels();
        }

        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            mNetworkManager.GetAllNetworkChannels(results);
        }

        public INetworkChannel CreateNetworkChannel(string name, ServiceType serviceType, INetworkChannelHelper networkChannelHelper)
        {
            return mNetworkManager.CreateNetworkChannel(name, serviceType, networkChannelHelper);
        }

        public bool DestroyNetworkChannel(string name)
        {
            return mNetworkManager.DestroyNetworkChannel(name);
        }

        private void OnNetworkConnected(object sender, GameFramework.Network.NetworkConnectedEventArgs e)
        {
            mEventComponent.Fire(this, NetworkConnectedEventArgs.Create(e));
        }

        private void OnNetworkClosed(object sender, GameFramework.Network.NetworkClosedEventArgs e)
        {
            mEventComponent.Fire(this, NetworkClosedEventArgs.Create(e));
        }

        private void OnNetworkMissHeartBeat(object sender, GameFramework.Network.NetworkMissHeartBeatEventArgs e)
        {
            mEventComponent.Fire(this, NetworkMissHeartBeatEventArgs.Create(e));
        }

        private void OnNetworkError(object sender, GameFramework.Network.NetworkErrorEventArgs e)
        {
            mEventComponent.Fire(this, NetworkErrorEventArgs.Create(e));
        }

        private void OnNetworkCustomError(object sender, GameFramework.Network.NetworkCustomErrorEventArgs e)
        {
            mEventComponent.Fire(this, NetworkCustomErrorEventArgs.Create(e));
        }
    }
}
