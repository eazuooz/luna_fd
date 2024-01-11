using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class LunaDragonCtrl : MonoBehaviour
{
    private SkeletonAnimation ani;

    void Start()
    {
        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
