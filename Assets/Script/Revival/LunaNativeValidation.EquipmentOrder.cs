#if UNITY_EDITOR || DEBUG
using System.Collections;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;

public sealed partial class LunaNativeValidation
{
    private IEnumerator GearOrderContracts()
    {
        yield return EnsureFixtureLobby();
        var progression = LunaDataTable.Instance.playerData;
        var oldSkin = progression.SkinType;
        float oldScale = Time.timeScale;
        try
        {
            Time.timeScale = 4f;
            foreach (var skin in new[] { PlayerData.eLunaSkin.SkullAx, PlayerData.eLunaSkin.Slime })
            foreach (bool buffFirst in new[] { false, true })
            {
                string label = "equipment-order-" + skin + (buffFirst ? "-buff-before-gear" : "-gear-before-buff");
                if (!Includes("buffs", label)) continue;
                BeginCase("buffs", label);
                int round = NewFixtureRound();
                try
                {
                    progression.SkinType = skin;
                    var targetPrefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
                    var targetData = UnitData.CreateFrom(targetPrefab.GetComponent<UnitView>(), Vector3.zero);
                    targetData.RoundId = round; targetData.Side = eUnitSide.Allied; targetData.InitialMode = UnitSpawnMode.Staged;
                    UnitLogic target = null;
                    yield return ShowFixture<UnitLogic>(targetPrefab, targetData, actor => target = actor);
                    HoldAutomaticActions(target);
                    float baselineAttack = target.stat.AttackDamage, baselineInterval = target.stat.AttackSpeed;
                    if (buffFirst) { target.TakeBeleifWaveMode(5f); target.TakeBerserkMode(5f); }

                    var playerPrefab = LunaSession.Current.Players[(int)skin];
                    var playerData = UnitData.CreateFrom(playerPrefab.GetComponent<UnitView>(), Vector3.left * 3f);
                    playerData.RoundId = round; playerData.Side = eUnitSide.Allied; playerData.InitialMode = UnitSpawnMode.Staged;
                    PlayerLogic player = null;
                    yield return ShowFixture<PlayerLogic>(playerPrefab, playerData, actor => player = actor);
                    HoldAutomaticActions(player);
                    float attackFactor = player.Abilities.Loadout.Equipment.Where(gear => gear.name == "LunaEquipmentSkullSword").Aggregate(1f, (product, gear) => product * (1f + gear.coefficient / 100f));
                    float speedFactor = player.Abilities.Loadout.Equipment.Where(gear => gear.name == "LunaEquipmentSlimeShield").Aggregate(1f, (product, gear) => product * (1f + gear.coefficient / 100f));
                    Require(attackFactor > 1f || speedFactor > 1f, "Equipment-order fixture has no actual attack/speed equipment.");
                    if (!buffFirst) { target.TakeBeleifWaveMode(5f); target.TakeBerserkMode(5f); }
                    RequireEquipmentStats(target, baselineAttack * 1.15f * attackFactor, baselineInterval / (1.5f * speedFactor), "gear + both timed buffs");
                    Require(Mathf.Abs(target.coefficient.AttackDamage - 1.15f) < 0.0001f && Mathf.Abs(target.coefficient.AttackSpeed - 1.5f) < 0.0001f,
                        "Equipment mutated the additive timed-buff coefficients.");

                    int ownerId = player.ID;
                    if (!buffFirst)
                    {
                        player.stat.Hp = 0f; player.BeginDeath();
                        RequireEquipmentStats(target, baselineAttack * 1.15f, baselineInterval / 1.5f, "owner death with both buffs still active");
                        Require(target.IsBerserk, "Owner death cancelled an independently owned recipient buff.");
                    }
                    yield return Until(() => !target.IsBerserk && Mathf.Abs(target.coefficient.AttackDamage - 1f) < 0.0001f, "Natural equipment-overlap buff expiry", 8f);
                    RequireEquipmentStats(target, baselineAttack * (buffFirst ? attackFactor : 1f), baselineInterval / (buffFirst ? speedFactor : 1f), "natural buff expiry");
                    if (buffFirst)
                    {
                        player.stat.Hp = 0f; player.BeginDeath();
                        RequireEquipmentStats(target, baselineAttack, baselineInterval, "owner death after natural buff expiry");
                    }
                    yield return Until(() => LunaSpawn.Find<PlayerLogic>(ownerId) == null, "Equipment source native death retirement");

                    // Interrupt a new modifier on a real source identity and verify the pooled target cannot inherit it.
                    var nextPlayerData = UnitData.CreateFrom(playerPrefab.GetComponent<UnitView>(), Vector3.left * 3f);
                    nextPlayerData.RoundId = round; nextPlayerData.Side = eUnitSide.Allied; nextPlayerData.InitialMode = UnitSpawnMode.Staged;
                    yield return ShowFixture<PlayerLogic>(playerPrefab, nextPlayerData, actor => player = actor);
                    RequireEquipmentStats(target, baselineAttack * attackFactor, baselineInterval / speedFactor, "new owner identity reapplies once");
                    int oldTargetId = target.ID;
                    target.SafeHide(); yield return null;
                    var freshData = UnitData.CreateFrom(targetPrefab.GetComponent<UnitView>(), Vector3.zero);
                    freshData.RoundId = round; freshData.Side = eUnitSide.Allied; freshData.InitialMode = UnitSpawnMode.Staged;
                    yield return ShowFixture<UnitLogic>(targetPrefab, freshData, actor => target = actor);
                    Require(target.ID != oldTargetId && LunaSpawn.Find<UnitLogic>(oldTargetId) == null, "Retired equipment target identity still resolves.");
                    RequireEquipmentStats(target, baselineAttack, baselineInterval, "new staged target has no retired-life equipment");
                    current.observations.Add("Attack factor=" + attackFactor + "; speed factor=" + speedFactor + "; additive buffs restored exactly across source death and target reuse.");
                    EndCase("Actual player gear and native recipient buffs are order-independent, source-owned, and removed exactly once without coefficient residue.");
                }
                finally { LunaSpawn.CancelRound(round); }
                yield return RetireFixture(round);
            }
            yield return DragonEquipmentOrderContract();
        }
        finally { progression.SkinType = oldSkin; Time.timeScale = oldScale; }
    }

    private IEnumerator DragonEquipmentOrderContract()
    {
        const string label = "equipment-order-Dragon-pet";
        if (!Includes("buffs", label)) yield break;
        BeginCase("buffs", label);
        int round = NewFixtureRound();
        try
        {
            LunaDataTable.Instance.playerData.SkinType = PlayerData.eLunaSkin.Dragon;
            var prefab = LunaSession.Current.Players[(int)PlayerData.eLunaSkin.Dragon];
            var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), Vector3.zero);
            data.RoundId = round; data.Side = eUnitSide.Allied; data.InitialMode = UnitSpawnMode.Staged;
            DragonPlayerLogic player = null;
            yield return ShowFixture<DragonPlayerLogic>(prefab, data, actor => player = actor);
            yield return Until(() => player.DragonPet != null, "Dragon equipment recipient native show");
            var pet = player.DragonPet;
            HoldAutomaticActions(pet);
            float baseline = (pet.objectData.AttackDamage + (pet.Level - 1) * pet.objectData.LevelUpAttPoint) * pet.Data.AttackMultiplier;
            float factor = player.Abilities.Loadout.Equipment.Where(gear => gear.name == "LunaEquipmentDragonShield").Aggregate(1f, (product, gear) => product * (1f + gear.coefficient / 100f));
            Require(baseline > 0f && factor > 1f, "Dragon fixture has no real pet attack or shield multiplier.");
            pet.TakeBeleifWaveMode(3f);
            HoldAutomaticActions(player);
            yield return Until(() => Mathf.Abs(pet.stat.AttackDamage - baseline * 1.15f * factor) < 0.05f, "Dragon pet gear with pre-existing timed attack buff");
            Require(Mathf.Abs(pet.coefficient.AttackDamage - 1.15f) < 0.0001f, "Dragon gear changed the pet's timed coefficient.");
            yield return Until(() => Mathf.Abs(pet.coefficient.AttackDamage - 1f) < 0.0001f, "Dragon pet natural attack-buff expiry");
            Require(Mathf.Abs(pet.stat.AttackDamage - baseline * factor) < 0.05f, "Dragon pet retained an attack-buff residue.");
            int petId = pet.ID, ownerId = player.ID;
            pet.TakeBeleifWaveMode(10f);
            player.stat.Hp = 0f; player.BeginDeath();
            yield return Until(() => LunaSpawn.Find<UnitLogic>(petId) == null && LunaSpawn.Find<PlayerLogic>(ownerId) == null, "Dragon owner death retires the buffed pet");
            var next = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), Vector3.zero);
            next.RoundId = round; next.Side = eUnitSide.Allied; next.InitialMode = UnitSpawnMode.Staged;
            yield return ShowFixture<DragonPlayerLogic>(prefab, next, actor => player = actor);
            HoldAutomaticActions(player);
            yield return Until(() => player.DragonPet != null, "New dragon pet native show");
            HoldAutomaticActions(player.DragonPet);
            yield return Until(() => Mathf.Abs(player.DragonPet.stat.AttackDamage - baseline * factor) < 0.05f, "New dragon pet receives its gear once");
            Require(player.DragonPet.ID != petId && Mathf.Abs(player.DragonPet.coefficient.AttackDamage - 1f) < 0.0001f, "Dragon pet reused an interrupted buff or previous identity.");
            current.observations.Add("Pet base ATK=" + baseline + "; shield factor=" + factor + "; natural expiry and owner-death/reuse verified.");
            EndCase("Dragon-specific and ordinary owner gear share one final modifier without mutating pet buff coefficients.");
        }
        finally { LunaSpawn.CancelRound(round); }
        yield return RetireFixture(round);
    }

    private static void RequireEquipmentStats(UnitLogic target, float attack, float interval, string phase)
    {
        Require(Mathf.Abs(target.stat.AttackDamage - attack) < Mathf.Max(0.001f, Mathf.Abs(attack) * 0.00001f) &&
            Mathf.Abs(target.stat.AttackSpeed - interval) < Mathf.Max(0.0001f, Mathf.Abs(interval) * 0.00001f),
            "Equipment arithmetic mismatch after " + phase + ": ATK=" + target.stat.AttackDamage + "/" + attack + ", interval=" + target.stat.AttackSpeed + "/" + interval);
    }
}
#endif
