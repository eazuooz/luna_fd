using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LunaItem : MonoBehaviour
{
    protected GameObject target;
    protected GameObject gainEffect;

    protected float floatingSpeed;
    protected bool isMagnetic;

    public void ItemInfoInitialized()
    {
        isMagnetic = false;
        target = null;

        IdentifyFunc.PickingUnits((unit) =>
        {
            target = unit;
        }, IdentifyFunc.IsPlayer);

        gainEffect 
            = Resources.Load<GameObject>("Prefabs/06Effects/GetCoinEffect");
    }

    private void AccelateMagneticSpeed(ref float targetSpeed, float accSpeed)
    {
        if (targetSpeed < 2.0f)
            targetSpeed += (accSpeed * Time.deltaTime);
    }
    public void ChasePlayer(System.Action getItemCallBack)
    {
        if (isMagnetic == true)
        {
            if (target != null)
            {
                AccelateMagneticSpeed(ref floatingSpeed, 0.1f);

                float distanceToPlayer
                        = target.transform.position.x - gameObject.transform.position.x;

                Vector3 coinDirection
                    = (target.GetComponent<Status>().ProjecttileArrivalLocation.transform.position - gameObject.transform.position).normalized;

                transform.Translate(coinDirection * floatingSpeed);

                if ((distanceToPlayer < 0.1f && distanceToPlayer > -0.1f) ||
                    floatingSpeed >= 0.3f)
                {
                    getItemCallBack();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
            return;
    }
}
