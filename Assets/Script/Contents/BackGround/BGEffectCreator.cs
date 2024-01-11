using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGEffectCreator : MonoBehaviour
{
    public GameObject effect;
    public float delayTime = 0.0f;
    //private ParticleSystem ps;


    void Start()
    {
        StartCoroutine(LoopParticle());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator LoopParticle()
    {
        if (effect == null)
        {
            yield return null;
        }

        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return null;
            
            Instantiate(effect, gameObject.transform);

            yield return new WaitForSeconds(delayTime);
        }

    }
}

