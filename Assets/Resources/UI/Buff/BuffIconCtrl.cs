using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffIconCtrl : MonoBehaviour
{


    public GameObject CharacterObject { get; set; }
    public int Index { get; set; }
    public float positionCoef { get; set; }
    public float positionHeight { get; set; }

    IEnumerator BlinkFlag;


    void Update()
    {
        if(CharacterObject != null)
        {
            gameObject.transform.position 
                = CharacterObject.transform.position 
                + new Vector3((Index - positionCoef) * 0.28f, positionHeight 
                + 1.0f * 0.28f, 0);
        }
    }

    public void SetFinePosition(int idx, float coef, float height)
    {
        Index = idx;
        positionCoef = coef;
        positionHeight = height;
    }

    public void Blink()
    {
        BlinkFlag = BlinkDuring();
        StartCoroutine(BlinkFlag);
    }

    public void UnBlink()
    {
        Color color = transform.Find("Sprite").GetComponent<SpriteRenderer>().color;
        color.a = 1.0f;
        transform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;

        if(BlinkFlag != null)
            StopCoroutine(BlinkFlag);
    }

    IEnumerator BlinkDuring()
    {
        Color color = transform.Find("Sprite").GetComponent<SpriteRenderer>().color;
        color.a = Random.Range(0.1f, 0.9f);
        transform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;

        while (true)
        {
            //InfiniteLoopDetector.Run();
            while (color.a > 0.1f)
            {
                //InfiniteLoopDetector.Run();
                color.a -= 2.0f * Time.deltaTime;
                transform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;

                yield return null;
            }

            while (color.a < 0.9f)
            {
                //InfiniteLoopDetector.Run();
                color.a += 2.0f * Time.deltaTime;
                transform.Find("Sprite").GetComponent<SpriteRenderer>().color = color;

                yield return null;
            }
        }
    }
}
