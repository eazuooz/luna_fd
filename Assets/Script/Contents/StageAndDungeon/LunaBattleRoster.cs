using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Round-owned resources and six summon slots. A HUD may observe this object but never owns its lifetime.</summary>
    public sealed class LunaBattleRoster
    {
        public sealed class Slot
        {
            public GameObject Prefab { get; internal set; }
            public ObjectData Definition { get; internal set; }
            public int EntityId { get; internal set; }
            public bool Pending { get; internal set; }
            public float Cooldown { get; internal set; }
            public float Remaining { get; internal set; }
            public UnitLogic LiveUnit => LunaSpawn.Find<UnitLogic>(EntityId);
        }

        private readonly LunaGame round;
        private readonly int roundId;
        private readonly bool automaticParty;
        private readonly Slot[] slots = new Slot[6];
        private readonly ReadOnlyCollection<Slot> readOnlySlots;
        private readonly HashSet<int> ownedIds = new HashSet<int>();
        private readonly HashSet<int> pendingIds = new HashSet<int>();
        private readonly List<int> retiredIds = new List<int>();
        private bool started, stopped;
        private int gold, mana, summonSerial;
        private double goldFraction, manaFraction;
        public int MaxGold => 100;
        public int MaxMana => 100;
        public int Gold { get => gold; set => gold = Mathf.Clamp(value, 0, MaxGold); }
        public int Mana { get => mana; set => mana = Mathf.Clamp(value, 0, MaxMana); }
        public int PendingCount => pendingIds.Count;
        public IReadOnlyList<Slot> SlotsReadOnly => readOnlySlots;
        private bool IsCombat => started && !stopped && round.IsActive && round.RoundId == roundId && !round.isStageStop;

        public LunaBattleRoster(LunaGame owner)
        {
            round = owner ?? throw new ArgumentNullException(nameof(owner));
            roundId = owner.RoundId;
            Gold = owner.Definition.Row.StartGold;
            Mana = owner.Definition.Row.StartMana;
            if (roundId <= 0) throw new ArgumentException("Roster requires an initialized round.", nameof(owner));
            automaticParty = LunaSession.Current != null && LunaSession.Current.doNotSummonMode;
            var deck = LunaDataTable.Instance.playerData.UnitNamesInDeck;
            if (deck == null || deck.Count != slots.Length) throw new InvalidOperationException("A battle roster requires six selected deck slots.");
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i] = new Slot();
                if (string.IsNullOrEmpty(deck[i])) continue;
                string resource = (i < 4 ? "Prefabs/01Heroes/" : "Prefabs/02Soldiers/") + deck[i];
                slot.Prefab = Resources.Load<GameObject>(resource);
                if (slot.Prefab == null) throw new InvalidOperationException("Selected roster prefab is missing: " + resource);
                var view = slot.Prefab.GetComponent<UnitView>();
                if (view == null || view.Definition == null) throw new InvalidOperationException("Selected roster unit has no authored definition: " + resource);
                slot.Definition = view.Data;
                slot.Cooldown = Mathf.Max(0f, slot.Definition.SpawnDelayTime);
                slot.Remaining = slot.Cooldown;
            }
            readOnlySlots = Array.AsReadOnly(slots);
        }

        public Slot GetSlot(int index)
        {
            if (index < 0 || index >= slots.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return slots[index];
        }

        public void StartCombat()
        {
            if (started || stopped || !round.IsActive) return;
            started = true;
            if (!automaticParty) return;
            // Dungeon04 receives its selected party once. Closing/reopening a HUD has no effect.
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].Prefab != null) RequestSummon(slots[i], PartyPosition(), 0);
        }

        public void Tick(float elapsed)
        {
            if (!IsCombat || elapsed == 0f) return;
            if (elapsed < 0f || float.IsNaN(elapsed) || float.IsInfinity(elapsed)) throw new ArgumentOutOfRangeException(nameof(elapsed));
            foreach (var slot in slots) slot.Remaining = Mathf.Max(0f, slot.Remaining - elapsed * 5f);
            var player = round.Player as PlayerLogic;
            float income = 1f + Defined.AddGoldSpeedValue * (LunaDataTable.Instance.playerData.GoldSpeedLevel + (player?.Abilities?.GoldBonus ?? 0f));
            income *= 1f + HeroPartyEffects.GoldBonusForRound(roundId) / 100f;
            income *= player?.Abilities?.IncomeMultiplier ?? 1f;
            if (player is RichPlayerLogic rich) income *= rich.IncomeMultiplier;
            if (float.IsNaN(income) || float.IsInfinity(income)) throw new InvalidOperationException("Non-finite battle income.");
            goldFraction += elapsed * 2d * Mathf.Max(0f, income);
            manaFraction += elapsed * 2d;
            double earnedGold = Math.Floor(goldFraction), earnedMana = Math.Floor(manaFraction);
            goldFraction -= earnedGold; manaFraction -= earnedMana;
            // Limit conversion before casting so even a large diagnostic timestep cannot overflow.
            Gold += (int)Math.Min(MaxGold, earnedGold);
            Mana += (int)Math.Min(MaxMana, earnedMana);
            retiredIds.Clear();
            foreach (int id in ownedIds)
                if (!pendingIds.Contains(id) && LunaSpawn.Find<UnitLogic>(id) == null) retiredIds.Add(id);
            foreach (int id in retiredIds) ownedIds.Remove(id);
        }

        public bool CanSummon(int index)
        {
            if (!IsCombat || automaticParty || index < 0 || index >= slots.Length) return false;
            var slot = slots[index];
            return slot.Prefab != null && slot.Definition != null && !slot.Pending && slot.Remaining <= 0f
                && Gold >= Mathf.Max(0, slot.Definition.Price)
                && (slot.Definition.UnitType != eUnitType.Hero || slot.LiveUnit == null);
        }

        public bool TrySummon(int index)
        {
            if (!CanSummon(index)) return false;
            var position = Defined.AllyTowerPosition;
            position.z = 0.00001f * ++summonSerial;
            return RequestSummon(slots[index], position, Mathf.Max(0, slots[index].Definition.Price));
        }

        private Vector3 PartyPosition()
        {
            var position = Defined.AllyTowerPosition;
            if (round is EndlessDungeon endless && endless.randomYDistance)
            {
                float y = UnityEngine.Random.Range(-0.3f, 1f);
                position.y += y; position.z += y;
            }
            else position.z = UnityEngine.Random.Range(0.00001f, 0.10001f);
            return position;
        }

        private bool RequestSummon(Slot slot, Vector3 position, int price)
        {
            if (stopped || !round.IsActive || slot.Pending) return false;
            var data = UnitData.CreateFrom(slot.Prefab.GetComponent<UnitView>(), position);
            data.RoundId = roundId;
            data.HealthMultiplier = data.AttackMultiplier = LunaEconomy.RankCombatMultiplier(data.Rank);
            data.Rotation = Quaternion.identity;
            data.InitialMode = round.isStageStop ? UnitSpawnMode.Staged : UnitSpawnMode.Battle;
            int id = data.Id;
            bool settled = false, failedSynchronously = false;
            Gold -= price;
            slot.EntityId = id; slot.Pending = true; slot.Remaining = slot.Cooldown;
            ownedIds.Add(id); pendingIds.Add(id);
            Action<string> fail = error =>
            {
                if (settled) return;
                settled = true; failedSynchronously = true;
                pendingIds.Remove(id); ownedIds.Remove(id);
                if (slot.EntityId == id) { slot.Pending = false; slot.EntityId = 0; slot.Remaining = 0f; }
                if (!stopped) Gold += price;
                Debug.LogError("Round " + roundId + " unit summon failed: " + error);
            };
            try
            {
                LunaSpawn.Show<UnitLogic>(slot.Prefab, data, actor =>
                {
                    if (settled) { actor.SafeHide(); return; }
                    settled = true; pendingIds.Remove(id);
                    if (slot.EntityId == id) slot.Pending = false;
                    if (stopped || !round.IsActive || round.RoundId != roundId || actor.SpawnData == null || actor.SpawnData.RoundId != roundId)
                    { ownedIds.Remove(id); actor.SafeHide(); return; }
                    actor.unitState.IsLobbyMove = round.isStageStop;
                    actor.Data.InitialMode = round.isStageStop ? UnitSpawnMode.Staged : UnitSpawnMode.Battle;
                    // The slot already holds its new native ID. No UI availability controls this lifetime.
                }, fail);
            }
            catch
            {
                if (!settled)
                {
                    settled = true; pendingIds.Remove(id); ownedIds.Remove(id);
                    if (slot.EntityId == id) { slot.Pending = false; slot.EntityId = 0; slot.Remaining = 0f; }
                    if (!stopped) Gold += price;
                }
                throw;
            }
            return !failedSynchronously;
        }

        public void Shutdown()
        {
            if (stopped) return;
            stopped = true;
            foreach (int id in ownedIds) LunaSpawn.Cancel(id);
            ownedIds.Clear(); pendingIds.Clear();
            foreach (var slot in slots) { slot.Pending = false; slot.EntityId = 0; }
        }
    }
}
