using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>A hero is the actual native unit entity, not a second component driven by an adapter.</summary>
    public class HeroUnitLogic : UnitLogic
    {
        protected UnitLogic status => this;
        protected bool skill_In_Action;
        protected bool skill_In_Move;
        public bool IsRankReady { get; private set; }
        public override bool HasAutomaticSkill => !skill_In_Action && stat.Stamina > 0f;
        private sealed class Summon { public int Id; public GameObject Prefab; public bool Shown; }
        private readonly List<Summon> summons = new List<Summon>();

        protected override void ConfigureUnitForSpawn()
        {
            base.ConfigureUnitForSpawn();
            IsRankReady = false;
            skill_In_Action = skill_In_Move = false;
            switch (ultState)
            {
                case eUltState.ULT2: ULT2(); break;
                case eUltState.ULT1: ULT1(); break;
                case eUltState.STAR5: STAR5(); break;
                case eUltState.STAR4: case eUltState.STAR3: STAR3(); break;
                default: STAR1(); break;
            }
            IsRankReady = true;
            HeroPartyEffects.Register(this);
        }
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (!IsRankReady || !LunaCombat.IsInCombat(this)) return;
            if (!skill_In_Action && unitState.IsSkillActivaion)
            {
                unitState.IsSkillActivaion = false;
                Skill();
            }
            if (skill_In_Move) SkillMove();
        }
        protected override void OnUnitHidden()
        {
            StopHeroEffects();
            base.OnUnitHidden();
        }
        protected override void OnUnitDying()
        {
            StopHeroEffects();
            base.OnUnitDying();
        }
        private void StopHeroEffects()
        {
            IsRankReady = false;
            try { AfterDeath(); }
            finally
            {
                HeroPartyEffects.Unregister(this);
                foreach (var summon in summons) LunaSpawn.Cancel(summon.Id);
                summons.Clear();
                skill_In_Action = skill_In_Move = false;
            }
        }
        protected int CountSummons(GameObject prefab)
        {
            summons.RemoveAll(item => item.Shown && LunaSpawn.Find<UnitLogic>(item.Id) == null);
            int count = 0;
            foreach (var summon in summons) if (summon.Prefab == prefab) count++;
            return count;
        }
        protected int SpawnSummon(GameObject prefab, Vector3 position, Action<UnitData> configure = null, GameObject effect = null)
        {
            if (prefab == null) throw new InvalidOperationException("Hero summon binding is missing: " + GetType().Name);
            var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), LunaCombat.ClampPosition(this, position));
            data.OwnerId = ID; data.RoundId = Data.RoundId; data.Side = UnitSide;
            configure?.Invoke(data);
            var summon = new Summon { Id = data.Id, Prefab = prefab };
            summons.Add(summon); // Also count pending asynchronous requests against the live cap.
            int ownerId = ID;
            return LunaSpawn.Show<UnitLogic>(prefab, data, actor =>
            {
                summon.Shown = true;
                if (ID != ownerId || Data == null || IsHiding) { actor.SafeHide(); return; }
                if (effect != null) LunaEffects.Show(effect, actor.transform.position, actor.ID, true);
            }, error => { summons.Remove(summon); Debug.LogError(error); });
        }
        public virtual void Skill() { }
        public virtual void SkillMove() { }
        public virtual void STAR1() { }
        public virtual void STAR3() { STAR1(); }
        public virtual void STAR5() { STAR3(); }
        public virtual void ULT1() { STAR5(); }
        public virtual void ULT2() { ULT1(); }
        public virtual void AfterDeath() { }
    }
}
