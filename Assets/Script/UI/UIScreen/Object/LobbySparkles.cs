using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySparkles : MonoBehaviour
{
    Vector3 dir;
    // Start is called before the first frame update
    void Start()
    {
        dir = new Vector3(-0.2f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {

        

        if (gameObject.transform.localPosition.x < -1.5f)
        {
            dir.x = 0.2f;
        }
        
        if (gameObject.transform.localPosition.x > 6.0f)
        {
            dir.x = -0.2f;
        }

        gameObject.transform.Translate(dir);
    }


}
