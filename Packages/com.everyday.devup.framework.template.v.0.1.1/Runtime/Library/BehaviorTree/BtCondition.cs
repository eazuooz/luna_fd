using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    /// <summary>
    /// Sequence ��忡�� ���� ó�� ���ϴ� ���
    /// </summary>
    public class BtCondition : BtNode
    {
        public BtCondition()
        {
            NodeType = eNodeType.Condition;
        }

        public override eStatus Tick()
        {
            //return base.Tick();
            Status = Update();

            if( Status == eStatus.Running )
            {
                Debug.Log("Behavior Tree Condition Error!!!\n");
            }

            if (Status == eStatus.Success)
            {
                TerminateRunningStatusByOtherAction();
            }

            return Status;
        }

        public void TerminateRunningStatusByOtherAction()
        {
            BtNode findRoot = FindRootNode(Parent);
            if (findRoot != null)
            {
                TerminateRunningAction(findRoot as BtRootNode);
            }
        }
        private void TerminateRunningAction(BtRootNode rootNode)
        {
            if (rootNode.Status == eStatus.Running)
            {
                BtNode runningAction
                    = FindRunningAction(rootNode.Child);

                TerminateAction(runningAction);
            }
        }
        private void TerminateAction(BtNode runningAction)
        {
            if (runningAction != null)
            {
                // ���� this Condition�� Running Action�� ���� �θ� ������
                // �ش� �θ� Sequence�� �ƴ϶�� Terminateȣ��
                if (Parent != runningAction.Parent ||
                    Parent.NodeType != eNodeType.Sequence)
                    runningAction.Terminate();
            }
        }
        private BtNode FindRootNode(BtNode parent)
        {
            BtNode findRoot = null;
            findRoot = parent;

            if(findRoot != null)
            {
                if(findRoot.Parent == null)
                {
                    return findRoot;
                }

                findRoot = FindRootNode(findRoot.Parent);
            }

            return findRoot;
        }

        public BtNode FindRunningAction( BtNode child )
        {
            BtNode runningActionNode = null;

            if( child != null )
            {
                if( child.NodeType == eNodeType.Selector ||
                    child.NodeType == eNodeType.Sequence)
                {
                    List<BtNode> childs = (child as BtComposite).Childs;
                    foreach( BtNode btNode in childs )
                    {
                        runningActionNode = FindRunningAction(btNode);

                        if (runningActionNode != null)
                            return runningActionNode;
                    }
                }

                if (child.NodeType == eNodeType.Action &&
                    child.Status == eStatus.Running)
                    return child;
            }

            return runningActionNode;
        }
    }
}

