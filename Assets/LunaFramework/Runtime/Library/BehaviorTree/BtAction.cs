using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    public class BtAction : BtNode
    {
        public BtAction()
        {
            NodeType = eNodeType.Action;
        }

        public override void Initialize()
        {
            
        }

        public override void Reset()
        {
            Status = eStatus.Invaild;
        }

        public override void Terminate()
        {

        }

        public override eStatus Tick()
        {
            if( Status == eStatus.Invaild )
            {
                Initialize();
                Status = eStatus.Running;
            }

            Status = Update();

            if( Status != eStatus.Running )
            {
                Terminate();
            }

            return Status;
        }
    }
}




