using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Luna.Gameplay;
using UnityEngine.SceneManagement;

public class Rugani_skill : EnemySkillLogic
{
    [SerializeField] private string skillMatter = "¸ÊÀüÃ¼ °ø°Ý";
    public string SkillDescription => skillMatter;
    public GameObject effect => View.GetPrefab("effect");
    public float skillDamage => View.GetFloat("skillDamage");

    protected override void ConfigureUnitForSpawn()
    {
        status.unitState.IsTargetingSkill = false;
    }

    

    protected override void ExecuteSkill() { StartCoroutine(InstantSkill()); }

    IEnumerator InstantSkill()
    {
        IdentifyFunc.PickingUnits(unit =>
        {
            var target = UnitLogic.From(unit);
            var data = ProjectileData.Create(effect, ID, target.ID,
                target.ProjecttileArrivalLocation.transform.position + Vector3.up * 3f, skillDamage, true, true, 0f, Data.RoundId);
            LunaSpawn.Show<ProjectileLogic>(effect, data);
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
        yield return null;
    }
}