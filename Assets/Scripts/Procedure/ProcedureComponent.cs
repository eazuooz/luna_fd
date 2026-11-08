//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using System.Collections;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Procedure")]
    public sealed class ProcedureComponent : GameFrameworkComponent
    {
        private IProcedureManager mProcedureManager = null;
        private ProcedureBase mEntranceProcedure = null;

        [SerializeField]
        private string[] mAvailableProcedureTypeNames = null;

        [SerializeField]
        private string mEntranceProcedureTypeName = null;

        public ProcedureBase CurrentProcedure
        {
            get
            {
                return mProcedureManager.CurrentProcedure;
            }
        }

        public float CurrentProcedureTime
        {
            get
            {
                return mProcedureManager.CurrentProcedureTime;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            mProcedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            if (mProcedureManager == null)
            {
                Log.Fatal("Procedure manager is invalid.");
                return;
            }
        }

        private IEnumerator Start()
        {
            ProcedureBase[] procedures = new ProcedureBase[mAvailableProcedureTypeNames.Length];
            for (int i = 0; i < mAvailableProcedureTypeNames.Length; i++)
            {
                Type procedureType = Utility.Assembly.GetType(mAvailableProcedureTypeNames[i]);
                if (procedureType == null)
                {
                    Log.Error("Can not find procedure type '{0}'.", mAvailableProcedureTypeNames[i]);
                    yield break;
                }

                procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
                if (procedures[i] == null)
                {
                    Log.Error("Can not create procedure instance '{0}'.", mAvailableProcedureTypeNames[i]);
                    yield break;
                }

                if (mEntranceProcedureTypeName == mAvailableProcedureTypeNames[i])
                {
                    mEntranceProcedure = procedures[i];
                }
            }

            if (mEntranceProcedure == null)
            {
                Log.Error("Entrance procedure is invalid.");
                yield break;
            }

            mProcedureManager.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(), procedures);

            yield return new WaitForEndOfFrame();

            mProcedureManager.StartProcedure(mEntranceProcedure.GetType());
        }

        public bool HasProcedure<T>() where T : ProcedureBase
        {
            return mProcedureManager.HasProcedure<T>();
        }

        public bool HasProcedure(Type procedureType)
        {
            return mProcedureManager.HasProcedure(procedureType);
        }

        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            return mProcedureManager.GetProcedure<T>();
        }

        public ProcedureBase GetProcedure(Type procedureType)
        {
            return mProcedureManager.GetProcedure(procedureType);
        }
    }
}
