using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using EverydayDevup.Framework;
public class LobbyDoor : MonoBehaviour
{
    //public AudioClip chainSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoorOpenAction()
    {
        StartCoroutine(OpenDoorCorutine());
        gameObject.GetComponent<AudioSource>().Play();

    }

    IEnumerator OpenDoorCorutine()
    {
        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return null;
            bool isEnd = OpenDoor();

            if (isEnd)
                break;
        }

        yield return null;
    }

    public bool OpenDoor()
    {
        if(gameObject.transform.localPosition.y < 2.5f)
        {
            Vector3 dir = new Vector3(0.0f, 0.02f, 0.0f);
            gameObject.transform.Translate(dir);
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool CloseDoor()
    {
        if (gameObject.transform.position.y > 0.15f)
        {
            Vector3 dir = new Vector3(0.0f, -0.1f, 0.0f);
            gameObject.transform.Translate(dir);
            return false;
        }
        else
        {
            return true;
        }
    }
}
