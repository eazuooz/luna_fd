using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class Rugani_skill : MonoBehaviour
{
    [SerializeField] private string skillMatter = "¸ÊÀüÃ¼ °ø°Ý";
    [HideInInspector] public Status status;
    public GameObject effect;
    public float skillDamage;

    void Start()
    {
        status = GetComponent<Status>();
        status.unitState.IsTargetingSkill = false;
    }

    void Update()
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
            if (enemy != null && enemy.GetComponent<Status>().UnitSide == eUnitSide.Allied && enemy.GetComponent<Status>().hitPriority != eHitPriority.TOWER)
            {
                GameObject bullet 
                    = GameObject.Instantiate(effect, enemy.GetComponent<Status>().ProjecttileArrivalLocation.transform.position + new Vector3(0, 3.0f, 0), effect.transform.rotation);
                bullet.GetComponent<Projectile>().Target = enemy;
                bullet.GetComponent<Projectile>().ProjectilePower = skillDamage;
                bullet.GetComponent<Projectile>().criticalShot = true;
            }
        }

        yield return null;
    }
}