using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class Deus_skill : MonoBehaviour 
{
    [SerializeField] private string skillMatter = "타겟 범위 공격";
    [HideInInspector] public Status status;
    public GameObject effect;
    private GameObject tmpEffect;
    public float skillDamage = 200.0f;
    public float splashRange = 4.0f;
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
        attackPos = transform.position - new Vector3(status.stat.AttackRange, 0, 0);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Unit");

        for (int i = 0; i < 3; i++)
        {
            //Game.Instance.lunaSceneManager.CurrentStage.GetComponent<StageControl>().ShakeCameraOn();

            foreach (GameObject enemy in enemies)
            {
                if (enemy != null && enemy.GetComponent<Status>().UnitSide == eUnitSide.Allied)
                {
                    float distance = enemy.GetComponent<Transform>().position.x - attackPos.x;

                    if (enemy != null && distance >= -splashRange && splashRange >= distance)
                    {
                        enemy.GetComponent<Status>().Attack(gameObject, skillDamage, true);
                    }
                }
            }
            yield return new WaitForSeconds(0.25f);
        }

    }
}
