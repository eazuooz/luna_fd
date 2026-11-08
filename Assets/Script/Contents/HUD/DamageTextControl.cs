using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextControl : EverydayDevup.Framework.LunaEntityBehaviour
{
    private float moveSpeed;
    private float duration;

    protected override void OnEntityInitialize()
    {
        moveSpeed = 1.5f;
        duration = 0.3f;
    }

    // Update is called once per frame
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        if (duration <= 0)
            EverydayDevup.Framework.LunaEntities.Hide(gameObject);

        transform.Translate(new Vector3(0,moveSpeed * Time.deltaTime,0));

        duration -= 1.0f * Time.deltaTime;
    }
}
