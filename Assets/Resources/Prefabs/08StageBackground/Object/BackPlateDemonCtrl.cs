using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackPlateDemonCtrl : MonoBehaviour
{
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -13.6f)
            transform.position = new Vector3(0, startPos.y, startPos.z);

        transform.Translate(Vector3.left * 0.1f * Time.deltaTime);
    }
}
