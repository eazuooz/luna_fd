//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Fsm;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/FSM")]
    public sealed class FsmComponent : GameFrameworkComponent
    {
        private IFsmManager mFsmManager = null;

        public int Count
        {
            get
            {
                return mFsmManager.Count;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mFsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
            if (mFsmManager == null)
            {
                Log.Fatal("FSM manager is invalid.");
                return;
            }
        }

        private void Start()
        {
        }

        public bool HasFsm<T>() where T : class
        {
            return mFsmManager.HasFsm<T>();
        }

        public bool HasFsm(Type ownerType)
        {
            return mFsmManager.HasFsm(ownerType);
        }

        public bool HasFsm<T>(string name) where T : class
        {
            return mFsmManager.HasFsm<T>(name);
        }

        public bool HasFsm(Type ownerType, string name)
        {
            return mFsmManager.HasFsm(ownerType, name);
        }

        public IFsm<T> GetFsm<T>() where T : class
        {
            return mFsmManager.GetFsm<T>();
        }

        public FsmBase GetFsm(Type ownerType)
        {
            return mFsmManager.GetFsm(ownerType);
        }

        public IFsm<T> GetFsm<T>(string name) where T : class
        {
            return mFsmManager.GetFsm<T>(name);
        }

        public FsmBase GetFsm(Type ownerType, string name)
        {
            return mFsmManager.GetFsm(ownerType, name);
        }

        public FsmBase[] GetAllFsms()
        {
            return mFsmManager.GetAllFsms();
        }

        public void GetAllFsms(List<FsmBase> results)
        {
            mFsmManager.GetAllFsms(results);
        }

        public IFsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states) where T : class
        {
            return mFsmManager.CreateFsm(owner, states);
        }

        public IFsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class
        {
            return mFsmManager.CreateFsm(name, owner, states);
        }

        public IFsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states) where T : class
        {
            return mFsmManager.CreateFsm(owner, states);
        }

        public IFsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class
        {
            return mFsmManager.CreateFsm(name, owner, states);
        }

        public bool DestroyFsm<T>() where T : class
        {
            return mFsmManager.DestroyFsm<T>();
        }

        public bool DestroyFsm(Type ownerType)
        {
            return mFsmManager.DestroyFsm(ownerType);
        }

        public bool DestroyFsm<T>(string name) where T : class
        {
            return mFsmManager.DestroyFsm<T>(name);
        }

        public bool DestroyFsm(Type ownerType, string name)
        {
            return mFsmManager.DestroyFsm(ownerType, name);
        }

        public bool DestroyFsm<T>(IFsm<T> fsm) where T : class
        {
            return mFsmManager.DestroyFsm(fsm);
        }

        public bool DestroyFsm(FsmBase fsm)
        {
            return mFsmManager.DestroyFsm(fsm);
        }
    }
}
