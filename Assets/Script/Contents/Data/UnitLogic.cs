using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Luna.Gameplay;
using GameFramework.Fsm;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
public partial class UnitLogic : EntityLogicBase
{
    public enum eAttackSoundType
    {
        DEFAULT = -1,
        SWORD = 0,
        MACE = 1,
        ARROW = 10,
        MAGIC = 11,
        THROW = 12,
    }

    private GameObject lunaAssetBundle;
    public GameObject LunaAssetBundle
    {
        set { lunaAssetBundle = value; }
    }
    public LunaAssetBundle GetLunaAssetBundle()
    {
        return lunaAssetBundle.GetComponent<LunaAssetBundle>();
    }

    public UnitData Data { get; private set; }
    public UnitView View { get; private set; }
    public SkeletonAnimation Animation { get; private set; }
    public bool IsStatusDataLoadComplete => Data != null && Available && !IsHiding;
    public ObjectData objectData => Data != null ? Data.Definition : View != null ? View.Definition : null;
    public AudioSource AudioSource;
    public AudioClip AttackSoundClip;
    public GameObject AttackEffectPrefab;
    private int healthBarId;
    private int playerAuraEffectId;
    private float playerAuraMoveBonus;
    public eAttackSoundType AttackSoundType = eAttackSoundType.DEFAULT;
    public string UnitName => objectData.UnitName;
    public int Level => Data.Level;
    public eHitPriority hitPriority => objectData.hitPriority;
    public eUltState ultState { get => Data.Rank; set => Data.Rank = value; }
    public eUnitSide UnitSide => Data.Side;
    public eUnitType UnitType => objectData.UnitType;
    public bool IsBossCategory => UnitType == eUnitType.Boss || UnitType == eUnitType.TowerAndBoss;
    public eUnitAttackType UnitAttackType => objectData.UnitAttackType;
    public Coefficient coefficient => Data.Coefficients;
    public Stat stat => Data.Stats;
    public UnitState unitState => Data.State;
    [HideInInspector] public List<GameObject> ProjecttileCreateLocations;
    [HideInInspector] public GameObject ProjecttileArrivalLocation;
    public GameObject AttackTarget
    {
        get { var target = Data != null ? LunaSpawn.Find<UnitLogic>(Data.TargetId) : null; return target != null && !target.IsHiding ? target.gameObject : null; }
        set { if (Data != null) Data.TargetId = value != null && From(value) != null ? From(value).ID : 0; }
    }
    public UnitLogic Target => Data != null ? LunaSpawn.Find<UnitLogic>(Data.TargetId) : null;
    public static UnitLogic From(GameObject instance) => instance != null ? instance.GetComponent<Entity>()?.Logic as UnitLogic : null;
    public static UnitLogic Find(int entityId) => LunaSpawn.Find<UnitLogic>(entityId);
    private static readonly HashSet<UnitLogic> activeUnits = new HashSet<UnitLogic>();
    public static IEnumerable<UnitLogic> ActiveUnits => activeUnits;
    [HideInInspector] public float LobbyDistance;
    [HideInInspector] public float HealthBarHpPoint;




    public float warfRange;
    public bool isWarf; // 순간이동 - 로엔, 클레어
    public bool isStun;// 스턴, 경직
    public bool isStunAttack;
    public float nextStunDuring;

    private bool SkillTrigger_Roni = false;
    private bool SkillTrigger_Rose = false;
    private bool SkillTrigger_Roen1 = false;
    private bool SkillTrigger_Roen2 = false;
    private bool SkillTrigger_Roen3 = false;
    private bool SkillTrigger_Elena = false;

    private bool curGrowUp; // 거대화 - 티라
    private bool curGrowDown; // 축소 - 티라
    private bool isPochiBarrier;
    private bool isPochiBarrier_ForPlayer;
    private bool isBleeding; // 출혈
    private bool isPoisoning; // 중독
    private bool isBerserk; // 광폭화
    private bool iSBeleifWave;
    private bool isSerenBlessing;
    private bool isLoneBlessing;
    private bool curRoenSkill;// Roen Skill and Ult
    private bool curInvincible;
    private bool curStealth;
    private bool curKnockback;
    private bool curStun;
    private bool skillSuperArmour;
    private float barrierReflect;
    private float playerBarrierReduction;
    private float curseSeverity;
    private float growUpRemaining, growDownRemaining;
    private Vector3 normalScale;
    public float CurseSeverity => curseSeverity;
    public bool HasPochiBarrier => isPochiBarrier;
    public bool IsBerserk => isBerserk;
    public bool IsBleeding => isBleeding;
    public bool IsPoisoning => isPoisoning;
    public bool IsInvincible => curInvincible;
    public bool IsStealthed => curStealth;



    private BuffImp buffImp = new BuffImp();
    public BuffImp BuffImp
    {
        get { return buffImp; }
    }



    

    
    public void ReleaseData()
    {
        ResetEquipmentModifiers();
        BuffImp.BuffIconOff();
        LunaSpawn.Cancel(healthBarId); healthBarId = 0;
        LunaSpawn.Cancel(playerAuraEffectId); playerAuraEffectId = 0;
        playerAuraMoveBonus = 0f;
        if (AudioSource != null) { AudioSource.Stop(); LunaSession.Current?.ObjectSounds.Remove(AudioSource); }
    }
    private void InitializeSpawnState()
    {
        ResetEquipmentModifiers();
        curseSeverity = 0f;
        InitializeCoefficentValues();
        LoadObjectData();
        ConnectHealthBar();
        SetPosition_Attack_Target();
        SetAudioSource();
        InitializeStatusCondition();
        BuffImp.UnitGameObject = gameObject;
        BuffImp.BuffIconInit();



    }
    void LoadObjectData() // 스크립터블오브젝트 내부 스테이터스 로드
    {
        if (objectData != null)
        {
            InitalizeLunaAssetBundle();
            InitializeUnitStats();

        }
    }
    private void InitalizeLunaAssetBundle()
    {
        lunaAssetBundle = Resources.Load<GameObject>("LunaAssetBundle");
    }
    
    
    void InitializeCoefficentValues() // 스테이터스 계수 초기화
    {
        coefficient.HP = Data.HealthMultiplier;
        coefficient.MP = 1.0f;
        coefficient.Stamina = 1.0f;
        coefficient.AttackDamage = Data.AttackMultiplier;
        coefficient.SkillPoint = 1.0f;
        coefficient.Armour = Data.ArmourMultiplier;
        coefficient.AttackSpeed = 1.0f;
        coefficient.MoveSpeed = 1.0f;

        stat.AttackAvoidance = 0.0f;
        stat.SkillAvoidance = 0.0f;
        stat.DebuffAvoidance = 0.0f;
        stat.StunResistance = 0f;
        stat.SkillResistance = 0f;
    }
    void InitializeUnitStats()
    {
        stat.Hp = (objectData.Hp + (Level - 1) * objectData.LevelUpHpPoint) * coefficient.HP;
        stat.Mp = objectData.GetCurrentLevelMP_Value(coefficient.MP);
        stat.Stamina = Mathf.Max(0f, objectData.Stamina * coefficient.Stamina);
        stat.AttackSpeed = objectData.GetAttackOnceBySecond_Value(coefficient.AttackSpeed * equipmentSpeedMultiplier);
        stat.MoveSpeed = MoveSpeedFromSpawnData();
        stat.Armour = (objectData.Armour + (Level - 1) * objectData.LevelUpArmourPoint) * coefficient.Armour;
        stat.AttackDamage = (Data.BaseAttackDamageOverride ?? (objectData.AttackDamage + (Level - 1) * objectData.LevelUpAttPoint)) * coefficient.AttackDamage * equipmentAttackMultiplier;
        stat.AttackRange = objectData.GetAttackRange_Value();

        stat.SkillResistance = Mathf.Clamp01(objectData.SkillResistance);
        stat.CriticalActuationProbability = objectData.CriticalProbability;
        stat.CriticalMultiples = objectData.CriticalMultiples;

        stat.SpawnDelayTime = objectData.SpawnDelayTime;
        stat.AttackDelayTime = 0.0f;

        LobbyDistance = 0.0f;
        HealthBarHpPoint = 0.0f;

        stat.ReflectPercentForBoss = 0.0f;
        stat.ReflectPowerForBoss = 0.0f;
        stat.ArmourPercentForBoss = 0.0f;
        stat.AttackPercentForBoss = 0.0f;

        unitState.InitializedUnitState(objectData);
    }

    void InitializeStatusCondition()
    {
        deathNotificationSent = false;
        AttackTarget = null;
        unitState.IsSkillActivaion = false;
        unitState.IsActionDelay = false;
        unitState.IsDamaged = false;
        unitState.IsDeath = false;
        unitState.IsSummonState = true;
        stat.AttackDelayTime = 0.0f;
        isPochiBarrier = false;
        isPochiBarrier_ForPlayer = false;
        isBleeding = false;
        isPoisoning = false;
        curGrowUp = false;
        curGrowDown = false;
        curStealth = false;
        curInvincible = false;
        curRoenSkill = false;
        isWarf = false;
        warfRange = 0.0f;
        isStun = false;
        isStunAttack = false;
        isBerserk = false;
        iSBeleifWave = false;
        isSerenBlessing = false;
        isLoneBlessing = false;
        nextStunDuring = 0.0f;
        curStun = false;
        curKnockback = false;
        skillSuperArmour = false;
        barrierReflect = 0f;
        playerBarrierReduction = 0f;
        growUpRemaining = growDownRemaining = 0f;
        normalScale = transform.localScale;


        SkillTrigger_Roni = false;
        SkillTrigger_Rose = false;
        SkillTrigger_Roen1 = false;
        SkillTrigger_Roen2 = false;
        SkillTrigger_Roen3 = false;
        SkillTrigger_Elena = false;

        SkeletonAnimationInitialize();
    }

    private void SkeletonAnimationInitialize()
    {
        SkeletonAnimation skeletonAnimation
            = transform.Find("Sprite").GetComponent<SkeletonAnimation>();
        if (skeletonAnimation != null)
            skeletonAnimation.skeleton.SetColor(Color.white);
    }
    

    void ConnectHealthBar()
    {
        HealthBarHpPoint = stat.Hp;
        if (Data.InitialMode == UnitSpawnMode.Preview) return;
        var prefab = UnitSide == eUnitSide.Enemy ? GetLunaAssetBundle().HealthBarEnemy : GetLunaAssetBundle().HealthBarAlly;
        healthBarId = LunaEffects.ShowHealthBar(prefab, ID, transform.position);
    }

    void SetPosition_Attack_Target()
    {
        InitializeProjecttileCreateLocation();
        InitializeProjecttileArrivalLocation();
    }

    private void InitializeProjecttileCreateLocation()
    {
        ProjecttileCreateLocations = new List<GameObject>();
        string[] attackPoses = { "AttackPos", "AttackPos2", "AttackPos3" };

        foreach (string attackPos in attackPoses)
        {
            Transform attackPosTr = transform.Find(attackPos);
            if (attackPosTr == null)
                continue;

            ProjecttileCreateLocations.Add(attackPosTr.gameObject);
        }

        if (ProjecttileCreateLocations.Count == 0)
        {
            ProjecttileCreateLocations.Add(gameObject);
        }
    }
    private void InitializeProjecttileArrivalLocation()
    {
        ProjecttileArrivalLocation = null;

        Transform attackPosTr = transform.Find("TargetPos");
        if (attackPosTr)
        {
            ProjecttileArrivalLocation = transform.Find("TargetPos").gameObject;
        }
        else
        {
            ProjecttileArrivalLocation = transform.Find("Sprite").gameObject;
        }
    }

    void SetAudioSource()
    {
        if (AudioSource == null)
            AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.volume = Defined.EffectVolumeSize;

        if (!LunaSession.Current.ObjectSounds.Contains(AudioSource))
        {
            LunaSession.Current.ObjectSounds.Add(AudioSource);
        }
    }

    public void PlayOneSound(AudioClip _clip)
    {
        if (AudioSource == null || _clip == null) return;
        AudioSource.PlayOneShot(_clip);
    }

    public void GetAttackSound()
    {
        if (AttackSoundType == eAttackSoundType.DEFAULT)
            return;
        else
        {
            switch (AttackSoundType)
            {
                case eAttackSoundType.SWORD:
                    AttackSoundClip = GetLunaAssetBundle().RandomSwordSound();
                    break;
                case eAttackSoundType.MACE:
                    break;
                case eAttackSoundType.ARROW:
                    AttackSoundClip = GetLunaAssetBundle().RandomArrowSound();
                    break;
                case eAttackSoundType.MAGIC:
                    break;
                case eAttackSoundType.THROW:
                    AttackSoundClip = GetLunaAssetBundle().RandomThrowSound();
                    break;
                default:
                    break;
            }
        }
    }



    #region StatusChange
    public void Heal(float healing) // ��
    {
        if (healing >= 0)
        {
            stat.Hp += healing;

            if (stat.Hp > HealthBarHpPoint)
                stat.Hp = HealthBarHpPoint;
        }
    }

    public enum eDamageType
    {
        NORMAL = 0,
        BLEED = 1,
        POISON = 2,
        REFLECT = 3,
    }

    public void Attack(GameObject striker, float damage, bool critical = false, bool isSkill = false, eDamageType damageType = eDamageType.NORMAL)
    {
        if (Data == null || IsHiding || stat.Hp <= 0f || damage <= 0f || float.IsNaN(damage) || float.IsInfinity(damage)) return;
        var attacker = From(striker);
        if (attacker != null && (attacker.Data == null || attacker.Data.RoundId != Data.RoundId)) return;
        if (striker == gameObject && !isSkill) return;
        if (damageType == eDamageType.NORMAL) damage = GetDamage(damage);
        if (UnitSide == eUnitSide.Allied) damage *= 1f - HeroPartyEffects.DamageReductionForRound(Data.RoundId);
        if (isSkill && damageType == eDamageType.NORMAL) damage *= 1f - Mathf.Clamp01(stat.SkillResistance);
        if (isPochiBarrier)
        {
            if (damageType != eDamageType.REFLECT && attacker != null && attacker != this && barrierReflect > 0f)
                attacker.Attack(gameObject, damage * barrierReflect, false, false, eDamageType.REFLECT);
            return;
        }
        if (isPochiBarrier_ForPlayer) damage *= 1f - playerBarrierReduction;
        BossDamageSetting(striker, ref damage, damageType != eDamageType.REFLECT);
        if (damageType == eDamageType.NORMAL && Defined.CanProbability(isSkill ? stat.SkillAvoidance : stat.AttackAvoidance)) return;
        if (damage <= 0f) return;
        if (!unitState.IsSuperArmour && critical) unitState.IsDamaged = true;
        stat.Hp = Mathf.Max(0f, stat.Hp - damage);
        LunaEffects.ShowDamageText(GetLunaAssetBundle().DamageText.gameObject, ID, damage, critical);
        GameObject hit = damageType == eDamageType.BLEED ? GetLunaAssetBundle().BleedEffect
            : attacker != null && attacker.AttackEffectPrefab != null && !isSkill ? attacker.AttackEffectPrefab : GetLunaAssetBundle().NormalHit;
        LunaEffects.Show(hit, ProjecttileArrivalLocation.transform.position, ID, true);
    }

    private void BossDamageSetting(GameObject striker, ref float damage, bool allowReflection)
    {
        var attacker = From(striker);
        if (attacker == null) return;
        if (allowReflection && attacker != this && Defined.CanProbability(stat.ReflectPercentForBoss))
            attacker.Attack(gameObject, damage * stat.ReflectPowerForBoss, false, false, eDamageType.REFLECT);
        if (attacker.IsBossCategory) damage *= 1f - Mathf.Clamp01(stat.ArmourPercentForBoss * 0.01f);
        if (IsBossCategory) damage *= 1f + Mathf.Max(0f, attacker.stat.AttackPercentForBoss * 0.01f);
    }

    private float GetDamage(float damage)
    {
        return Mathf.Max(1f, damage - Mathf.Max(0f, stat.Armour));
    }

    
    public void DeathRelease()
    {
        LunaEffects.Show(UnitType == eUnitType.Tower ? GetLunaAssetBundle().TowerDeath : GetLunaAssetBundle().NormalDeath, transform.position, roundId: Data.RoundId);
    }

    
    #endregion

    #region Skill


    public void GrowUp(float duration)
    {
        growUpRemaining = Mathf.Max(growUpRemaining, duration);
        curGrowUp = true;
    }
    public void GrowDownSkill(float duration)
    {
        if (!Debuff()) return;
        growDownRemaining = Mathf.Max(growDownRemaining, duration);
        curGrowDown = true;
    }
    private void UpdateGrowth(float elapsed)
    {
        if (!curGrowUp && !curGrowDown) return;
        growUpRemaining = Mathf.Max(0f, growUpRemaining - elapsed);
        growDownRemaining = Mathf.Max(0f, growDownRemaining - elapsed);
        float scale = (growUpRemaining > 0f ? 1.5f : 1f) * (growDownRemaining > 0f ? 0.65f : 1f);
        transform.localScale = Vector3.MoveTowards(transform.localScale, normalScale * scale, elapsed * normalScale.magnitude);
        if (growUpRemaining == 0f && growDownRemaining == 0f && transform.localScale == normalScale)
            curGrowUp = curGrowDown = false;
    }

    public void StartWarf(float range)
    {
        isWarf = true;
        warfRange = objectData.AttackRange * 1.5f;

        if (stat.AttackRange < range)
        {
            stat.AttackRange = range;
        }
    }

    public void EndWarf()
    {
        isWarf = false;
        warfRange = 0.0f;
        stat.AttackRange = objectData.AttackRange;
        if (this is Clare && ultState >= eUltState.STAR5) StartWarf(3f);
    }


    public void TakePochiBarrier(float duration, float reflectFraction = 0f)
    {
        barrierReflect = Mathf.Max(barrierReflect, Mathf.Clamp01(reflectFraction));
        if (isPochiBarrier) return;
        isPochiBarrier = true;
        StartCoroutine(Barrier(duration, false));
    }
    public void TakePochiBarrier_ForPlayer(float duration, float reduction = 0.5f)
    {
        if (isPochiBarrier_ForPlayer) return;
        isPochiBarrier_ForPlayer = true;
        playerBarrierReduction = Mathf.Clamp01(reduction);
        StartCoroutine(Barrier(duration, true));
    }
    private IEnumerator Barrier(float duration, bool partial)
    {
        var data = EffectData.Create(ProjecttileArrivalLocation.transform.position, ID, true, Data.RoundId);
        data.Animation = "normal"; data.Loop = true; data.Lifetime = duration;
        int effectId = LunaSpawn.Show<EffectLogic>(GetLunaAssetBundle().pochiBarrierEffect, data);
        yield return new WaitForSeconds(duration);
        LunaSpawn.Cancel(effectId);
        if (partial) { isPochiBarrier_ForPlayer = false; playerBarrierReduction = 0f; }
        else { isPochiBarrier = false; barrierReflect = 0f; }
    }

    public void TakeBleeding(float tickDamage, float duration)
    {
        if (!isBleeding && Debuff())
        {
            isBleeding = true;
            StartCoroutine(Bleeding_Proceed(tickDamage, (int)duration));
        }
    }

    IEnumerator Bleeding_Proceed(float tickDamage, int tick)
    {
        for (int i = 0; i < tick; i++)
        {
            yield return new WaitForSeconds(1.0f);
            Attack(gameObject, tickDamage, false, true, eDamageType.BLEED);
        }

        isBleeding = false;
    }


    public void TakePoisoning(float tickDamage, float duration)
    {
        if (!isPoisoning && Debuff())
        {
            isPoisoning = true;
            StartCoroutine(Poisoning_Proceed(tickDamage, (int)duration));
        }
    }

    IEnumerator Poisoning_Proceed(float tickDamage, int tick)
    {
        for (int i = 0; i < tick; i++)
        {
            yield return new WaitForSeconds(1.0f);
            Attack(gameObject, tickDamage, false, true, eDamageType.POISON);
        }

        isPoisoning = false;
    }


    public void TakeBerserkMode(float duration, float extraStatBonus = 0f)
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (isBerserk == false)
        {
            isBerserk = true;
            BuffImp.AddBuff(eBuffType.ATKSPEED, duration);
            BuffImp.AddBuff(eBuffType.MOVESPEED, duration);
            float extra = Mathf.Max(0f, extraStatBonus);
            StartCoroutine(Berserk_Proceed(duration, 0.5f + extra, 1f + extra));
        }
    }

    IEnumerator Berserk_Proceed(float duration, float attackSpeedBonus, float moveSpeedBonus)
    {
        int effect = LunaEffects.Show(GetLunaAssetBundle().BerserkEffect, transform.position, ID, true);
        coefficient.AttackSpeed += attackSpeedBonus;
        coefficient.MoveSpeed += moveSpeedBonus;
        RefreshSuperArmour(); RefreshTint(); StatusAfterReady();
        yield return new WaitForSeconds(duration);
        coefficient.AttackSpeed -= attackSpeedBonus;
        coefficient.MoveSpeed -= moveSpeedBonus;
        isBerserk = false;
        RefreshSuperArmour(); RefreshTint(); StatusAfterReady();
        LunaSpawn.Cancel(effect);
    }

    public void SetSkillSuperArmour(bool value) { skillSuperArmour = value; RefreshSuperArmour(); }
    private void RefreshSuperArmour() { unitState.IsSuperArmour = objectData.TypeSuperArmour || isBerserk || skillSuperArmour; }
    private void RefreshTint()
    {
        var sprite = transform.Find("Sprite");
        var ani = sprite != null ? sprite.GetComponent<SkeletonAnimation>() : null;
        if (ani == null || ani.Skeleton == null) return;
        Color color = isBerserk ? Color.black : (iSBeleifWave || isSerenBlessing || isLoneBlessing ? Color.gray : Color.white);
        if (curStealth) color.a = 0.5f;
        ani.Skeleton.SetColor(color);
    }



    public void TakeBeleifWaveMode(float duration, float attackBonus = 0.15f)
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (iSBeleifWave == false)
        {
            iSBeleifWave = true;
            BuffImp.AddBuff(eBuffType.ATTACK, duration);
            StartCoroutine(BeleifWave_Proceed(duration, Mathf.Max(0f, attackBonus)));
        }
    }
    IEnumerator BeleifWave_Proceed(float duration, float attackBonus)
    {
        int tmpEffect = LunaEffects.Show(GetLunaAssetBundle().beleifWaveEffect, transform.position, ID, true);

        RefreshTint();
        coefficient.AttackDamage += attackBonus;

        StatusAfterReady();


        yield return new WaitForSeconds(duration);
        {
            iSBeleifWave = false;
            coefficient.AttackDamage -= attackBonus;
            RefreshTint();

            StatusAfterReady();
        }

        LunaSpawn.Cancel(tmpEffect);
        iSBeleifWave = false;
    }


    public void TakeSerenBlessingMode(float duration, float avoidanceBonus = 25f)
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (isSerenBlessing == false)
        {
            isSerenBlessing = true;
            StartCoroutine(SerenBlessing_Proceed(duration, Mathf.Max(0f, avoidanceBonus)));
        }
    }
    IEnumerator SerenBlessing_Proceed(float duration, float avoidanceBonus)
    {
        int tmpEffect = LunaEffects.Show(GetLunaAssetBundle().serenBlessing, transform.position, ID, true);

        RefreshTint();

        stat.AttackAvoidance += avoidanceBonus;
        stat.SkillAvoidance += avoidanceBonus;
        stat.DebuffAvoidance += avoidanceBonus;
        StatusAfterReady();


        yield return new WaitForSeconds(duration);
        {
            isSerenBlessing = false;
            RefreshTint();
            stat.AttackAvoidance -= avoidanceBonus;
            stat.SkillAvoidance -= avoidanceBonus;
            stat.DebuffAvoidance -= avoidanceBonus;

            StatusAfterReady();
        }

        LunaSpawn.Cancel(tmpEffect);
        isSerenBlessing = false;
    }



    public void TakeLoneBlessingMode(float duration, float armourBonus = 1f)
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (isLoneBlessing == false)
        {
            isLoneBlessing = true;
            BuffImp.AddBuff(eBuffType.DEFENSE, duration);
            StartCoroutine(LoneBlessing_Proceed(duration, Mathf.Max(0f, armourBonus)));
        }
    }
    IEnumerator LoneBlessing_Proceed(float duration, float armourBonus)
    {
        int tmpEffect = LunaEffects.Show(GetLunaAssetBundle().loneBlessing, transform.position, ID, true);

        RefreshTint();

        coefficient.Armour += armourBonus;
        StatusAfterReady();


        yield return new WaitForSeconds(duration);
        {
            isLoneBlessing = false;
            RefreshTint();
            coefficient.Armour -= armourBonus;
            StatusAfterReady();
        }

        LunaSpawn.Cancel(tmpEffect);
        isLoneBlessing = false;
    }





    public void TakeStun(float duration)
    {
        if (unitState.IsDeath == true)
            return;

        if (hitPriority == eHitPriority.TOWER)
            return;

        if (!curStun && Debuff() && !Defined.CanProbability(stat.StunResistance))
        {
            curStun = true;
            StartCoroutine(Stun_Proceed(duration));
        }
    }

    IEnumerator Stun_Proceed(float duration)
    {
        int tmpEffect = LunaEffects.Show(GetLunaAssetBundle().StunEffect, transform.position, ID, true);

        float tmpSpeed = stat.MoveSpeed;
        {
            isStun = true;
        }

        yield return new WaitForSeconds(duration);
        {
            isStun = false;
        }

        LunaSpawn.Cancel(tmpEffect);
        curStun = false;
    }

    //// stun, KnockBack


    public void TakeNuckbackMovement()
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (curKnockback == false)
        {
            curKnockback = true;
            StartCoroutine(Knuckback_Proceed());
        }
    }

    IEnumerator Knuckback_Proceed()
    {
        float groundY = transform.position.y;
        float horizontal = UnitSide == eUnitSide.Enemy ? 0.3f : -0.3f;
        float velocity = 3f;
        do
        {
            velocity -= 4.5f * Time.deltaTime;
            LunaCombat.Move(this, new Vector3(horizontal, velocity, 0f) * Time.deltaTime);
            yield return null;
        } while (transform.position.y > groundY && !unitState.IsDeath);
        transform.position = LunaCombat.ClampPosition(this, new Vector3(transform.position.x, groundY, transform.position.z));
        curKnockback = false;
    }

    /////////////////////////// Stealth , invincible


    public void Stealth(float duration)
    {
        if (curStealth == false)
        {
            curStealth = true;
            StartCoroutine(Stealth_Proceed(duration));
        }
    }

    IEnumerator Stealth_Proceed(float duration)
    {
        stat.AttackAvoidance += 100f; stat.SkillAvoidance += 100f; stat.DebuffAvoidance += 100f;
        RefreshTint();
        yield return new WaitForSeconds(duration);
        stat.AttackAvoidance -= 100f; stat.SkillAvoidance -= 100f; stat.DebuffAvoidance -= 100f;
        curStealth = false;
        RefreshTint();
    }


    public void Invincible(float duration)
    {
        if (curInvincible == false)
        {
            curInvincible = true;
            StartCoroutine(Invincible_Proceed(duration));
        }
    }

    IEnumerator Invincible_Proceed(float duration)
    {
        stat.AttackAvoidance += 100f; stat.SkillAvoidance += 100f; stat.DebuffAvoidance += 100f;
        if (duration <= 0f) yield break; // Permanent for this spawn; reset by Ready on reuse.
        yield return new WaitForSeconds(duration);
        stat.AttackAvoidance -= 100f; stat.SkillAvoidance -= 100f; stat.DebuffAvoidance -= 100f;
        curInvincible = false;
    }



    public void RoenSkill(float duration)
    {
        if (curRoenSkill == false)
        {
            curRoenSkill = true;
            BuffImp.AddBuff(eBuffType.ATKSPEED, duration);
            StartCoroutine(RoenSkill_Proceed(duration));
        }
    }

    IEnumerator RoenSkill_Proceed(float duration)
    {
        {
            coefficient.AttackSpeed += 0.4f;
            StatusAfterReady();
        }
        yield return new WaitForSeconds(duration);
        {
            coefficient.AttackSpeed -= 0.4f;
            StatusAfterReady();
        }

        curRoenSkill = false;
    }
    /////////////////////////// trigger <-리팩토링필수



    /////////////////////////// Debuff



    public void Curse()
    {
        if (!Debuff()) return;
        curseSeverity = Mathf.Min(80f, curseSeverity + 20f);
        StatusAfterReady();
    }
    public void CleanseCurse(float percentagePoints)
    {
        curseSeverity = Mathf.Max(0f, curseSeverity - Mathf.Max(0f, percentagePoints));
        StatusAfterReady();
    }


    public bool Debuff()
    {
        return gameObject.activeInHierarchy && !unitState.IsDeath && stat.Hp > 0f && !Defined.CanProbability(stat.DebuffAvoidance);
    }


    ///////////////////////////

    public void MultipleAttackDamage(float multiple)
    {
        stat.AttackDamage *= multiple;
    }
    //public void MultipleSkillDamage(float multiple)
    //{
    //    stat.SkillDamage *= multiple;
    //}

    public void MultipleArmour(float multiple)
    {
        stat.Armour *= multiple;
    }
    public void MultipleAttackSpeed(float multiple)
    {
        stat.AttackSpeed *= multiple;
    }

    #endregion

    public void ConfirmSkillTriger()
    {
        bool active = LunaCombat.IsInCombat(this);
        bool roni = active && HeroPartyEffects.Has<Roni>(eUltState.ULT2, Data.RoundId);
        bool rose = active && HeroPartyEffects.Has<Rose>(eUltState.ULT2, Data.RoundId);
        bool roen1 = active && HeroPartyEffects.Has<Roen>(eUltState.STAR5, Data.RoundId);
        bool roen2 = active && HeroPartyEffects.Has<Roen>(eUltState.ULT1, Data.RoundId);
        bool roen3 = active && HeroPartyEffects.Has<Roen>(eUltState.ULT2, Data.RoundId);
        bool elena = active && HeroPartyEffects.Has<Elena>(eUltState.ULT1, Data.RoundId);
        if (SkillTrigger_Roni == false && roni == true)
        {
            SkillTrigger_Roni = true;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed)
            {
                coefficient.HP += 0.1f;
                HpAfterReady();
            }
        }
        else if (SkillTrigger_Roni == true && roni == false)
        {
            SkillTrigger_Roni = false;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed)
            {
                coefficient.HP -= 0.1f;
                HpAfterReady();
            }
        }

        if (SkillTrigger_Rose == false && rose == true)
        {
            SkillTrigger_Rose = true;

            if (UnitSide == eUnitSide.Enemy && objectData.IsAirborne)
            {
                coefficient.AttackDamage -= 0.1f;
                StatusAfterReady();
            }
        }
        else if (SkillTrigger_Rose == true && rose == false)
        {
            SkillTrigger_Rose = false;

            if (UnitSide == eUnitSide.Enemy && objectData.IsAirborne)
            {
                coefficient.AttackDamage += 0.1f;
                StatusAfterReady();
            }
        }

        if (SkillTrigger_Roen1 == false && roen1 == true)
        {
            SkillTrigger_Roen1 = true;

            if (UnitSide == eUnitSide.Allied && UnitType == eUnitType.Hero && objectData.IsOffensiveHero)
            {
                coefficient.AttackSpeed += 0.15f;
                StatusAfterReady();
            }
        }
        else if (SkillTrigger_Roen1 == true && roen1 == false)
        {
            SkillTrigger_Roen1 = false;

            if (UnitSide == eUnitSide.Allied && UnitType == eUnitType.Hero && objectData.IsOffensiveHero)
            {
                coefficient.AttackSpeed -= 0.15f;
                StatusAfterReady();
            }
        }

        if (SkillTrigger_Roen2 == false && roen2 == true)
        {
            SkillTrigger_Roen2 = true;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed)
            {
                stat.AttackAvoidance += 5.0f; //무슨회피율?
            }
        }
        else if (SkillTrigger_Roen2 == true && roen2 == false)
        {
            SkillTrigger_Roen2 = false;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed)
            {
                stat.AttackAvoidance -= 5.0f; //무슨회피율?
            }
        }

        if (SkillTrigger_Roen3 == false && roen3 == true)
        {
            SkillTrigger_Roen3 = true;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed && hitPriority != eHitPriority.TOWER)
            {
                StartWarf(4.0f);
            }
        }
        else if (SkillTrigger_Roen3 == true && roen3 == false)
        {
            SkillTrigger_Roen3 = false;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed && hitPriority != eHitPriority.TOWER)
            {
                EndWarf();
            }
        }

        if (SkillTrigger_Elena == false && elena == true)
        {
            SkillTrigger_Elena = true;

            if (UnitSide == eUnitSide.Allied)
            {
                coefficient.Armour += 0.08f;
                StatusAfterReady();
            }
        }
        else if (SkillTrigger_Elena == true && elena == false)
        {
            SkillTrigger_Elena = false;

            if (UnitSide == eUnitSide.Allied)
            {
                coefficient.Armour -= 0.08f;
                StatusAfterReady();
            }
        }
    }

    public void HpAfterReady()
    {
        float tmpHp = HealthBarHpPoint > 0f ? Mathf.Clamp01(stat.Hp / HealthBarHpPoint) : 1f;

        HealthBarHpPoint = Mathf.Max(1f, (objectData.Hp + (Level - 1) * objectData.LevelUpHpPoint) * coefficient.HP);
        stat.Hp = tmpHp * HealthBarHpPoint;
    }

    private float MoveSpeedFromSpawnData()
    {
        float multiplier = coefficient.MoveSpeed;
        if (float.IsNaN(multiplier) || float.IsInfinity(multiplier)) multiplier = 1f;
        return Data.BaseMoveSpeed * Mathf.Max(0f, multiplier) * (UnitSide == eUnitSide.Enemy ? -1f : 1f);
    }

    public void StatusAfterReady()
    {
        // status afterupdate
        stat.Stamina = Mathf.Max(0f, objectData.Stamina * coefficient.Stamina);
        stat.AttackSpeed = objectData.GetAttackOnceBySecond_Value(coefficient.AttackSpeed * equipmentSpeedMultiplier);
        stat.MoveSpeed = MoveSpeedFromSpawnData() * (1f - curseSeverity / 100f);
        stat.Armour = (objectData.Armour + (Level - 1) * objectData.LevelUpArmourPoint) * coefficient.Armour * (1f - curseSeverity / 100f);
        stat.AttackDamage = (Data.BaseAttackDamageOverride ?? (objectData.AttackDamage + (Level - 1) * objectData.LevelUpAttPoint)) * coefficient.AttackDamage * equipmentAttackMultiplier;
    }

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        View = GetComponent<UnitView>();
        if (View == null) throw new System.InvalidOperationException("Unit entity has no authored UnitView.");
        Animation = GetComponentInChildren<SkeletonAnimation>(true);
        if (Animation != null)
        {
            Animation.AnimationState.Event += HandleAnimationEvent;
            Animation.AnimationState.Complete += HandleAnimationComplete;
        }
    }
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        Data = userData as UnitData;
        if (Data == null || Data.Definition == null) throw new System.InvalidOperationException("UnitLogic requires UnitData.");
        AttackEffectPrefab = View.AttackEffectPrefab; AttackSoundClip = View.AttackSoundClip;
        AttackSoundType = (eAttackSoundType)View.AttackSoundType;
        int requestedTargetId = Data.TargetId;
        InitializeSpawnState();
        Data.TargetId = requestedTargetId;
        unitState.IsLobbyMove = Data.InitialMode != UnitSpawnMode.Battle;
        remainingLifetime = Data.Lifetime;
        ResetAnimation();
        ConfigureUnitForSpawn();
        if (Data.InvincibleSeconds != 0f) Invincible(Mathf.Max(0f, Data.InvincibleSeconds));
        if (Data.BerserkSeconds > 0f) TakeBerserkMode(Data.BerserkSeconds);
        activeUnits.Add(this);
        if (Data.InitialMode != UnitSpawnMode.Preview) CreateCombatFsm();
    }
    protected virtual void ConfigureUnitForSpawn() { }
    protected virtual void OnUnitHidden() { }
    private float remainingLifetime;
    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        if (Data == null || IsHiding) return;
        if (remainingLifetime > 0f && (remainingLifetime -= elapsed) <= 0f) { SafeHide(); return; }
        ConfirmSkillTriger(); UpdateGrowth(elapsed); BuffImp.Tick(elapsed);
        UpdatePlayerAura();
        if (!unitState.IsLobbyMove && stat.Hp > 0f)
        {
            if (!unitState.IsSkillActivaion && stat.Stamina > 0f) stat.Mp = Mathf.Min(100f, stat.Mp + elapsed * 100f / stat.Stamina);
            if (unitState.IsActionDelay && !isStun)
            {
                stat.AttackDelayTime += elapsed;
                if (stat.AttackDelayTime >= 0f) unitState.IsActionDelay = false;
            }
            if (UnitSide == eUnitSide.Enemy && Target != null)
            {
                float distance = transform.position.x - Target.transform.position.x;
                if (distance > 0f && distance < 0.5f) LunaCombat.Move(this, Vector3.right * elapsed);
            }
        }
    }
    private void UpdatePlayerAura()
    {
        playerAuraMoveBonus = 0f;
        var playerObject = LunaSession.Current.LunaPlayer;
        var player = From(playerObject);
        bool eligible = UnitSide == eUnitSide.Allied && (UnitType == eUnitType.Hero || UnitType == eUnitType.Soldier)
            && LunaCombat.IsInCombat(this) && LunaCombat.IsAlive(playerObject)
            && player.Data.RoundId == Data.RoundId;
        int auraLevel = eligible ? player.Data.AuraLevel : 0;
        float range = 2f + Defined.AddAuroraRangeValue * auraLevel;
        if (eligible && Vector3.Distance(transform.position, playerObject.transform.position) < range)
        {
            playerAuraMoveBonus = Mathf.Max(0, auraLevel - 1) * 0.15f;
            if (playerAuraEffectId == 0 && GetLunaAssetBundle().MiniPlayerEffectForUnit != null)
            {
                var prefab = GetLunaAssetBundle().MiniPlayerEffectForUnit;
                var effect = EffectData.Create(transform.position, ID, true, Data.RoundId);
                effect.Loop = true; effect.Lifetime = 0f;
                playerAuraEffectId = LunaSpawn.Show<EffectLogic>(prefab, effect);
            }
        }
        else if (playerAuraEffectId != 0)
        {
            LunaSpawn.Cancel(playerAuraEffectId); playerAuraEffectId = 0;
        }
    }
    protected override void OnHide(bool isShutdown, object userData)
    {
        try
        {
            DestroyCombatFsm();
            StopAllCoroutines();
            OnUnitHidden();
        }
        finally
        {
            activeUnits.Remove(this);
            try { ReleaseData(); }
            finally { Data = null; base.OnHide(isShutdown, userData); }
        }
    }
    protected override void OnRecycle()
    {
        try { DestroyCombatFsm(); }
        finally
        {
            StopAllCoroutines();
            activeUnits.Remove(this);
            try { ReleaseData(); }
            finally { Data = null; base.OnRecycle(); }
        }
    }
}
}
