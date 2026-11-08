//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Event;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Event")]
    public sealed class EventComponent : GameFrameworkComponent
    {
        private IEventManager mEventManager = null;

        public int EventHandlerCount
        {
            get
            {
                return mEventManager.EventHandlerCount;
            }
        }

        public int EventCount
        {
            get
            {
                return mEventManager.EventCount;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mEventManager = GameFrameworkEntry.GetModule<IEventManager>();
            if (mEventManager == null)
            {
                Log.Fatal("Event manager is invalid.");
                return;
            }
        }

        private void Start()
        {
        }

        public int Count(int id)
        {
            return mEventManager.Count(id);
        }

        public bool Check(int id, EventHandler<GameEventArgs> handler)
        {
            return mEventManager.Check(id, handler);
        }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            mEventManager.Subscribe(id, handler);
        }

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            mEventManager.Unsubscribe(id, handler);
        }

        public void SetDefaultHandler(EventHandler<GameEventArgs> handler)
        {
            mEventManager.SetDefaultHandler(handler);
        }

        public void Fire(object sender, GameEventArgs e)
        {
            mEventManager.Fire(sender, e);
        }

        public void FireNow(object sender, GameEventArgs e)
        {
            mEventManager.FireNow(sender, e);
        }
    }
}
