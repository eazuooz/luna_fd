using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    public enum eStatus
    {
        Invaild,
        Success,
        Failure,
        Running,
        Aborted,
    }

    public enum eNodeType
    {
        Root,
        Selector,
        Sequence,
        Paraller,
        Decorator,
        Condition,
        Action,
    }

    /// <summary>
    /// 행동트리 노드
    /// </summary>
    public class BtNode 
    {
        private eStatus status;
        private eNodeType nodeType;
        private BtNode parent;
        private int index;
        public bool IsTerminated()
        {
            return (status == eStatus.Success) | (status == eStatus.Failure);
        }
        public bool IsRunning()
        {
            return status == eStatus.Running;
        }

        virtual public void Reset()
        {
            status = eStatus.Invaild;
        }
        public virtual void Initialize()
        {
            //noop        
        }

        public virtual eStatus Update()
        {
            return eStatus.Success;
        }
        public virtual void Terminate()
        {
            //noop
        }
        public virtual eStatus Tick()
        {
            if (status == eStatus.Invaild)
            {
                Initialize();
                status = eStatus.Running;
            }

            status = Update();

            if (status != eStatus.Running)
            {
                Terminate();
            }
            return status;
        }

        public BtNode()
        {
            status = eStatus.Invaild;
        }

        public eStatus Status 
        { 
            get { return status; }
            set { status = value; } 
        }
        public eNodeType NodeType 
        { 
            get { return nodeType; } 
            set { nodeType = value; } 
        }
        public BtNode Parent 
        { 
            get { return parent; } 
            set { parent = value; } 
        }
        public int Index
        {
            get { return index; }
            set { index = value; }
        }


    }
}



