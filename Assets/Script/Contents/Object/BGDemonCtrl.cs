using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class BGDemonCtrl : MonoBehaviour
{

    private SkeletonAnimation skeletonAnimation;
    private float counter;

    // Start is called before the first frame update
    void Start()
    {
        skeletonAnimation = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);

        counter = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        counter += 1.0f * Time.deltaTime;

        if (counter < 4.0f)
        {
            skeletonAnimation.skeleton.ScaleX = 1.0f;
            transform.Translate(Vector3.right * 0.5f * Time.deltaTime);
        }
        else if(counter > 6.0f && counter < 10.0f)
        {
            skeletonAnimation.skeleton.ScaleX = -1.0f;
            transform.Translate(Vector3.left * 0.5f * Time.deltaTime);
        }
        else if(counter > 12.0f)
        {
            counter = 0;
        }
    }
}
