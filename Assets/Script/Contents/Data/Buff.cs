using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff 
{
    

    private eBuffType buffType;
    private float buffLeftTime;
    private bool isBlinking = false;
    private GameObject iconObject = null;

    public Buff(eBuffType info, float time)
    {
        buffType = info;
        buffLeftTime = time;
    }

    public eBuffType BuffType
    { 
        get { return buffType; }
        set { buffType = value; }    
    }
    public float BuffLeftTime
    {
        get { return buffLeftTime; }
        set { buffLeftTime = value; }
    }
    public bool IsBlinking
    {
        get { return isBlinking; }
        set { isBlinking = value; }
    }
    public GameObject IconObject
    {
        get { return iconObject; }
        set { iconObject = value; }
    }
}


