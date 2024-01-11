using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton_magicianLeader_skill : MonoBehaviour 
{
    [SerializeField] private string skillMatter = "¸Ê ÀüÃ¼ °ø°Ý";
    [HideInInspector] public Status status;
    public GameObject effect;
    public float skillDamage = 20.0f;

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
        IdentifyFunc.PickingUnits((unit) =>
        {
            Defined.EffectCreate(effect, unit);

            unit.GetComponent<Status>().Attack(gameObject, skillDamage, true);
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        yield return null;
    }
}
