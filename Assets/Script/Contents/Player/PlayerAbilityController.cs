using System;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Per-player gameplay abilities. The native player ticks and disposes this object; UI only submits requests.</summary>
    public sealed class PlayerAbilityController : IDisposable
    {
        private sealed class TimedSkill { public PlayerSkillSnapshot Skill; public float Remaining; }
        private sealed class ThunderHit { public int TargetId; public PlayerSkillSnapshot Skill; public float Remaining; }
        private sealed class AppliedGear
        {
            public int UnitId;
            public float AttackFactor = 1f, SpeedFactor = 1f, SkillFactor = 1f, ResistanceAdded;
            public float PreviousArmourPercent, PreviousAttackPercent;
            public bool ArmourChanged, AttackChanged;
        }
        private readonly PlayerLogic owner;
        private readonly Dictionary<string, float> cooldowns = new Dictionary<string, float>();
        private readonly Dictionary<string, PlayerSkillSnapshot> equippedSkills = new Dictionary<string, PlayerSkillSnapshot>();
        public PlayerLoadoutSnapshot Loadout { get; }
        private readonly List<PlayerSkillSnapshot> equipment = new List<PlayerSkillSnapshot>();
        private readonly List<TimedSkill> timers = new List<TimedSkill>();
        private readonly List<ThunderHit> thunderHits = new List<ThunderHit>();
        private readonly Dictionary<int, string> summons = new Dictionary<int, string>();
        private readonly HashSet<int> pendingSummons = new HashSet<int>();
        private readonly Dictionary<int, AppliedGear> gearRecipients = new Dictionary<int, AppliedGear>();
        private PlayerSkillSnapshot dragonSword;
        private bool disposed;
        public float IncomeMultiplier { get; private set; } = 1f;
        public float GoldBonus { get; private set; }
        private bool CanAct => !disposed && owner != null && owner.Data != null && owner.Available && !owner.IsHiding && owner.stat.Hp > 0f && !owner.unitState.IsDeath && !owner.unitState.IsLobbyMove;

        public PlayerAbilityController(PlayerLogic owner)
        {
            this.owner = owner;
            Loadout = owner.Data.PlayerLoadout ?? throw new InvalidOperationException("Player request has no captured loadout.");
            foreach (PlayerSkillSnapshot skill in Loadout.Skills)
            {
                cooldowns[skill.name] = Mathf.Max(0f, skill.coolTime);
                equippedSkills[skill.name] = skill;
            }
            equipment.AddRange(Loadout.Equipment);
            foreach (PlayerSkillSnapshot skill in equipment)
            {
                switch (skill.name)
                {
                    case "LunaEquipmentRichSword": IncomeMultiplier *= 1.1f; break;
                    case "LunaEquipmentRichShield": case "LunaEquipmentRichRing": IncomeMultiplier *= 1.15f; break;
                    case "LunaEquipmentDragonSword": dragonSword = skill; break;
                    case "LunaEquipmentWoodRing": case "LunaEquipmentSkullShield": case "LunaEquipmentSkullRing":
                    case "LunaEquipmentDragonRing": case "LunaEquipmentLeneSword": case "LunaEquipmentSlimeSword":
                        timers.Add(new TimedSkill { Skill = skill, Remaining = Mathf.Max(0.1f, skill.coolTime) }); break;
                    case "LunaEquipmentSlimeRing":
                        timers.Add(new TimedSkill { Skill = skill, Remaining = 0f }); break;
                }
            }
            ApplyEquipmentToNewUnits();
        }

        private PlayerSkillSnapshot Equipped(string key) => key != null && equippedSkills.TryGetValue(key, out PlayerSkillSnapshot skill) ? skill : null;
        public float Remaining(SkillData data) => Remaining(data != null ? Equipped(data.name) : null);
        public bool Ready(SkillData data) => Ready(data != null ? Equipped(data.name) : null);
        public bool TryUse(SkillData data, Vector3? targetPosition = null) => TryUse(data != null ? Equipped(data.name) : null, targetPosition);
        public float Remaining(PlayerSkillSnapshot data) => data != null && cooldowns.TryGetValue(data.name, out float value) ? Mathf.Max(0f, value) : 0f;
        public bool Ready(PlayerSkillSnapshot data)
        {
            PlayerSkillSnapshot captured = data != null ? Equipped(data.name) : null;
            return CanAct && captured != null && captured.ableSkill && Remaining(captured) <= 0f;
        }

        public bool TryUse(PlayerSkillSnapshot data, Vector3? targetPosition = null)
        {
            data = data != null ? Equipped(data.name) : null;
            if (!Ready(data) || (data.name == "Thunder" && !targetPosition.HasValue)) return false;
            var round = LunaScenes.CurrentGame;
            if (round == null || !round.IsActive || round.isStageStop || round.RoundId != owner.Data.RoundId ||
                round.PlayerEntityId != owner.ID || round.Roster == null || data.mana < 0 || round.Roster.Mana < data.mana) return false;
            if (targetPosition.HasValue && (!float.IsFinite(targetPosition.Value.x) || !float.IsFinite(targetPosition.Value.y) || !float.IsFinite(targetPosition.Value.z))) return false;
            bool used = Execute(data, targetPosition);
            if (used)
            {
                // Execute and this deduction are one synchronous request. Target selection and UI lifetime never own payment.
                round.Roster.Mana -= data.mana;
                cooldowns[data.name] = Mathf.Max(0f, data.coolTime);
            }
            return used;
        }

        public void Tick(float elapsed)
        {
            if (!CanAct) return;
            foreach (string key in cooldowns.Keys.ToArray()) cooldowns[key] = Mathf.Max(0f, cooldowns[key] - elapsed);
            ApplyEquipmentToNewUnits();
            for (int i = thunderHits.Count - 1; i >= 0; i--)
            {
                ThunderHit hit = thunderHits[i]; hit.Remaining -= elapsed;
                if (hit.Remaining > 0f) continue;
                thunderHits.RemoveAt(i);
                UnitLogic target = UnitLogic.Find(hit.TargetId);
                if (!Living(target) || target.UnitSide == owner.UnitSide) continue;
                Effect(hit.Skill.effectPrefab, target);
                target.Attack(owner.gameObject, SkillDamage(hit.Skill), false, true);
                target.TakeStun(hit.Skill.stunTime);
            }
            foreach (TimedSkill timer in timers.ToArray())
            {
                timer.Remaining -= elapsed;
                if (timer.Remaining > 0f) continue;
                ExecutePeriodic(timer.Skill);
                if (timer.Skill.name == "LunaEquipmentSlimeRing") timers.Remove(timer);
                else timer.Remaining = Mathf.Max(0.1f, timer.Skill.coolTime);
            }
            foreach (int id in summons.Keys.ToArray()) if (!pendingSummons.Contains(id) && LunaSpawn.Find<UnitLogic>(id) == null) summons.Remove(id);
            foreach (int id in gearRecipients.Keys.ToArray()) if (UnitLogic.Find(id) == null) gearRecipients.Remove(id);
        }

        private float SkillDamage(PlayerSkillSnapshot data) => Mathf.Max(0f, data.damage * owner.coefficient.SkillPoint);
        private static bool Living(UnitLogic unit) => unit != null && unit.Data != null && unit.Available && !unit.IsHiding && unit.stat.Hp > 0f && !unit.unitState.IsDeath;
        private UnitLogic[] Allies(bool towers = false) => UnitLogic.ActiveUnits.Where(unit => Living(unit) && unit.Data.RoundId == owner.Data.RoundId &&
            (unit == owner || LunaCombat.IsInCombat(unit)) && unit.UnitSide == owner.UnitSide && (towers || unit.hitPriority != eHitPriority.TOWER)).ToArray();
        private UnitLogic[] Enemies(bool towers = false) => UnitLogic.ActiveUnits.Where(unit => LunaCombat.IsInCombat(unit) && unit.Data.RoundId == owner.Data.RoundId &&
            unit.UnitSide != owner.UnitSide && (towers || unit.hitPriority != eHitPriority.TOWER)).ToArray();
        private void Effect(GameObject prefab, UnitLogic target)
        {
            if (prefab != null) LunaSpawn.Show<EffectLogic>(prefab, EffectData.Create(target.transform.position, target.ID, true, owner.Data.RoundId));
        }

        private bool Execute(PlayerSkillSnapshot data, Vector3? targetPosition)
        {
            switch (data.name)
            {
                case "FireBall": FireBall(data); return true;
                case "Blizzard":
                    Effect(data.effectPrefab, owner);
                    foreach (UnitLogic target in Enemies().Where(unit => unit.transform.position.x >= owner.transform.position.x && Vector3.Distance(unit.transform.position, owner.transform.position) <= data.range))
                    { target.Attack(owner.gameObject, SkillDamage(data), false, true); target.TakeStun(data.stunTime > 0f ? data.stunTime : 2f); }
                    return true;
                case "Healing":
                    foreach (UnitLogic target in Allies().Where(unit => Vector3.Distance(unit.transform.position, owner.transform.position) <= data.range))
                    { Effect(data.effectPrefab, target); target.Heal(data.damage); }
                    return true;
                case "Thunder":
                    int index = 0;
                    foreach (UnitLogic target in Enemies(true).Where(unit => Vector3.Distance(unit.transform.position, targetPosition.Value) <= data.range))
                        thunderHits.Add(new ThunderHit { TargetId = target.ID, Skill = data, Remaining = 0.45f * index++ });
                    return true;
                case "GoldyBlessing": GoldBonus = data.GoldBonus; return true;
                case "LoneBleesing": foreach (UnitLogic target in Allies(true)) target.TakeLoneBlessingMode(Duration(data, 10f), data.ArmourBonus); return true;
                case "WaterDropBarrier": owner.TakePochiBarrier_ForPlayer(Duration(data, 5f), data.DamageReduction); return true;
                case "Dwarf": return Summon(data, 1);
                case "AngerArmy": foreach (UnitLogic target in Allies().Where(unit => unit.hitPriority == eHitPriority.NORMAL)) target.TakeBerserkMode(Duration(data, 3f), data.BerserkStatBonus); return true;
                case "DemiGod": return Summon(data, 2, Duration(data, 5f), true);
                case "SerenBlessing": foreach (UnitLogic target in Allies(true)) target.TakeSerenBlessingMode(Duration(data, 10f), data.AvoidanceBonus); return true;
                case "BeleifWave": foreach (UnitLogic target in Allies()) target.TakeBeleifWaveMode(Duration(data, 5f), data.AttackBonus); return true;
                case "KingSlime": case "SkeletonArcher": return Summon(data, 1, 0f, false, Mathf.Max(1, data.maxSummonCount));
                default: return false;
            }
        }

        private static float Duration(PlayerSkillSnapshot data, float fallback) => data.burfTime > 0f ? data.burfTime : fallback;

        public void FireBall(PlayerSkillSnapshot data)
        {
            if (!CanAct || data.effectPrefab == null) return;
            foreach (UnitLogic target in Enemies().Where(unit => Vector3.Distance(unit.transform.position, owner.transform.position) <= data.range))
            {
                Vector3 point = target.ProjecttileArrivalLocation != null ? target.ProjecttileArrivalLocation.transform.position : target.transform.position;
                point += Vector3.up * (2.5f + UnityEngine.Random.Range(0f, 2f));
                LunaSpawn.Show<ProjectileLogic>(data.effectPrefab, ProjectileData.Create(data.effectPrefab, owner.ID, target.ID, point, SkillDamage(data), true, true, 0f, owner.Data.RoundId));
            }
        }

        private bool Summon(PlayerSkillSnapshot skill, int count, float lifetime = 0f, bool clone = false, int cap = int.MaxValue)
        {
            if (skill.effectPrefab == null) return false;
            int existing = summons.Count(pair => pair.Value == skill.effectPrefab.name && (pendingSummons.Contains(pair.Key) || LunaSpawn.Find<UnitLogic>(pair.Key) != null));
            if (existing + count > cap) return false;
            for (int i = 0; i < count; i++)
            {
                UnitView view = skill.effectPrefab.GetComponent<UnitView>();
                var data = UnitData.CreateFrom(view, Defined.AllyTowerPosition + Vector3.right * (0.5f + i * 0.3f));
                data.OwnerId = owner.ID; data.RoundId = owner.Data.RoundId; data.Side = owner.UnitSide; data.Lifetime = lifetime;
                data.HealthMultiplier = skill.SummonHealthMultiplier;
                data.AttackMultiplier = skill.SummonAttackMultiplier;
                data.ArmourMultiplier = skill.SummonArmourMultiplier;
                if (skill.name == "Dwarf") data.TauntRadius = 2f;
                if (clone)
                {
                    // The clone receives ordinary party equipment on its own life.
                    // Remove this controller's owner gear factor from the copied
                    // attack so the same sword is not counted a second time.
                    float ownerGear = gearRecipients.TryGetValue(owner.ID, out AppliedGear applied) ? applied.AttackFactor : 1f;
                    data.BaseAttackDamageOverride = owner.stat.AttackDamage / Mathf.Max(0.0001f, ownerGear);
                    data.AttackMultiplier = 1.5f; data.ArmourMultiplier = 1.5f; data.BerserkSeconds = lifetime;
                }
                int id = data.Id;
                summons[id] = skill.effectPrefab.name;
                pendingSummons.Add(id);
                LunaSpawn.Show<UnitLogic>(skill.effectPrefab, data, shown =>
                {
                    pendingSummons.Remove(id);
                    if (disposed || owner.Data == null || owner.IsHiding) { shown.SafeHide(); return; }
                    if (clone && shown.Animation != null) shown.Animation.Skeleton.SetColor(new Color(0.3f, 0.3f, 0.3f, 1f));
                    GameObject visual = Resources.Load<GameObject>("Prefabs/06Effects/Hero/SummonEffectYellow");
                    Effect(visual, shown);
                }, error => { pendingSummons.Remove(id); summons.Remove(id); Debug.LogError("Player summon failed: " + error); });
            }
            return true;
        }

        private void ExecutePeriodic(PlayerSkillSnapshot data)
        {
            switch (data.name)
            {
                case "LunaEquipmentWoodRing": foreach (UnitLogic target in Allies()) target.Heal(target.HealthBarHpPoint * 0.2f); break;
                case "LunaEquipmentSkullShield": foreach (UnitLogic target in Enemies()) target.TakeBleeding(target.HealthBarHpPoint * 0.1f, Duration(data, 5f)); break;
                case "LunaEquipmentSkullRing": owner.TakeBerserkMode(Duration(data, 5f)); break;
                case "LunaEquipmentDragonRing": FireBall(data); break;
                case "LunaEquipmentLeneSword": owner.Heal(owner.HealthBarHpPoint * data.coefficient * 0.01f); break;
                case "LunaEquipmentSlimeSword": Summon(data, 1, 0f, false, Mathf.Max(1, data.maxSummonCount)); break;
                case "LunaEquipmentSlimeRing": Summon(data, 2); break;
            }
        }

        public void OnAttack()
        {
            if (!CanAct || dragonSword == null || !Living(owner.Target) || !Defined.CanProbability(dragonSword.probablity)) return;
            Effect(dragonSword.effectPrefab, owner.Target);
            owner.Target.Attack(owner.gameObject, owner.stat.AttackDamage * 2f * dragonSword.coefficient, false, true);
        }

        private void ApplyEquipmentToNewUnits()
        {
            foreach (UnitLogic unit in Allies())
            {
                if (gearRecipients.ContainsKey(unit.ID)) continue;
                var applied = new AppliedGear { UnitId = unit.ID, PreviousArmourPercent = unit.stat.ArmourPercentForBoss, PreviousAttackPercent = unit.stat.AttackPercentForBoss };
                foreach (PlayerSkillSnapshot skill in equipment)
                {
                    float multiplier = 1f + skill.coefficient * 0.01f;
                    switch (skill.name)
                    {
                        case "LunaEquipmentSkullSword": applied.AttackFactor *= multiplier; break;
                        case "LunaEquipmentDragonShield":
                            if (unit is DragonPetLogic && unit.Data.OwnerId == owner.ID) applied.AttackFactor *= multiplier;
                            break;
                        case "LunaEquipmentSlimeShield": applied.SpeedFactor *= multiplier; break;
                        case "LunaEquipmentLeneShield": unit.stat.ArmourPercentForBoss = skill.coefficient; applied.ArmourChanged = true; break;
                        case "LunaEquipmentLeneRing": unit.stat.AttackPercentForBoss = skill.coefficient; applied.AttackChanged = true; break;
                        case "LunaEquipmentWoodSword": if (unit == owner) applied.SkillFactor *= skill.multipleSkillDamageBuff; break;
                        case "LunaEquipmentWoodShield": if (unit == owner) applied.ResistanceAdded += Mathf.Max(0f, skill.multipleSkillResistanceBuff - 1f); break;
                    }
                }
                unit.SetEquipmentModifiers(owner.ID, applied.AttackFactor, applied.SpeedFactor);
                unit.coefficient.SkillPoint *= applied.SkillFactor;
                unit.stat.SkillResistance += applied.ResistanceAdded;
                unit.StatusAfterReady();
                gearRecipients.Add(unit.ID, applied);
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (int id in summons.Keys.ToArray()) LunaSpawn.Cancel(id);
            summons.Clear(); pendingSummons.Clear(); timers.Clear(); thunderHits.Clear();
            foreach (AppliedGear applied in gearRecipients.Values)
            {
                UnitLogic unit = UnitLogic.Find(applied.UnitId);
                if (unit == null || unit.Data == null) continue;
                unit.RemoveEquipmentModifiers(owner.ID);
                unit.coefficient.SkillPoint /= Mathf.Max(0.0001f, applied.SkillFactor);
                unit.stat.SkillResistance -= applied.ResistanceAdded;
                if (applied.ArmourChanged) unit.stat.ArmourPercentForBoss = applied.PreviousArmourPercent;
                if (applied.AttackChanged) unit.stat.AttackPercentForBoss = applied.PreviousAttackPercent;
                unit.StatusAfterReady();
            }
            gearRecipients.Clear(); dragonSword = null;
        }
    }
}
