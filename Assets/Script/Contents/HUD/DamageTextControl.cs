using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextControl : MonoBehaviour
{
    private float moveSpeed;
    private float duration;

    void Start()
    {
        moveSpeed = 1.5f;
        duration = 0.3f;
    }

    // Update is called once per frame
    void Update()
    {
        if (duration <= 0)
            Destroy(gameObject);

        transform.Translate(new Vector3(0,moveSpeed * Time.deltaTime,0));

        duration -= 1.0f * Time.deltaTime;
    }
}
