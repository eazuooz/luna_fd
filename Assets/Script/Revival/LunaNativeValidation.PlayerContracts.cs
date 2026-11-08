#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Linq;
using GameFramework.Event;
using Luna.Gameplay;
using UnityEngine;
using UnityGameFramework.Runtime;

public sealed partial class LunaNativeValidation
{
    private IEnumerator SkinContracts()
    {
        yield return EnsureFixtureLobby();
        var playerData = LunaDataTable.Instance.playerData;
        var oldSkin = playerData.SkinType;
        float oldTimeScale = Time.timeScale;
        try
        {
            Time.timeScale = 4f;
            foreach (PlayerData.eLunaSkin skin in Enum.GetValues(typeof(PlayerData.eLunaSkin)))
            {
                if (!Includes("skins", skin.ToString())) continue;
                BeginCase("skins", skin.ToString());
                int round = NewFixtureRound(), deaths = 0, expectedPlayer = 0;
                var events = GameEntry.GetComponent<EventComponent>();
                PlayerLogic observedPlayer = null;
                Spine.AnimationState.TrackEntryDelegate completedTrace = null;
                EventHandler<GameEventArgs> onDeath = (sender, args) =>
                {
                    var death = (PlayerDiedEventArgs)args;
                    if (death.RoundId != round) return;
                    Require(death.EntityId == expectedPlayer, "Skin death event contained the wrong native spawn identity.");
                    deaths++;
                };
                events.Subscribe(PlayerDiedEventArgs.EventId, onDeath);
                try
                {
                    playerData.SkinType = skin;
                    var prefab = LunaSession.Current.Players[(int)skin];
                    var view = prefab.GetComponent<UnitView>();
                    float sourceHp = view.Definition.Hp;
                    var data = UnitData.CreateFrom(view, Vector3.zero);
                    data.RoundId = round; data.Level = 1; data.Side = eUnitSide.Allied; data.InitialMode = UnitSpawnMode.Staged;
                    PlayerLogic player = null;
                    yield return ShowFixture<PlayerLogic>(prefab, data, actor => player = actor);
                    expectedPlayer = player.ID;
                    Require(player.Abilities != null && player.Data.Definition != view.Definition, "Player did not initialize owned abilities/runtime data.");
                    var fsms = GameEntry.GetComponent<FsmComponent>();
                    Require(fsms.HasFsm<PlayerLogic>("LunaPlayer/" + player.ID) && !fsms.HasFsm<UnitLogic>(player.ID.ToString()), "Player did not replace the automatic unit FSM with its one native player FSM.");
                    float attackCoefficient = player.coefficient.AttackDamage, armourCoefficient = player.coefficient.Armour;
                    var targetPrefab = Resources.Load<GameObject>("Prefabs/02Soldiers/PorongSword");
                    var targetData = UnitData.CreateFrom(targetPrefab.GetComponent<UnitView>(), Vector3.right);
                    targetData.RoundId = round; targetData.Side = eUnitSide.Enemy; targetData.InitialMode = UnitSpawnMode.Staged; targetData.HealthMultiplier = 1000f;
                    UnitLogic target = null;
                    yield return ShowFixture<UnitLogic>(targetPrefab, targetData, actor => target = actor);
                    HoldAutomaticActions(target); target.stat.AttackDamage = target.stat.Armour = 0f;
                    UnitLogic splashTarget = null;
                    float splashHp = 0f;
                    if (skin == PlayerData.eLunaSkin.SkullAx)
                    {
                        var splashData = UnitData.CreateFrom(targetPrefab.GetComponent<UnitView>(), player.transform.position + Vector3.right * (player.stat.AttackRange + 1.2f));
                        splashData.RoundId = round; splashData.Side = eUnitSide.Enemy; splashData.InitialMode = UnitSpawnMode.Staged; splashData.HealthMultiplier = 1000f;
                        yield return ShowFixture<UnitLogic>(targetPrefab, splashData, actor => splashTarget = actor);
                        HoldAutomaticActions(splashTarget); splashTarget.stat.AttackDamage = splashTarget.stat.Armour = splashTarget.stat.SkillResistance = 0f;
                        splashHp = splashTarget.stat.Hp;
                    }
                    HoldAutomaticActions(player); player.Data.TargetId = target.ID;
                    player.unitState.IsActionDelay = false;
                    float hp = target.stat.Hp;
                    yield return Until(() => target.stat.Hp < hp, "Skin native FSM attack: " + skin, 8f);
                    if (splashTarget != null) Require(Mathf.Abs(splashHp - splashTarget.stat.Hp - player.View.GetFloat("attackDamage", 200f)) <= 1f, "Skull's ordinary attack did not also deliver its authored splash to a separate target.");
                    player.unitState.IsActionDelay = true; player.stat.AttackDelayTime = -1000f;
                    Require(Finite(player.stat.Hp) && Finite(player.stat.Mp) && Finite(player.stat.Stamina), "Skin has non-finite combat values.");
                    int petId = 0;
                    if (player is DragonPlayerLogic dragon)
                    {
                        yield return Until(() => dragon.DragonPet != null, "Dragon native pet show");
                        var pet = dragon.DragonPet; petId = pet.ID;
                        Require(pet.Data.OwnerId == player.ID && pet.Data.RoundId == round && pet.transform.parent == player.transform, "Dragon pet lost typed owner/round/parent binding.");
                        Require(UnitLogic.ActiveUnits.Count(actor => actor.Data.OwnerId == player.ID && actor is DragonPetLogic) == 1, "Dragon skin has duplicate or missing native pets.");
                        Require(Finite(pet.stat.Mp) && Finite(pet.stat.Stamina), "Zero-stamina pet has non-finite resource values.");
                    }
                    player.transform.localScale = prefab.transform.localScale * 1.5f;
                    int firstId = player.ID;
                    observedPlayer = player;
                    completedTrace = entry =>
                    {
                        if (entry.Animation.Name == "idle" || current.observations.Count >= 64) return;
                        var activeTrack = player.Animation.AnimationState.GetCurrent(0);
                        current.observations.Add("Complete=" + entry.Animation.Name + ", current=" + activeTrack?.Animation.Name + ", sameTrack=" + ReferenceEquals(entry, activeTrack) + ", FSM=" + player.CurrentStateName + ", HP=" + (player.Data != null ? player.stat.Hp.ToString() : "hidden"));
                    };
                    player.Animation.AnimationState.Complete += completedTrace;
                    current.observations.Add("Before death: animation=" + player.Animation.AnimationState.GetCurrent(0)?.Animation.Name + ", FSM=" + player.CurrentStateName);
                    player.stat.Hp = 0f; player.BeginDeath();
                    current.observations.Add("After BeginDeath: animation=" + player.Animation.AnimationState.GetCurrent(0)?.Animation.Name);
                    if (petId != 0) Require(LunaSpawn.Find<UnitLogic>(petId) == null, "Dragon pet continued living after owner death-start.");
                    float deathStarted = Time.realtimeSinceStartup;
                    yield return Until(() =>
                    {
                        if (deaths == 1 && LunaSpawn.Find<PlayerLogic>(firstId) == null) return true;
                        if (Time.realtimeSinceStartup - deathStarted > 9f)
                        {
                            var track = player.Animation.AnimationState.GetCurrent(0);
                            throw new InvalidOperationException("Skin death stalled: events=" + deaths + ", resolves=" + (LunaSpawn.Find<PlayerLogic>(firstId) != null) + ", available=" + player.Available + ", hiding=" + player.IsHiding + ", HP=" + (player.Data != null ? player.stat.Hp.ToString() : "null") + ", mode=" + player.Data?.InitialMode + ", FSM=" + player.CurrentStateName + ", animation=" + track?.Animation.Name + ", trackTime=" + track?.TrackTime);
                        }
                        return false;
                    }, "Skin death notification and native retirement", 10f);
                    yield return Until(() => !UnitLogic.ActiveUnits.Any(actor => actor.Data.OwnerId == firstId) && LunaSpawn.PendingCount == 0, "Skin-owned summon cleanup");
                    yield return null;
                    var next = UnitData.CreateFrom(view, Vector3.zero);
                    next.RoundId = round; next.Level = 1; next.Side = eUnitSide.Allied; next.InitialMode = UnitSpawnMode.Staged;
                    PlayerLogic reused = null;
                    yield return ShowFixture<PlayerLogic>(prefab, next, actor => reused = actor);
                    Require(reused.ID != firstId && reused.Abilities != null && !reused.unitState.IsDeath && reused.stat.Hp > 0f, "Skin reuse retained dead identity or lost its controller.");
                    Require(Vector3.Distance(reused.transform.localScale, prefab.transform.localScale) < 0.001f, "Skin reuse retained modified local scale.");
                    Require(Mathf.Abs(reused.coefficient.AttackDamage - attackCoefficient) < 0.001f && Mathf.Abs(reused.coefficient.Armour - armourCoefficient) < 0.001f, "Skin/equipment coefficients stacked across native hide/show.");
                    if (reused is DragonPlayerLogic reusedDragon)
                    {
                        yield return Until(() => reusedDragon.DragonPet != null, "Dragon pet recreated for new owner ID");
                        Require(reusedDragon.DragonPet.ID != petId && reusedDragon.DragonPet.Data.OwnerId == reused.ID, "Reused dragon retained the previous pet identity.");
                    }
                    Require(view.Definition.Hp == sourceHp && deaths == 1, "Skin changed its authored source data or emitted duplicate death events.");
                    current.observations.Add("First ID=" + firstId + ", reused ID=" + reused.ID + ", target damage=" + (hp - target.stat.Hp) + ", death events=" + deaths + ", armour coefficient=" + reused.coefficient.Armour);
                }
                finally
                {
                    if (observedPlayer != null && completedTrace != null) observedPlayer.Animation.AnimationState.Complete -= completedTrace;
                    events.Unsubscribe(PlayerDiedEventArgs.EventId, onDeath); LunaSpawn.CancelRound(round);
                }
                yield return RetireFixture(round);
                EndCase("Actual player FSM attack, finite resources, one death event, owned child cleanup, typed fresh-ID reuse and stat/scale reset.");
            }
        }
        finally { playerData.SkinType = oldSkin; Time.timeScale = oldTimeScale; }
    }
}
#endif
