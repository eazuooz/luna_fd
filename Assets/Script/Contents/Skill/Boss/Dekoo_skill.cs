using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Dekoo_skill : MonoBehaviour 
{
    [SerializeField] private string skillMatter = "타겟 주위 범위 공격";
    [HideInInspector] public Status status;
    public GameObject effect;
    private GameObject tmpEffect;
    public float skillDamage = 500.0f;
    public float splashRange = 2.0f;
    private Vector3 attackPos;

    void Start()
    {
        status = GetComponent<Status>();
        status.unitState.IsTargetingSkill = true;
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
        if (status.AttackTarget != null)
        {
            attackPos = status.AttackTarget.transform.position;

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Unit");

            foreach (GameObject enemy in enemies)
            {
                if (enemy != null && enemy.GetComponent<Status>().UnitSide == eUnitSide.Enemy )
                {
                    float distance = enemy.GetComponent<Transform>().position.x - attackPos.x;

                    if (enemy != null && distance >= -splashRange && splashRange >= distance)
                    {
                        enemy.GetComponent<Status>().Attack(gameObject, skillDamage);
                    //    GameObject.Instantiate(effect, enemy.transform.position + effect.transform.position, effect.transform.rotation);
                    }
                }
            }
        }

        yield return null;
    }
}
