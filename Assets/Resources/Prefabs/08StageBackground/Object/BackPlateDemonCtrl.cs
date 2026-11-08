using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackPlateDemonCtrl : EverydayDevup.Framework.LunaEntityBehaviour
{
    private Vector3 startPos;

    // Start is called before the first frame update
    protected override void OnEntityInitialize()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        if (transform.position.x < -13.6f)
            transform.position = new Vector3(0, startPos.y, startPos.z);

        transform.Translate(Vector3.left * 0.1f * Time.deltaTime);
    }
}
