using UnityEngine;

namespace Luna.Gameplay
{
    public sealed class SlimePlayerLogic : PlayerLogic
    {
        protected override void ConfigureUnitForSpawn()
        {
            base.ConfigureUnitForSpawn();
            coefficient.Armour *= 1.1f;
            StatusAfterReady();
        }
    }

    public sealed class RichPlayerLogic : PlayerLogic
    {
        public override float IncomeMultiplier => 1.1f;
    }

    public sealed class DragonPlayerLogic : PlayerLogic
    {
        private int petId;
        public UnitLogic DragonPet => LunaSpawn.Find<UnitLogic>(petId);

        protected override void ConfigureUnitForSpawn()
        {
            base.ConfigureUnitForSpawn();
            GameObject prefab = Resources.Load<GameObject>("Prefabs/00Luna/06_DragonPet");
            if (prefab == null) throw new System.InvalidOperationException("Dragon player requires its authored pet prefab.");
            Vector3 offset = new Vector3(View.GetFloat("petLocalPositionX"), View.GetFloat("petLocalPositionY"), View.GetFloat("petLocalPositionZ"));
            var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), transform.TransformPoint(offset));
            data.OwnerId = ID; data.RoundId = Data.RoundId; data.InitialMode = Data.InitialMode;
            data.Parent = transform;
            int ownerId = ID;
            petId = data.Id;
            LunaSpawn.Show<UnitLogic>(prefab, data, pet =>
            {
                if (Data == null || ID != ownerId || IsHiding) { pet.SafeHide(); return; }
            });
        }

        protected override void OnUnitHidden()
        {
            LunaSpawn.Cancel(petId); petId = 0;
            base.OnUnitHidden();
        }

        protected override void OnUnitDying()
        {
            LunaSpawn.Cancel(petId); petId = 0;
            base.OnUnitDying();
        }
    }

    /// <summary>Pet behavior is owned by its native unit logic; no nested scene-authored actor is adopted.</summary>
    public sealed class DragonPetLogic : UnitLogic
    {
        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            if (Data != null && Data.OwnerId != 0 && Find(Data.OwnerId) == null) SafeHide();
        }
    }
}
