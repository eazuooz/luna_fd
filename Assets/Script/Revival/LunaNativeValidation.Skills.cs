#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class LunaNativeValidation
{
    private static readonly string[] PlayerSkillNames = { "FireBall", "Blizzard", "Healing", "Thunder", "GoldyBlessing", "LoneBleesing", "WaterDropBarrier", "Dwarf", "AngerArmy", "DemiGod", "SerenBlessing", "BeleifWave", "KingSlime", "SkeletonArcher" };

    private IEnumerator PlayerSkillContracts()
    {
        yield return NativeSkillDataContracts();
        var playerData = LunaDataTable.Instance.playerData;
        var oldSkills = playerData.equippedSkill.ToList();
        var oldSkin = playerData.SkinType;
        float oldScale = Time.timeScale;
        try
        {
            Time.timeScale = 8f; playerData.SkinType = PlayerData.eLunaSkin.Basic;
            foreach (string name in PlayerSkillNames)
            {
                if (!Includes("skills", name)) continue;
                BeginCase("skills", name);
                playerData.equippedSkill = new[] { name }.Concat(new[] { "FireBall", "Healing", "Thunder" }.Where(key => key != name).Take(2)).ToList();
                yield return LoadStage("Stage01-01");
                // Each actual round intentionally starts at its own 1.5x speed.
                // Apply the diagnostic acceleration after that normal intro completes.
                Time.timeScale = 8f;
                var round = LunaScenes.CurrentGame;
                var player = (PlayerLogic)round.Player;
                var ability = player.Abilities;
                var skill = LunaDataTable.Instance.skiiTable[name];
                var hud = Active<UIPlayer>(); var ui = Active<UISkill>();
                Require(hud != null && ui != null && ability != null, "Player skill UI/owned controller was not initialized.");
                // Hold round wave/currency production, but allow the native actor clock to advance the real cooldown.
                round.isStageStop = true;
                foreach (var actor in UnitLogic.ActiveUnits.Where(actor => actor.Data.RoundId == round.RoundId).ToArray())
                {
                    if (actor.hitPriority == eHitPriority.TOWER) actor.Invincible(0f);
                    else if (actor.UnitSide == eUnitSide.Enemy) actor.SafeHide();
                    else { HoldAutomaticActions(actor); actor.ResetAnimation(); }
                }
                yield return Until(() => ability.Ready(skill), "Natural initial ability cooldown: " + name, 45f);
                var soldierPrefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
                UnitLogic ally = null, enemy = null;
                foreach (bool opposing in new[] { false, true })
                {
                    var data = UnitData.CreateFrom(soldierPrefab.GetComponent<UnitView>(), player.transform.position + Vector3.right * (opposing ? 1f : -0.7f));
                    data.RoundId = round.RoundId; data.Side = opposing ? eUnitSide.Enemy : eUnitSide.Allied;
                    data.InitialMode = UnitSpawnMode.Staged; data.HealthMultiplier = 10000f;
                    yield return ShowFixture<UnitLogic>(soldierPrefab, data, actor => { if (opposing) enemy = actor; else ally = actor; });
                }
                foreach (var actor in new[] { ally, enemy }) { HoldAutomaticActions(actor); actor.stat.AttackDamage = 0f; }
                enemy.stat.Armour = enemy.stat.SkillResistance = enemy.stat.AttackAvoidance = enemy.stat.SkillAvoidance = enemy.stat.DebuffAvoidance = 0f;
                ally.stat.Hp = ally.HealthBarHpPoint * 0.5f;
                player.Data.TargetId = enemy.ID;
                float beforeEnemy = enemy.stat.Hp, beforeAlly = ally.stat.Hp;
                float beforeArmour = ally.coefficient.Armour, beforeAttack = ally.coefficient.AttackDamage;
                float beforeAvoidance = ally.stat.SkillAvoidance;
                float beforePlayerHp = player.stat.Hp;
                int manaCost = skill.mana;
                var button = ui.transform.Find("Skill_Button_0").GetComponent<Button>();
                round.isStageStop = false;
                hud.Mana = Mathf.Max(0, manaCost - 1);
                int insufficient = hud.Mana;
                Require(!ability.TryUse(skill, name == "Thunder" ? enemy.transform.position : (Vector3?)null) && hud.Mana == insufficient && ability.Remaining(skill) <= 0f, "Insufficient-mana request altered state or bypassed the controller's business rule.");
                hud.Mana = 100;
                string unequipped = PlayerSkillNames.First(key => !playerData.equippedSkill.Contains(key));
                Require(!ability.TryUse(LunaDataTable.Instance.skiiTable[unequipped], enemy.transform.position) && hud.Mana == 100, "Unequipped ability bypassed the player-owned loadout.");
                float priorTargetingScale = Time.timeScale;
                button.onClick.Invoke();
                if (name == "Thunder")
                {
                    Require(hud.Mana == 100 && Mathf.Approximately(Time.timeScale, 0.5f), "Thunder paid before target acceptance or failed to enter targeting.");
                    Require(!ui.TrySubmitThunderTarget(new Vector3(float.NaN, 0f)), "Thunder accepted an invalid world target.");
                    Require(ui.TrySubmitThunderTarget(enemy.transform.position), "Thunder rejected a valid target selection.");
                    Require(!ui.TrySubmitThunderTarget(enemy.transform.position), "Thunder accepted the same target twice.");
                    Require(Mathf.Approximately(Time.timeScale, priorTargetingScale), "Thunder failed to restore the prior simulation speed: expected " + priorTargetingScale + ", actual " + Time.timeScale);
                }
                Require(hud.Mana == 100 - manaCost && ability.Remaining(skill) > 0f, "Normal UI activation did not pay exactly once/start cooldown.");
                button.onClick.Invoke();
                Require(hud.Mana == 100 - manaCost, "Repeated UI event spent mana during cooldown.");
                round.isStageStop = true;

                if (name == "WaterDropBarrier")
                {
                    player.stat.Armour = player.stat.AttackAvoidance = player.stat.SkillAvoidance = 0f;
                    player.Attack(enemy.gameObject, 100f);
                    Require(Mathf.Abs(beforePlayerHp - player.stat.Hp - 50f) < 0.1f, "Water barrier did not halve actual incoming damage.");
                }
                bool stun = false, berserk = false, auraAlive = false;
                int peakOwned = 0;
                var ownerIds = new HashSet<int>();
                float maximumArmour = beforeArmour, maximumAttack = beforeAttack, maximumAvoidance = beforeAvoidance;
                string serenAddress = null;
                if (name == "SerenBlessing")
                {
                    GameObject effect = player.GetLunaAssetBundle().serenBlessing;
                    serenAddress = Resources.Load<LunaEntityCatalog>("Luna/EntityCatalog").Entries.Single(entry => entry.Prefab == effect).Address;
                }
                float started = Time.time;
                while (Time.time - started < 2.8f)
                {
                    stun |= enemy.isStun; berserk |= ally.IsBerserk;
                    maximumArmour = Mathf.Max(maximumArmour, ally.coefficient.Armour);
                    maximumAttack = Mathf.Max(maximumAttack, ally.coefficient.AttackDamage);
                    maximumAvoidance = Mathf.Max(maximumAvoidance, ally.stat.SkillAvoidance);
                    var owned = UnitLogic.ActiveUnits.Where(actor => actor.Data.OwnerId == player.ID && actor.Data.RoundId == round.RoundId).ToArray();
                    peakOwned = Mathf.Max(peakOwned, owned.Length);
                    foreach (var actor in owned) ownerIds.Add(actor.ID);
                    if (serenAddress != null)
                        auraAlive |= Entities().OfType<EffectLogic>().Any(effect => effect.Entity.EntityAssetName == serenAddress && effect.SpawnData.OwnerId == ally.ID && effect.GetComponentsInChildren<ParticleSystem>().Any(particles => particles.IsAlive(true)));
                    yield return null;
                }
                float damage = beforeEnemy - enemy.stat.Hp, healing = ally.stat.Hp - beforeAlly;
                switch (name)
                {
                    case "FireBall": case "Thunder": Require(damage > 0f, "Native projectile/targeted skill produced no damage."); break;
                    case "Blizzard": Require(damage > 0f && stun, "Blizzard did not damage and stun its actual target."); break;
                    case "Healing": Require(Mathf.Abs(healing - skill.damage) <= 1f, "Healing did not apply its authored HP amount."); break;
                    case "LoneBleesing": Require(Mathf.Abs(maximumArmour - beforeArmour - 1f) < 0.001f, "Lone blessing armour was missing or double-applied."); break;
                    case "BeleifWave": Require(Mathf.Abs(maximumAttack - beforeAttack - 0.15f) < 0.001f, "Belief wave did not apply authored +15% attack coefficient."); break;
                    case "SerenBlessing": Require(Mathf.Abs(maximumAvoidance - beforeAvoidance - 25f) < 0.001f && auraAlive, "Seren resistance or its live nested-particle aura was missing."); break;
                    case "AngerArmy": Require(berserk, "Anger Army did not affect the live soldier."); break;
                    case "Dwarf": case "KingSlime": case "SkeletonArcher": Require(peakOwned == 1, "Single-summon ability did not create one owned native actor."); break;
                    case "DemiGod": Require(peakOwned == 2 && ownerIds.Count == 2, "DemiGod did not create exactly two independently owned clones."); break;
                    case "GoldyBlessing":
                        Require(ability.GoldBonus == 6f + Mathf.Clamp(skill.level / 5, 0, 4), "Gold blessing did not apply its reviewed level bonus.");
                        round.Roster.Gold = 0; round.isStageStop = false;
                        float incomeStart = Time.time;
                        yield return new WaitForSeconds(5f);
                        float elapsed = Time.time - incomeStart;
                        float rate = 2f * (1f + Defined.AddGoldSpeedValue * (playerData.GoldSpeedLevel + ability.GoldBonus));
                        Require(Mathf.Abs(round.Roster.Gold - rate * elapsed) <= 2f, "Round-owned gold income did not consume the player's blessing bonus.");
                        round.isStageStop = true;
                        current.observations.Add("Measured gold=" + round.Roster.Gold + " over " + elapsed + " game seconds; expected rate=" + rate);
                        break;
                }
                if (name == "SerenBlessing")
                {
                    yield return new WaitForSeconds(7.6f);
                    Require(Mathf.Abs(ally.stat.SkillAvoidance - beforeAvoidance) < 0.001f, "Seren's ten-second resistance did not expire.");
                    Require(!Entities().OfType<EffectLogic>().Any(effect => effect.Entity.EntityAssetName == serenAddress && effect.SpawnData.OwnerId == ally.ID), "Seren aura outlived its owner's ten-second status effect.");
                }
                if (name == "DemiGod")
                {
                    float lifetime = 5f + Mathf.FloorToInt(skill.level / 5f) * 5f;
                    yield return Until(() => ownerIds.All(id => LunaSpawn.Find<UnitLogic>(id) == null), "DemiGod natural lifetime", lifetime / 8f + 5f);
                }
                current.observations.Add("Paid mana=" + manaCost + "; target damage=" + damage + "; healing=" + healing + "; peak owned=" + peakOwned + "; aura alive=" + auraAlive);
                CheckOwnership();
                EndCase("Normal UI activation, controller-owned loadout/mana/cooldown rejection, exactly-once payment and observed native skill outcome.");
            }
        }
        finally { playerData.equippedSkill = oldSkills; playerData.SkinType = oldSkin; Time.timeScale = oldScale; }
    }
}
#endif
