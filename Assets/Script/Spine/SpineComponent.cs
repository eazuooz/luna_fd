using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;


public class SpineComponent
{
    private SpineRenderer mRenerer = null;

    public void Init()
    {
        mRenerer = new SpineRenderer();

        //mRenerer.eventMain += new SpineRenderer.DelegateEvent(OnEvent);
    }
}
