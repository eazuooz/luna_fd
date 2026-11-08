using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class BtRootNode : BtNode
{
    private BtNode child;

    public BtNode Child
    {
        set { child = value; }
        get { return child; }
    }

    public BtRootNode()
    {
        NodeType = eNodeType.Root;
        Parent = null;
    }

    public void AddChild( BtNode child )
    {
        this.child = child;
        this.child.Parent = this;
    }

    public override eStatus Tick()
    {
        child.Status = InitializeChildStatus();
        if (child.Status == eStatus.Invaild)
            return eStatus.Invaild;

        this.Status = child.Update();
        child.Status = this.Status;

        if (Status != eStatus.Running)
            Terminate();

        return Status;
    }

    private eStatus InitializeChildStatus()
    {
        if (child == null)
        {
            return eStatus.Invaild;
        }
        else if (child.Status == eStatus.Invaild)
        {
            child.Initialize();
            child.Status = eStatus.Running;
        }

        return child.Status;
    }

}
