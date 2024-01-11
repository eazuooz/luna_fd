using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class LunaCtrl_Lene : MonoBehaviour
{
    private Status status;

    // Start is called before the first frame update
    void Start()
    {
        status = GetComponent<Status>();

        status.stat.ReflectPercentForBoss = 50.0f;
        status.stat.ReflectPowerForBoss = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
