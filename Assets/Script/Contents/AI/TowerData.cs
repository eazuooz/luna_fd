using System.Collections.Generic;
using UnityEngine;

namespace Luna.Gameplay
{
    public enum TurretKind { None, Arrow, Fire, Ice }

    public sealed class TowerData : UnitData
    {
        public TurretKind Turret;
        public GameObject TurretProjectile;
        public AudioClip TurretSound;
        public float TurretRange, TurretDamage, TurretInterval, TurretStun;

        public new static TowerData CreateFrom(UnitView view, Vector3 position)
        {
            var data = LunaEntityData.Create<TowerData>();
            FillFrom(data, view, position);
            var available = new List<TurretKind>();
            if (view.GetPrefab("TurretArrow") != null) available.Add(TurretKind.Arrow);
            if (view.GetPrefab("TurretFire") != null) available.Add(TurretKind.Fire);
            if (view.GetPrefab("TurretIce") != null) available.Add(TurretKind.Ice);
            if (available.Count == 0) return data;
            data.Turret = available[Random.Range(0, available.Count)];
            string key = data.Turret.ToString();
            data.TurretProjectile = view.GetPrefab("Turret" + key);
            data.TurretSound = view.GetSound(key + "Sound");
            data.TurretRange = view.GetFloat(key + "Distance", data.Turret == TurretKind.Arrow ? 6f : 7f);
            data.TurretDamage = view.GetFloat(key + "Power", data.Turret == TurretKind.Arrow ? 100f : 300f);
            data.TurretInterval = Mathf.Max(0.05f, view.GetFloat(key + "Term", data.Turret == TurretKind.Arrow ? 1.5f : data.Turret == TurretKind.Fire ? 2.5f : 3.5f));
            data.TurretStun = data.Turret == TurretKind.Ice ? 2f : 0f;
            return data;
        }

        public override void Clear()
        {
            base.Clear();
            Turret = TurretKind.None; TurretProjectile = null; TurretSound = null;
            TurretRange = TurretDamage = TurretInterval = TurretStun = 0f;
        }
    }
}
