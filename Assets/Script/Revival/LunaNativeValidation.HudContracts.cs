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
    private static void ProtectHudRound(BasicDungeon round)
    {
        Require(round != null && round.Player != null && round.gameAllyTower != null && round.gameEnemyTower != null, "HUD fixture actor graph is incomplete.");
        round.Player.Invincible(0f);
        UnitLogic.From(round.gameAllyTower).Invincible(0f);
        UnitLogic.From(round.gameEnemyTower).Invincible(0f);
    }

    private IEnumerator HudContracts()
    {
        if (Includes("lifecycle", "hud-round-ownership"))
        {
            BeginCase("lifecycle", "hud-round-ownership");
            float previousScale = Time.timeScale;
            var pickups = new Dictionary<int, int>(LunaSession.Current.pickUpData);
            try
            {
                yield return LoadStage("Stage01-01");
                var round = (BasicDungeon)LunaScenes.CurrentGame;
                ProtectHudRound(round);
                var roster = round.Roster;
                int index = Enumerable.Range(0, 6).First(value => roster.GetSlot(value).Prefab != null && roster.GetSlot(value).Definition.Price <= roster.MaxGold);
                var slot = roster.GetSlot(index);
                roster.Gold = roster.MaxGold;
                Time.timeScale = 8f;
                yield return Until(() => roster.CanSummon(index), "Normal summon cooldown", 30f);
                Time.timeScale = 0f;
                yield return Until(() => LunaSpawn.PendingCount == 0, "Pre-existing round requests settled");
                roster.Mana = 73;
                var player = (PlayerLogic)round.Player;
                int playerId = player.ID;
                var abilities = player.Abilities;
                var abilityCooldowns = LunaDataTable.Instance.playerData.equippedSkill.Distinct().ToDictionary(key => key, key => abilities.Remaining(LunaDataTable.Instance.skiiTable[key]));
                int equipmentEntities = Entities().Count(actor => actor.SpawnData.OwnerId == playerId);
                LunaSession.Current.pickUpData.Clear();
                int item = LunaDataTable.Instance.itemTable.Keys.First();
                LunaSession.Current.pickUpData.Add(item, 3);
                Require(roster.TrySummon(index), "Ready round-owned summon was rejected.");
                Require(slot.Pending && roster.PendingCount == 1, "HUD fixture did not exercise an outstanding native summon.");
                // Keep a nonzero balance which visibly lowers unaffordable, non-live cards.
                roster.Gold = 1;
                int id = slot.EntityId, gold = roster.Gold, mana = roster.Mana;
                float cooldown = slot.Remaining;
                // Close both forms before the asynchronous shown callback can be dispatched.
                LunaUI.Close(UI_TYPE.PLAYER); LunaUI.Close(UI_TYPE.SKILL);
                yield return Until(() => NativeUi(UI_TYPE.PLAYER) == null && NativeUi(UI_TYPE.SKILL) == null, "HUD pair closed");
                yield return Until(() => !slot.Pending, "Summon completed while HUD was closed");
                Require(slot.LiveUnit != null && slot.EntityId == id && slot.LiveUnit.Data.RoundId == round.RoundId,
                    "Closing the HUD canceled or orphaned the native summon.");
                Require(roster.SlotsReadOnly.Any(value => value.Definition != null && value.Definition.Price > gold && value.LiveUnit == null),
                    "HUD position fixture has no unaffordable, non-live card.");
                Vector3[] cardPositions = null;
                for (int cycle = 0; cycle < 3; cycle++)
                {
                    yield return OpenNativeUi(UI_TYPE.PLAYER);
                    yield return OpenNativeUi(UI_TYPE.SKILL);
                    var hud = (UIPlayer)NativeUi(UI_TYPE.PLAYER);
                    Require(ReferenceEquals(round.Roster, roster), "HUD reopen replaced the round roster.");
                    Require(roster.Gold == gold && roster.Mana == mana && hud.Money == gold && hud.Mana == mana,
                        "HUD reopen changed round-owned currency.");
                    Require(slot.EntityId == id && slot.Remaining == cooldown && !slot.Pending && roster.PendingCount == 0,
                        "HUD reopen reset summon identity, cooldown or pending accounting.");
                    Require(hud.SummonCards.Count == 6 && ReferenceEquals(hud.SummonCards[index].Slot, slot), "HUD cards do not observe their existing round slots.");
                    hud.ButtonDisableChecker();
                    var positions = hud.SummonCards.Select(card => card.button.transform.localPosition).ToArray();
                    if (cycle == 0) cardPositions = positions;
                    else for (int card = 0; card < positions.Length; card++)
                        Require((positions[card] - cardPositions[card]).sqrMagnitude < 0.000001f,
                            "Frozen HUD card changed position on reopen: slot " + card + ", " + cardPositions[card] + " -> " + positions[card]);
                    Require(round.Player.ID == playerId && ReferenceEquals(((PlayerLogic)round.Player).Abilities, abilities), "HUD reopen recreated the player ability controller.");
                    Require(abilityCooldowns.All(pair => abilities.Remaining(LunaDataTable.Instance.skiiTable[pair.Key]) == pair.Value), "HUD reopen changed a player-owned ability cooldown.");
                    Require(Entities().Count(actor => actor.ID == id) == 1 && Entities().Count(actor => actor.SpawnData.OwnerId == playerId) == equipmentEntities,
                        "HUD reopen duplicated a native summon or equipment entity.");
                    Require(LunaSession.Current.pickUpData.Count == 1 && LunaSession.Current.pickUpData.TryGetValue(item, out int count) && count == 3,
                        "HUD reopen discarded or modified pending round loot.");
                    CheckOwnership();
                    if (cycle < 2) { yield return CloseNativeUi(UI_TYPE.PLAYER); yield return CloseNativeUi(UI_TYPE.SKILL); }
                }
                EndCase("A pending native summon survived closed forms; a baseline open and two reopens preserved six card positions, the same roster, exact frozen currencies/cooldown/loot, native IDs and player abilities.");
            }
            finally
            {
                Time.timeScale = previousScale;
                LunaSession.Current.pickUpData.Clear();
                foreach (var pair in pickups) LunaSession.Current.pickUpData.Add(pair.Key, pair.Value);
            }
        }

        if (Includes("lifecycle", "skill-hud-rebinds-after-player-death"))
        {
            BeginCase("lifecycle", "skill-hud-rebinds-after-player-death");
            float previousScale = Time.timeScale;
            var playerData = LunaDataTable.Instance.playerData;
            var savedSkills = playerData.equippedSkill.ToArray();
            var savedSkin = playerData.SkinType;
            try
            {
                playerData.SkinType = PlayerData.eLunaSkin.Basic;
                playerData.equippedSkill.Clear(); playerData.equippedSkill.Add("Thunder");
                Time.timeScale = 1f;
                yield return LoadStage("Stage01-01");
                var round = (BasicDungeon)LunaScenes.CurrentGame;
                ProtectHudRound(round);
                HoldRoundActors(round);
                var dying = (PlayerLogic)round.Player;
                int deadId = dying.ID;
                Require(((UISkill)NativeUi(UI_TYPE.SKILL)).BoundPlayerId == deadId,
                    "Initial skill HUD is not bound to the round player.");
                dying.stat.Hp = 0f;
                dying.BeginDeath();
                yield return Until(() => round.PlayerRespawnPending, "Actual player death scheduled round respawn", 15f);
                Time.timeScale = 0f;
                yield return CloseNativeUi(UI_TYPE.SKILL);
                yield return OpenNativeUi(UI_TYPE.SKILL);
                var skill = (UISkill)NativeUi(UI_TYPE.SKILL);
                Require(skill.BoundPlayerId == 0 && !skill.transform.Find("Skill_Button_0").gameObject.activeSelf,
                    "Skill HUD reopened during death with stale player bindings or an actionable button.");
                playerData.equippedSkill.Clear(); playerData.equippedSkill.Add("FireBall");
                Time.timeScale = 8f;
                yield return Until(() => round.Player != null && round.Player.ID != deadId && !round.PlayerRespawnPending &&
                    skill.BoundPlayerId == round.Player.ID, "Skill HUD rebound to the native respawn", 30f);
                var revived = (PlayerLogic)round.Player;
                revived.Invincible(0f);
                Require(revived.Abilities != null && revived.Abilities.Loadout.Skills.Count == 1 &&
                    revived.Abilities.Loadout.Skills[0].name == "FireBall", "Respawn did not capture the changed player loadout.");
                var snapshot = revived.Abilities.Loadout.Skills[0];
                var button = skill.transform.Find("Skill_Button_0").GetComponent<Button>();
                Require(button.gameObject.activeSelf && button.transform.Find("Skill_Button_Image").GetComponent<Image>().sprite == snapshot.skillImage &&
                    !skill.transform.Find("Skill_Button_1").gameObject.activeSelf && !skill.transform.Find("Skill_Button_2").gameObject.activeSelf,
                    "Rebound skill HUD retained obsolete icons or extra skill slots.");
                yield return Until(() => revived.Abilities.Ready(snapshot), "Rebound skill's normal cooldown", 30f);
                Time.timeScale = 0f;
                round.Roster.Mana = round.Roster.MaxMana;
                int before = round.Roster.Mana;
                Require(snapshot.mana > 0 && before >= snapshot.mana, "Rebind skill payment fixture is not affordable.");
                button.onClick.Invoke(); button.onClick.Invoke();
                Require(round.Roster.Mana == before - snapshot.mana && revived.Abilities.Remaining(snapshot) == snapshot.coolTime,
                    "Rebound button failed its new skill or charged multiple times during one cooldown.");
                Require(!skill.transform.Find("mask").gameObject.activeSelf && !skill.TrySubmitThunderTarget(Vector3.zero),
                    "Rebound FireBall button retained the dead player's Thunder targeting callback.");
                CheckOwnership();
                EndCase("The real Basic player died; a skill HUD opened while dead stayed inert, then rebound to the new native ID and changed loadout, showed its exact snapshot icon, and charged once for repeated button requests.");
            }
            finally
            {
                LunaUI.Close(UI_TYPE.SKILL);
                Time.timeScale = previousScale;
                playerData.SkinType = savedSkin;
                playerData.equippedSkill.Clear(); playerData.equippedSkill.AddRange(savedSkills);
            }
        }

        if (Includes("lifecycle", "targeting-close-cancels-without-payment"))
        {
            BeginCase("lifecycle", "targeting-close-cancels-without-payment");
            float previousScale = Time.timeScale;
            var equipped = LunaDataTable.Instance.playerData.equippedSkill;
            var savedSkills = equipped.ToArray();
            try
            {
                equipped[0] = "Thunder";
                yield return LoadStage("Stage01-01");
                var round = (BasicDungeon)LunaScenes.CurrentGame;
                ProtectHudRound(round);
                var player = (PlayerLogic)round.Player;
                SkillData thunder = LunaDataTable.Instance.skiiTable["Thunder"];
                Time.timeScale = 8f;
                yield return Until(() => player.Abilities.Ready(thunder), "Real Thunder cooldown", 30f);
                Time.timeScale = 1.25f;
                round.Roster.Mana = 100;
                var skill = (UISkill)NativeUi(UI_TYPE.SKILL);
                Require(skill != null, "Real skill HUD is missing.");
                skill.transform.Find("Skill_Button_0").GetComponent<Button>().onClick.Invoke();
                Require(Time.timeScale == 0.5f && skill.transform.Find("mask").gameObject.activeSelf, "Real Thunder button did not enter targeting mode.");
                Require(round.Roster.Mana == 100, "Targeting charged mana before a target was accepted.");
                LunaUI.Close(UI_TYPE.SKILL);
                Require(Time.timeScale == 1.25f, "Closing the target form did not restore the previous time scale.");
                Require(round.Roster.Mana == 100 && player.Abilities.Ready(thunder), "Canceled targeting spent mana or started an ability cooldown.");
                Require(!skill.TrySubmitThunderTarget(Vector3.zero), "Closed skill form retained a live target request.");
                Time.timeScale = 0f;
                yield return OpenNativeUi(UI_TYPE.SKILL);
                Require(!NativeUi(UI_TYPE.SKILL).transform.Find("mask").gameObject.activeSelf, "Reopened skill form retained its targeting overlay.");
                CheckOwnership();
                EndCase("The actual Thunder button entered targeting after its real cooldown; closing the native form restored time scale and canceled without mana or cooldown payment.");
            }
            finally
            {
                LunaUI.Close(UI_TYPE.SKILL);
                Time.timeScale = previousScale;
                equipped.Clear(); equipped.AddRange(savedSkills);
            }
        }
    }
}
#endif
