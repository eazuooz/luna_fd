using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class Projectile : EverydayDevup.Framework.LunaEntityBehaviour
{
    public GameObject Attacker;

    public enum eBulletType
    {
        MAGIC = 0,
        ARROW = 1,
        CREST = 2,
    }
    public eBulletType bulletType;
    public float projectileSpeed;
    public GameObject hitEffect;


    private float initProjecttileSpeed;
    private GameObject target;
    private Vector3 tmpTargetPos;
    private float projectilePower;

    private Vector3 dir;
    private float dis;
    private float disX;
    public bool criticalShot = false;
    public bool skillShot = false;

    private Vector3 tmpPosArrowDestination;

    public bool isStun;
    public float StunTime { get; set; }

    public GameObject Target
    {
        get { return target; }
        set { target = value; }
    }
    public float ProjectilePower
    {
        get { return projectilePower; }
        set { projectilePower = value; }
    }

    protected override void OnEntityShow()
    {
        Attacker = null;
        target = null;
        projectilePower = 0f;
        criticalShot = false;
        skillShot = false;
        isStun = false;
        StunTime = 0f;
        projectileSpeed = initProjecttileSpeed;
    }

    protected override void OnEntityHide()
    {
        projectileSpeed = initProjecttileSpeed;
    }

    protected override void OnEntityAwake()
    {
        initProjecttileSpeed = projectileSpeed;
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        if (target != null)
            tmpTargetPos = target.GetComponent<Status>().ProjecttileArrivalLocation.transform.position;

        dis = Vector3.Distance(tmpTargetPos, GetComponent<Transform>().position);

        if (dis < 0.2f || target == null)
        {
            ProjectileHit();
            return;
        }

        if (projectileSpeed > 1.0f)
        {
            projectileSpeed -= 2.0f * Time.deltaTime;
        }

        if (bulletType == eBulletType.MAGIC)
        {

            dir = (tmpTargetPos - GetComponent<Transform>().position).normalized;

            GetComponent<Transform>().Translate(dir * projectileSpeed * Time.deltaTime);

            if(GetComponent<Transform>().Find("Sprite") != null)
                GetComponent<Transform>().Find("Sprite").rotation = Quaternion.Euler(0, 0, 90 + Mathf.Atan2(dir.y, dir.x) / Mathf.PI * 180);

        }
        else if(bulletType == eBulletType.ARROW)
        {
            disX = tmpTargetPos.x - GetComponent<Transform>().position.x;

            if (disX < 0)
                disX *= -1;

            tmpPosArrowDestination = new Vector3(tmpTargetPos.x, tmpTargetPos.y + disX, tmpTargetPos.z);

            dir = (tmpPosArrowDestination - GetComponent<Transform>().position).normalized;

            GetComponent<Transform>().position = GetComponent<Transform>().position + (dir * projectileSpeed * Time.deltaTime);

            GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) / Mathf.PI * 180);
        }
    }

    private void ProjectileHit()
    {
        if (hitEffect != null)
        {
            GameObject Effect = EverydayDevup.Framework.LunaEntities.Spawn(hitEffect, GetComponent<Transform>().position, Quaternion.Euler(0, 0, 0));
        }

        if (target != null && target.GetComponent<Status>().stat.Hp > 0)
        {
            if(isStun == true)
            {
                target.GetComponent<Status>().TakeStun(StunTime);
            }

            target.GetComponent<Status>().Attack(Attacker, projectilePower, criticalShot, skillShot);
        }

        PoolManager.ReleaseObject(this.gameObject);
    }

    public void SetProjectileState(GameObject attacker, GameObject target, float power, bool addStun = true, float stunTime = 0.0f)
    {
        Attacker = attacker;
        Target = target;
        projectilePower = power;

        isStun = addStun;
        StunTime = addStun ? stunTime : 0f;
    }

    public void SetProjectileAddStun(float stunTime)
    {
        isStun = true;
        StunTime = stunTime;
    }
}
