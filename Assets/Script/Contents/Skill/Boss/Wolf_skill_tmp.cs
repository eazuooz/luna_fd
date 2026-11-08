using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class Wolf_skill_tmp : EverydayDevup.Framework.LunaEntityBehaviour
{
    [SerializeField] private string skillMatter = "¸ÊÀüÃ¼ °ø°Ý";
    public string SkillDescription => skillMatter;
    [HideInInspector] public Status status;
    public GameObject effect;
    public float skillDamage;

    protected override void OnEntityInitialize()
    {
        status = GetComponent<Status>();
        status.unitState.IsTargetingSkill = false;
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        if (status.unitState.IsSkillActivaion == true)
        {
            status.unitState.IsSkillActivaion = false;
            StartCoroutine("InstantSkill");
        }
    }

    IEnumerator InstantSkill()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Unit");

        foreach (GameObject enemy in enemies)
        {
            if (enemy.GetComponent<Status>().UnitSide == eUnitSide.Allied && enemy.GetComponent<Status>().hitPriority != eHitPriority.TOWER)
            {
                GameObject bullet = EverydayDevup.Framework.LunaEntities.Spawn(effect, enemy.GetComponent<Status>().ProjecttileArrivalLocation.transform.position + new Vector3(0, 3.0f, 0), effect.transform.rotation);
                bullet.GetComponent<Projectile>().Target = enemy;
                bullet.GetComponent<Projectile>().ProjectilePower = skillDamage;
                bullet.GetComponent<Projectile>().criticalShot = true;
            }
        }

        yield return null;
    }
}