using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;


public class BtComposite : BtNode
{
    protected List<BtNode> childs = new List<BtNode>();

    public BtComposite()
    {
        //noop
    }

    public override void Reset()
    {
        foreach( BtNode node in childs )
        {
            node.Reset();
        }
    }

    public void AddChild( BtNode child )
    {
        childs.Add( child );
        child.Index = ( childs.Count - 1 );
        child.Parent = this;
    }

    public List<BtNode> Childs
    {
        get { return childs; }
        set { childs = value; }
    }
}
