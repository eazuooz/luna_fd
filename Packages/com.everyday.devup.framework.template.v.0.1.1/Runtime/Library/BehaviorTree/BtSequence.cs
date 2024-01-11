using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EverydayDevup.Framework
{
    public class BtSequence : BtComposite
    {
        public BtSequence()
        {
            NodeType = eNodeType.Sequence;
        }

        public override eStatus Update()
        {
            List<BtNode> childs = Childs;
            foreach (BtNode btNode in childs)
            {
                eStatus status = eStatus.Invaild;
                status = btNode.Status;

                if( btNode.NodeType != eNodeType.Action
                    || btNode.Status != eStatus.Success )
                {
                    status = btNode.Tick();
                }

                if (status != eStatus.Success)
                {
                    return status;
                }
            }

            return eStatus.Success;
        }
    }
}
