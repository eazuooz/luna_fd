using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class TurretCtrl : MonoBehaviour
{
    public enum eTurret
    {
        NONE,
        ARROW,
        FIRE,
        ICE,
    }

    public eTurret TowerTurret;
    public GameObject TurretArrow;
    public GameObject TurretFire;
    public GameObject TurretIce;

    public float ArrowDistance = 6.0f;
    public float FireDistance = 7.0f;
    public float IceDistance = 7.0f;

    public float ArrowTerm = 1.5f;
    public float FireTerm = 2.5f;
    public float IceTerm = 3.5f;

    public float ArrowPower = 100.0f;
    public float FirePower = 300.0f;
    public float IcePower = 300.0f;

    public AudioClip ArrowSound;
    public AudioClip FireSound;
    public AudioClip IceSound;

    private Vector3 TurretPos;
    private GameObject target;

    private GameObject TurretBullet;
    private float TurretAttackRange;
    private float TurretAttackPoint;
    private float TurretAttackTerm;
    private AudioClip TurretSound;

    private Status thisStatus;

    void Start()
    {
        thisStatus = GetComponent<Status>();

        if (transform.Find("AttackPos") != null)
        {
            TurretPos = transform.Find("AttackPos").transform.position;
        }

        InitTerret();

        if (TowerTurret != eTurret.NONE)
        {
            StartCoroutine(ActivateTurret());
        }
    }

    private void InitTerret()
    {
        target = null;

        int idx = Random.Range(0, 3);

        if (idx == 0)
        {
            TowerTurret = eTurret.ARROW;

            TurretBullet = TurretArrow;
            TurretAttackRange = ArrowDistance;
            TurretAttackPoint = ArrowPower;
            TurretAttackTerm = ArrowTerm;
            TurretSound = ArrowSound;
        }
        else if (idx == 1)
        {
            TowerTurret = eTurret.FIRE;

            TurretBullet = TurretFire;
            TurretAttackRange = FireDistance;
            TurretAttackPoint = FirePower;
            TurretAttackTerm = FireTerm;
            TurretSound = FireSound;
        }
        else if (idx == 2)
        {
            TowerTurret = eTurret.ICE;

            TurretBullet = TurretIce;
            TurretAttackRange = IceDistance;
            TurretAttackPoint = IcePower;
            TurretAttackTerm = IceTerm;
            TurretSound = IceSound;
        }
    }

    private void SetTarget()
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(gameObject, unit, TurretAttackRange) &&
                unit.GetComponent<Status>().stat.Hp > 0.0f)
            {
                target = unit;
                return;
            }
        }, IdentifyFunc.IsEnemy);
    }

    private void ShotMissile()
    {
        GameObject bullet = PoolManager.Instance.spawnObject(TurretBullet, TurretPos, TurretBullet.transform.rotation);
        bullet.GetComponent<Projectile>().SetProjectileState(gameObject, target, TurretAttackPoint);

        if (TowerTurret == eTurret.ICE)
        {
            bullet.GetComponent<Projectile>().SetProjectileAddStun(2.0f);
        }

        thisStatus.PlayOneSound(TurretSound);
    }

    IEnumerator ActivateTurret()
    {
        while (Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>().gameEnemyTower != null)
        {
            if (target == null ||
                target.GetComponent<Status>().stat.Hp <= 0.0f ||
                target.activeSelf == false)
            {
                SetTarget();

                yield return null;
                continue;
            }

            ShotMissile();

            yield return new WaitForSeconds(TurretAttackTerm);
        }
    }
}