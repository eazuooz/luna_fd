using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    public class BtSelector : BtComposite
    {
        public BtSelector()
        {
            NodeType = eNodeType.Selector;
        }

        public override eStatus Update()
        {
            List<BtNode> childs = this.Childs;
            foreach (BtNode btNode in childs)
            {
                eStatus status = btNode.Tick();

                if (status != eStatus.Failure)
                {
                    ClearChild();
                    return status;
                }

            }

            return eStatus.Failure;
        }

        protected void ClearChild()
        {
            foreach (BtNode btNode in childs)
            {
                btNode.Reset();
            }
        }
    }

}

