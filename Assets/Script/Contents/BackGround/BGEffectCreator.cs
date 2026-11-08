using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGEffectCreator : EverydayDevup.Framework.LunaEntityBehaviour
{
    public GameObject effect;
    public float delayTime = 0.0f;
    //private ParticleSystem ps;


    protected override void OnEntityInitialize()
    {
        StartCoroutine(LoopParticle());
    }

    // Update is called once per frame
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        
    }

    IEnumerator LoopParticle()
    {
        if (effect == null)
        {
            yield break;
        }

        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return null;
            
            EverydayDevup.Framework.LunaEntities.Spawn(effect, transform.position, transform.rotation, transform);

            yield return new WaitForSeconds(delayTime);
        }

    }
}

