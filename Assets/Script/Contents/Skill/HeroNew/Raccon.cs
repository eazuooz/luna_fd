using System.Collections;
using UnityEngine;
using Spine.Unity;
using Luna.Gameplay;

public class Raccon : HeroUnitLogic
{
    public AudioClip skillSound => View.GetSound("skillSound");
    private bool airborneBonusApplied;
    private float skillMultiplier;
    private SkeletonAnimation ani;
    public bool IsSpinning => skill_In_Action;

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }
        if (!LunaCombat.IsInCombat(status)) return;
        bool bonus = status.ultState >= eUltState.STAR5 && IdentifyFunc.IsAlive(status.AttackTarget)
            && IdentifyFunc.IsAirborne(status.AttackTarget);
        if (bonus != airborneBonusApplied)
        {
            status.coefficient.AttackDamage += bonus ? 0.2f : -0.2f;
            airborneBonusApplied = bonus;
            status.StatusAfterReady();
        }
        base.OnUpdate(elapsed, realElapsed);
    }
    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;
        airborneBonusApplied = false;
        skillMultiplier = 1f;
        ani = transform.Find("Sprite").GetComponent<SkeletonAnimation>();
    }
    public override void STAR3() { STAR1(); status.coefficient.AttackDamage += 0.05f; status.StatusAfterReady(); }
    public override void STAR5() { STAR3(); }
    public override void ULT1() { STAR5(); skillMultiplier = 1.15f; }
    public override void ULT2() { ULT1(); status.coefficient.Stamina *= 0.9f; status.StatusAfterReady(); }
    public override void Skill()
    {
        if (skill_In_Action || !LunaCombat.IsInCombat(status)) return;
        skill_In_Action = skill_In_Move = true;
        StartCoroutine(DuringSkill());
    }
    public override void SkillMove()
    {
        float speed = Mathf.Abs(status.stat.MoveSpeed) * (IdentifyFunc.IsAlive(status.AttackTarget) ? 1f : 3f);
        float stop = Mathf.Max(Defined.AllyTowerPosition.x, Defined.EnemyTowerPosition.x - status.stat.AttackRange);
        Vector3 desired = transform.position + Vector3.right * speed * Time.deltaTime;
        desired.x = Mathf.Min(desired.x, stop);
        transform.position = LunaCombat.ClampPosition(status, desired);
    }
    private IEnumerator DuringSkill()
    {
        status.SetSkillSuperArmour(true);
        status.unitState.IsActionDelay = true;
        status.stat.AttackDelayTime = -100f;
        ani.AnimationState.SetAnimation(0, "skill_loop", true);
        for (int i = 0; i < 10; i++)
        {
            if (!LunaCombat.IsInCombat(status)) break;
            status.PlayOneSound(skillSound);
            IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).Attack(gameObject,
                status.stat.AttackDamage * skillMultiplier, Defined.CanProbability(status.stat.CriticalActuationProbability), true),
                IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower,
                unit => IdentifyFunc.IsInDistance(gameObject, unit, 2.3f));
            yield return new WaitForSeconds(0.25f);
        }
        EndSpin();
        if (ani != null && gameObject.activeInHierarchy) ani.AnimationState.SetAnimation(0, "idle", true);
    }
    private void EndSpin()
    {
        FinishSkillExecution();
        status.SetSkillSuperArmour(false);
        status.unitState.IsActionDelay = false;
        status.unitState.IsSkillActivaion = false;
        status.stat.AttackDelayTime = -status.stat.AttackSpeed;
        skill_In_Action = skill_In_Move = false;
    }
    public override void AfterDeath() { EndSpin(); }
}
