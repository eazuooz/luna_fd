using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using EverydayDevup.Framework;

public class Status : MonoBehaviour
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

    public bool IsStatusDataLoadComplete;
    public ObjectData objectData;
    public AudioSource AudioSource;
    public AudioClip AttackSoundClip;
    public GameObject AttackEffectPrefab;
    private GameObject healthBarGameObject;

    public eAttackSoundType AttackSoundType = eAttackSoundType.DEFAULT;

    [HideInInspector] public string UnitName;
    [HideInInspector] public int Level;
    [HideInInspector] public eHitPriority hitPriority;
    [HideInInspector] public eUltState ultState;

    [HideInInspector] public eUnitSide UnitSide;
    [HideInInspector] public eUnitType UnitType;
    [HideInInspector] public eUnitAttackType UnitAttackType;
    [HideInInspector] public Coefficient coefficient = new Coefficient();
    [HideInInspector] public Stat stat = new Stat();
    [HideInInspector] public UnitState unitState = new UnitState();

    [HideInInspector] public List<GameObject> ProjecttileCreateLocations;
    [HideInInspector] public GameObject ProjecttileArrivalLocation;
    [HideInInspector] public GameObject AttackTarget;
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



    private BuffImp buffImp = new BuffImp();
    public BuffImp BuffImp
    {
        get { return buffImp; }
    }


    public void BuffIconInit()
    {
        BuffImp.UnitGameObject = gameObject;
        BuffImp.BuffIconInit();
        StartCoroutine(BuffImp.UpdateBuffs());
    }

    public void Ready()
    {
        ResetRespwanData();
    }
    public void ReleaseData()
    {
        if (AudioSource != null)
        {
            if (Game.Instance != null
                && Game.Instance.dataManager != null
                && Game.Instance.dataManager.ObjectSounds != null
                && Game.Instance.dataManager.ObjectSounds.Contains(AudioSource) == true)
                Game.Instance.dataManager.ObjectSounds.Remove(AudioSource);
        }
        BuffImp.BuffIconOff();
    }
    public void ResetRespwanData()
    {
        InitializeCoefficentValues();
        LoadObjectData();
        ConnectHealthBar();
        SetPosition_Attack_Target();
        SetAudioSource();
        InitializeStatusCondition();
        BuffIconInit();

        IsStatusDataLoadComplete = true;
    }
    void LoadObjectData() // 스크립터블오브젝트 내부 스테이터스 로드
    {
        if (objectData != null)
        {
            InitalizeLunaAssetBundle();
            InitializeUnitData();
            InitializeTypeValues();
            InitializeUnitStats();

            DoUnitStatsBuff();
        }
    }
    private void InitalizeLunaAssetBundle()
    {
        lunaAssetBundle = Resources.Load<GameObject>("LunaAssetBundle");
    }
    private void InitializeUnitData()
    {
        hitPriority = objectData.hitPriority;
        ultState = objectData.ultState;
        UnitName = objectData.UnitName;
        Level = objectData.Level;
    }
    private void InitializeTypeValues()
    {
        UnitSide = objectData.UnitSide;
        UnitType = objectData.UnitType;
        UnitAttackType = objectData.UnitAttackType;
    }
    void InitializeCoefficentValues() // 스테이터스 계수 초기화
    {
        coefficient.HP = 1.0f;
        coefficient.MP = 1.0f;
        coefficient.Stamina = 1.0f;
        coefficient.AttackDamage = 1.0f;
        coefficient.SkillPoint = 1.0f;
        coefficient.Armour = 1.0f;
        coefficient.AttackSpeed = 1.0f;
        coefficient.MoveSpeed = 1.0f;

        stat.AttackAvoidance = 0.0f;
        stat.SkillAvoidance = 0.0f;
        stat.DebuffAvoidance = 0.0f;
    }
    void InitializeUnitStats()
    {
        stat.Hp = objectData.GetCurrentLevelHP_Value(coefficient.HP);
        stat.Mp = objectData.GetCurrentLevelMP_Value(coefficient.MP);
        stat.Stamina = objectData.GetCurrentLevelStamina_Value(coefficient.Stamina);
        stat.AttackSpeed = objectData.GetAttackOnceBySecond_Value(coefficient.AttackSpeed);
        stat.MoveSpeed = objectData.GetMoveSpeed_Value(coefficient.MoveSpeed);
        stat.Armour = objectData.GetArmour_Value(coefficient.Armour);
        stat.AttackDamage = objectData.GetAttackDamage_Value(coefficient.AttackDamage);
        stat.AttackRange = objectData.GetAttackRange_Value();

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
    private void DoUnitStatsBuff()
    {

    }

    void ConnectHealthBar()
    {
        if (healthBarGameObject != null)
        {
            healthBarGameObject.GetComponent<HealthBarControl>().Chaser = null;
            healthBarGameObject = null;
        }

        Transform tmpTransform = transform.Find("Canvas").transform;
        RectTransform tmpRTransform = transform.Find("Canvas").GetComponent<RectTransform>();

        if (UnitSide != eUnitSide.Enemy)
        {
            healthBarGameObject = GameObject.Instantiate(GetLunaAssetBundle().HealthBarAlly, tmpTransform.position + new Vector3(0, tmpRTransform.rect.height / 2, 0), Quaternion.identity);
        }
        else
        {
            healthBarGameObject = GameObject.Instantiate(GetLunaAssetBundle().HealthBarEnemy, tmpTransform.position + new Vector3(0, tmpRTransform.rect.height / 2, 0), Quaternion.identity);
        }

        healthBarGameObject.GetComponent<HealthBarControl>().InitHealthBar(gameObject);

        HealthBarHpPoint = stat.Hp;
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
        AudioSource = gameObject.AddComponent<AudioSource>();
        AudioSource.volume = Defined.EffectVolumeSize;

        if (AudioSource != null)
        {
            Game.Instance.dataManager.ObjectSounds.Add(AudioSource);
        }
    }

    public void PlayOneSound(AudioClip _clip)
    {
        AudioSource.clip = _clip;
        AudioSource.Play();
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
    }

    public void Attack(GameObject striker, float damage, bool critical = false, bool isSkill = false, eDamageType damageType = eDamageType.NORMAL)
    {
        if (striker == gameObject && isSkill == false)
            return;

        if (striker == null)
            return;

        //damage = GetDamage(damage);

        if (isPochiBarrier == true)
        {
            return;
        }
        else if (isPochiBarrier_ForPlayer == true)
        {
            damage /= 2;
        }

        BossDamageSetting(striker, ref damage);

        if (damage > 0)
        {
            RectTransform tmpRTransform = transform.Find("Canvas").GetComponent<RectTransform>();
            Text damageEffect = GameObject.Instantiate(GetLunaAssetBundle().DamageText, transform.position + new Vector3(0, tmpRTransform.rect.height, 0), GetLunaAssetBundle().DamageText.transform.rotation);

            if (Defined.CanProbability(stat.AttackAvoidance) && isSkill == false)
            {
                //miss
                damageEffect.text = "<color=#808080>" + "MISS" + "</color>";
            }
            else if (Defined.CanProbability(stat.SkillAvoidance) && isSkill == true)
            {
                //miss
                damageEffect.text = "<color=#808080>" + "MISS" + "</color>";
            }
            else
            {
                if (unitState.IsSuperArmour == false && critical == true)
                {
                    unitState.IsDamaged = true;
                }

                stat.Hp -= damage;

                if (healthBarGameObject != null)
                    healthBarGameObject.GetComponent<HealthBarControl>().Hit();

                GameObject tmpHitEffect;

                if (striker.GetComponent<Status>() == null || striker.GetComponent<Status>().AttackEffectPrefab == null || isSkill == true)
                {
                    tmpHitEffect = Defined.EffectCreate(GetLunaAssetBundle().NormalHit, transform.position + new Vector3(0, tmpRTransform.rect.height, 0));
                }
                else
                {
                    tmpHitEffect = Defined.EffectCreate(striker.GetComponent<Status>().AttackEffectPrefab, gameObject, Defined.eEffectPosition.TARGET);
                }

                if (damageType == eDamageType.BLEED) // bleed
                {
                    GameObject.Instantiate(GetLunaAssetBundle().BleedEffect, ProjecttileArrivalLocation.transform.position, GetLunaAssetBundle().BleedEffect.transform.rotation);

                    damageEffect.text = "<color=#8B0000>" + damage.ToString() + "</color>";
                }
                else if (damageType == eDamageType.POISON) // poison
                {
                    damageEffect.text = "<color=#4b0082>" + damage.ToString() + "</color>";
                }
                else // normal
                {
                    GameObject tmpBlood = Defined.EffectCreate(GetLunaAssetBundle().AllyAttacked, gameObject, Defined.eEffectPosition.TARGET);
                    if (UnitSide == eUnitSide.Enemy)
                    {
                        Vector3 tmpScale = new Vector3(tmpBlood.transform.localScale.x * -1, tmpBlood.transform.localScale.y, tmpBlood.transform.localScale.z);
                        tmpBlood.transform.localScale = tmpScale;
                    }

                    if (isSkill == true)
                    {
                        damageEffect.text = "<color=#7b68ee>" + damage.ToString() + "</color>";
                    }
                    else if (critical == true)
                    {
                        damageEffect.text = "<color=#ff8c00>" + damage.ToString() + "</color>";
                    }
                    else
                    {
                        damageEffect.text = "<color=#fdffff>" + damage.ToString() + "</color>";
                    }
                }
            }
            damageEffect.transform.SetParent(transform.Find("Canvas"));
        }
    }

    private void BossDamageSetting(GameObject striker, ref float damage)
    {
        if (striker.GetComponent<Status>() == null)
        {
            return;
        }

        if (Defined.CanProbability(stat.ReflectPercentForBoss))
        {
            striker.GetComponent<Status>().Attack(gameObject, damage * stat.ReflectPowerForBoss);
        }
        if (stat.ArmourPercentForBoss > 0.0f && striker.GetComponent<Status>().UnitType == eUnitType.Boss)
        {
            damage *= (1.0f - (stat.ArmourPercentForBoss * 0.01f));
        }
        if (stat.AttackPercentForBoss > 0.0f && UnitType == eUnitType.Boss)
        {
            damage *= (1.0f + (stat.ArmourPercentForBoss * 0.01f));
        }
    }

    private float GetDamage(float damage)
    {
        return damage - stat.Armour;
    }

    private void ReflectPercentForBoss()
    {

    }
    public void DeathRelease()
    {
        if (UnitType == eUnitType.Tower)
            Defined.EffectCreate(GetLunaAssetBundle().TowerDeath, transform.position);
        else
            Defined.EffectCreate(GetLunaAssetBundle().NormalDeath, transform.position);

        if (UnitType == eUnitType.Tower)
        {
            MonsterDrop(transform.position);
        }
    }

    void MonsterDrop(Vector3 pos)
    {
        if (Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>() != null)
        {
            Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>().DropItemMonster(pos);
        }
    }
    #endregion

    #region Skill


    public void GrowUp(float duration)
    {
        if (curGrowUp == false)
        {
            curGrowUp = true;
            StartCoroutine(GrowUpProceed(duration));
        }
    }

    IEnumerator GrowUpProceed(float duration)
    {
        Vector3 beforeScale = transform.localScale;

        float originScale = 1.0f;
        float destScale = 1.5f;

        StartCoroutine(TransformScaleUp(beforeScale, originScale, destScale));

        yield return new WaitForSeconds(duration);

        StartCoroutine(TransformSacaleDown(beforeScale, destScale, originScale));

        curGrowUp = false;
    }



    public void GrowDownSkill(float duration)
    {
        if (curGrowDown == false)
        {
            curGrowDown = true;
            StartCoroutine(GrowDownProceed(duration));
        }
    }

    IEnumerator GrowDownProceed(float duration)
    {
        Vector3 beforeScale = transform.localScale;

        float originScale = 1.0f;
        float destScale = 0.65f;

        StartCoroutine(TransformSacaleDown(beforeScale, originScale, destScale));

        yield return new WaitForSeconds(duration);

        StartCoroutine(TransformScaleUp(beforeScale, destScale, originScale));

        curGrowDown = false;
    }

    IEnumerator TransformScaleUp(Vector3 beforeScale, float tmpScale, float destScale)
    {
        while (tmpScale < destScale)
        {
            //InfiniteLoopDetector.Run();
            tmpScale += Time.deltaTime;
            transform.localScale = beforeScale * tmpScale;

            if (tmpScale >= destScale)
                tmpScale = destScale;

            yield return null;
        }
    }
    IEnumerator TransformSacaleDown(Vector3 beforeScale, float tmpScale, float destScale)
    {
        while (tmpScale > destScale)
        {
            //InfiniteLoopDetector.Run();
            tmpScale -= Time.deltaTime;
            transform.localScale = beforeScale * tmpScale;

            if (tmpScale <= destScale)
                tmpScale = destScale;

            yield return null;
        }
    }

    ///////////////////////////

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
        stat.AttackRange = objectData.AttackRange * Random.Range(0.8f, 1.2f);
    }


    public void TakePochiBarrier(float duration)
    {
        if (isPochiBarrier == false)
        {
            GameObject effect = Defined.EffectCreate(GetLunaAssetBundle().pochiBarrierEffect, gameObject, Defined.eEffectPosition.TARGET, true);

            SkeletonAnimation effectAni = effect.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

            effectAni.state.Complete += delegate (Spine.TrackEntry trackEntry)
            {
                if (trackEntry.Animation.Name == "show")
                {
                    effectAni.state.SetAnimation(0, "normal", true);
                }

                if (trackEntry.Animation.Name == "hide")
                {
                    Destroy(effect);
                }
            };

            StartCoroutine(PochiBarrier_Proceed(duration, effectAni));
        }
    }

    IEnumerator PochiBarrier_Proceed(float duration, SkeletonAnimation Ani)
    {
        Ani.state.SetAnimation(0, "show", false);
        isPochiBarrier = true;

        yield return new WaitForSeconds(duration);

        Ani.state.SetAnimation(0, "hide", false);
        isPochiBarrier = false;
    }


    public void TakePochiBarrier_ForPlayer(float duration)
    {
        if (isPochiBarrier_ForPlayer == false)
        {
            GameObject effect
                = Defined.EffectCreate(GetLunaAssetBundle().pochiBarrierEffect, gameObject, Defined.eEffectPosition.TARGET, true);

            SkeletonAnimation effectAni = effect.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

            effectAni.state.Complete += delegate (Spine.TrackEntry trackEntry)
            {
                if (trackEntry.Animation.Name == "show")
                {
                    effectAni.state.SetAnimation(0, "normal", true);
                }

                if (trackEntry.Animation.Name == "hide")
                {
                    Destroy(effect);
                }
            };

            StartCoroutine(PochiBarrier_Proceed_ForPlayer(duration, effectAni));
        }
    }

    IEnumerator PochiBarrier_Proceed_ForPlayer(float duration, SkeletonAnimation Ani)
    {
        Ani.state.SetAnimation(0, "show", false);
        isPochiBarrier_ForPlayer = true;

        yield return new WaitForSeconds(duration);

        Ani.state.SetAnimation(0, "hide", false);
        isPochiBarrier_ForPlayer = false;
    }



    public void TakeBleeding(float tickDamage, float duration)
    {
        if (isBleeding == false)
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
        if (isPoisoning == false)
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


    public void TakeBerserkMode(float duration)
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (isBerserk == false)
        {
            isBerserk = true;
            StartCoroutine(Berserk_Proceed(duration));

            BuffImp.AddBuff(eBuffType.ATTACK, duration);
            BuffImp.AddBuff(eBuffType.ATKSPEED, duration);
            BuffImp.AddBuff(eBuffType.DEFENSE, duration);
        }
    }

    IEnumerator Berserk_Proceed(float duration)
    {
        //GameObject tmpEffect 
        //    = GameObject.Instantiate(GetLunaAssetBundle().BerserkEffect, 
        //    ProjecttileArrivalLocation.transform.position, 
        //    GetLunaAssetBundle().BerserkEffect.transform.rotation);

        //tmpEffect.transform.SetParent(transform);

        GameObject tmpEffect = Defined.EffectCreate(GetLunaAssetBundle().BerserkEffect, gameObject, Defined.eEffectPosition.TARGET, true);

        var ani = transform.Find("Sprite").GetComponent<SkeletonAnimation>();
        Color tmpColor = ani.skeleton.GetColor();

        bool tmpSuperArmour = unitState.IsSuperArmour;
        {
            //ani.skeleton.SetColor(tmpColor - new Color(0.1f, 0.1f, 0.1f, 0));
            ani.skeleton.SetColor(Color.black);
            coefficient.AttackSpeed += 0.5f;
            coefficient.MoveSpeed += 1.0f;
            unitState.IsSuperArmour = true;
            StatusAfterReady();
        }

        yield return new WaitForSeconds(duration);
        {
            ani.skeleton.SetColor(tmpColor);

            coefficient.AttackSpeed -= 0.5f;
            coefficient.MoveSpeed -= 1.0f;
            unitState.IsSuperArmour = tmpSuperArmour && objectData.TypeSuperArmour;
            StatusAfterReady();
        }

        Destroy(tmpEffect);
        isBerserk = false;
    }



    public void TakeBeleifWaveMode(float duration)
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (iSBeleifWave == false)
        {
            iSBeleifWave = true;
            StartCoroutine(BeleifWave_Proceed(duration));

            BuffImp.AddBuff(eBuffType.ATTACK, duration);
        }
    }
    IEnumerator BeleifWave_Proceed(float duration)
    {
        GameObject tmpEffect = Defined.EffectCreate(GetLunaAssetBundle().beleifWaveEffect, gameObject, Defined.eEffectPosition.UNDERFEET, true);

        var ani = transform.Find("Sprite").GetComponent<SkeletonAnimation>();
        Color tmpColor = ani.skeleton.GetColor();
        ani.skeleton.SetColor(tmpColor - new Color(0.5f, 0.5f, 0.5f, 0));
        stat.AttackAvoidance += 0.25f;

        StatusAfterReady();


        yield return new WaitForSeconds(duration);
        {
            ani.skeleton.SetColor(tmpColor);

            StatusAfterReady();
        }

        Destroy(tmpEffect);
        iSBeleifWave = false;
    }


    public void TakeSerenBlessingMode(float duration)
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (isSerenBlessing == false)
        {
            isSerenBlessing = true;
            StartCoroutine(SerenBlessing_Proceed(duration));

            BuffImp.AddBuff(eBuffType.ATTACK, duration);
            BuffImp.AddBuff(eBuffType.DEFENSE, duration);
        }
    }
    IEnumerator SerenBlessing_Proceed(float duration)
    {
        GameObject tmpEffect = Defined.EffectCreate(GetLunaAssetBundle().serenBlessing, gameObject, Defined.eEffectPosition.UNDERFEET, true);

        var ani = transform.Find("Sprite").GetComponent<SkeletonAnimation>();
        Color tmpColor = ani.skeleton.GetColor();
        ani.skeleton.SetColor(tmpColor - new Color(0.5f, 0.5f, 0.5f, 0));

        stat.AttackAvoidance += 0.25f;
        stat.SkillAvoidance += 0.25f;
        stat.DebuffAvoidance += 0.25f;
        StatusAfterReady();


        yield return new WaitForSeconds(duration);
        {
            ani.skeleton.SetColor(tmpColor);
            stat.AttackAvoidance -= 0.25f;
            stat.SkillAvoidance -= 0.25f;
            stat.DebuffAvoidance -= 0.25f;

            StatusAfterReady();
        }

        Destroy(tmpEffect);
        isSerenBlessing = false;
    }



    public void TakeLoneBlessingMode(float duration)
    {
        if (hitPriority == eHitPriority.TOWER)
            return;

        if (isLoneBlessing == false)
        {
            isLoneBlessing = true;
            StartCoroutine(LoneBlessing_Proceed(duration));

            BuffImp.AddBuff(eBuffType.DEFENSE, duration);
        }
    }
    IEnumerator LoneBlessing_Proceed(float duration)
    {
        GameObject tmpEffect = Defined.EffectCreate(GetLunaAssetBundle().loneBlessing, gameObject, Defined.eEffectPosition.UNDERFEET, true);

        var ani = transform.Find("Sprite").GetComponent<SkeletonAnimation>();
        Color tmpColor = ani.skeleton.GetColor();
        ani.skeleton.SetColor(tmpColor - new Color(0.5f, 0.5f, 0.5f, 0));

        coefficient.Armour += 1.0f;
        StatusAfterReady();


        yield return new WaitForSeconds(duration);
        {
            ani.skeleton.SetColor(tmpColor);
            coefficient.Armour -= 0.25f;
            StatusAfterReady();
        }

        Destroy(tmpEffect);
        isLoneBlessing = false;
    }





    public void TakeStun(float duration)
    {
        if (unitState.IsDeath == true)
            return;

        if (hitPriority == eHitPriority.TOWER)
            return;

        if (curStun == false)
        {
            curStun = true;
            StartCoroutine(Stun_Proceed(duration));
        }
    }

    IEnumerator Stun_Proceed(float duration)
    {
        GameObject tmpEffect = Defined.EffectCreate(GetLunaAssetBundle().StunEffect, gameObject, Defined.eEffectPosition.TARGET, true);

        float tmpSpeed = stat.MoveSpeed;
        {
            isStun = true;
        }

        yield return new WaitForSeconds(duration);
        {
            isStun = false;
        }

        Destroy(tmpEffect);
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
        float knuckbackPower = 0.3f;
        float jumpPower = 3.0f;

        const float gravitySpeed = 4.5f;

        if (!(UnitSide == eUnitSide.Enemy))
            knuckbackPower *= -1;

        while (transform.position.y >= 0)
        {
            //InfiniteLoopDetector.Run();
            if (transform.position.x <= Defined.AllyTowerPosition.x)
            {
                knuckbackPower = 0;
            }

            jumpPower -= gravitySpeed * Time.deltaTime;
            transform.Translate(knuckbackPower * Time.deltaTime, jumpPower * Time.deltaTime, 0);

            yield return null;
        }

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

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
        var ani = transform.Find("Sprite").GetComponent<SkeletonAnimation>();
        Color tmpColor = ani.skeleton.GetColor();

        {
            ani.skeleton.SetColor(tmpColor - new Color(0, 0, 0, -0.5f));

            stat.AttackAvoidance += 200f;
            stat.SkillAvoidance += 200f;
            stat.DebuffAvoidance += 200f;
        }
        yield return new WaitForSeconds(duration);
        {
            ani.skeleton.SetColor(tmpColor);

            stat.AttackAvoidance -= 200f;
            stat.SkillAvoidance -= 200f;
            stat.DebuffAvoidance -= 200f;
        }

        curStealth = false;
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
        {
            stat.AttackAvoidance += 200f;
            stat.SkillAvoidance += 200f;
            stat.DebuffAvoidance += 200f;
        }
        if (duration != 0)
        {
            yield return new WaitForSeconds(duration);
            {
                stat.AttackAvoidance -= 200f;
                stat.SkillAvoidance -= 200f;
                stat.DebuffAvoidance -= 200f;
            }
        }

        curInvincible = false;
    }



    public void RoenSkill(float duration)
    {
        if (curRoenSkill == false)
        {
            curRoenSkill = true;
            StartCoroutine(RoenSkill_Proceed(duration));

            BuffImp.AddBuff(eBuffType.ATKSPEED, duration);
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



    public void Curse() // 저주
    {
        if (Debuff())
        {
            stat.Armour *= 0.8f;
            stat.MoveSpeed *= 0.8f;
        }
    }


    public bool Debuff()
    {
        return !Defined.CanProbability(stat.DebuffAvoidance);
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
        if (SkillTrigger_Roni == false && Defined.SkillTrigger_Roni == true)
        {
            SkillTrigger_Roni = true;

            if (UnitAttackType != eUnitAttackType.Ranged)
            {
                coefficient.HP += 0.1f;
                HpAfterReady();
            }
        }
        else if (SkillTrigger_Roni == true && Defined.SkillTrigger_Roni == false)
        {
            SkillTrigger_Roni = false;

            if (UnitAttackType != eUnitAttackType.Ranged)
            {
                coefficient.HP -= 0.1f;
                HpAfterReady();
            }
        }

        if (SkillTrigger_Rose == false && Defined.SkillTrigger_Rose == true)
        {
            SkillTrigger_Rose = true;

            if (UnitSide == eUnitSide.Enemy)
            {
                coefficient.AttackDamage -= 0.1f;
                StatusAfterReady();
            }
        }
        else if (SkillTrigger_Rose == true && Defined.SkillTrigger_Rose == false)
        {
            SkillTrigger_Rose = false;

            if (UnitSide == eUnitSide.Allied)
            {
                coefficient.AttackDamage += 0.1f;
                HpAfterReady();
            }
        }

        if (SkillTrigger_Roen1 == false && Defined.SkillTrigger_Roen1 == true)
        {
            SkillTrigger_Roen1 = true;

            if (UnitSide == eUnitSide.Allied /*&& 공격형 영웅*/)
            {
                coefficient.AttackSpeed += 0.15f;
                StatusAfterReady();
            }
        }
        else if (SkillTrigger_Roen1 == true && Defined.SkillTrigger_Roen1 == false)
        {
            SkillTrigger_Roen1 = false;

            if (UnitSide == eUnitSide.Allied /*&& 공격형 영웅*/)
            {
                coefficient.AttackSpeed -= 0.15f;
                StatusAfterReady();
            }
        }

        if (SkillTrigger_Roen2 == false && Defined.SkillTrigger_Roen2 == true)
        {
            SkillTrigger_Roen2 = true;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed)
            {
                stat.AttackAvoidance += 5.0f; //무슨회피율?
            }
        }
        else if (SkillTrigger_Roen2 == true && Defined.SkillTrigger_Roen2 == false)
        {
            SkillTrigger_Roen2 = false;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed)
            {
                stat.AttackAvoidance -= 5.0f; //무슨회피율?
            }
        }

        if (SkillTrigger_Roen3 == false && Defined.SkillTrigger_Roen3 == true)
        {
            SkillTrigger_Roen3 = true;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed && hitPriority == eHitPriority.NORMAL)
            {
                StartWarf(4.0f);
            }
        }
        else if (SkillTrigger_Roen3 == true && Defined.SkillTrigger_Roen3 == false)
        {
            SkillTrigger_Roen3 = false;

            if (UnitSide == eUnitSide.Allied && UnitAttackType == eUnitAttackType.Closed && hitPriority == eHitPriority.NORMAL)
            {
                EndWarf();
            }
        }

        if (SkillTrigger_Elena == false && Defined.SkillTrigger_Elena == true)
        {
            SkillTrigger_Elena = true;

            if (UnitSide == eUnitSide.Allied)
            {
                coefficient.Armour += 0.08f;
                StatusAfterReady();
            }
        }
        else if (SkillTrigger_Elena == true && Defined.SkillTrigger_Elena == false)
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
        float tmpHp = stat.Hp / HealthBarHpPoint;

        HealthBarHpPoint = (objectData.Hp + (Level - 1) * objectData.LevelUpHpPoint) * coefficient.HP;
        stat.Hp = tmpHp * HealthBarHpPoint;
    }

    public void StatusAfterReady()
    {
        // status afterupdate
        stat.Stamina = objectData.GetCurrentLevelStamina_Value(coefficient.Stamina);
        stat.AttackSpeed = objectData.GetAttackOnceBySecond_Value(coefficient.AttackSpeed);
        stat.MoveSpeed = objectData.GetMoveSpeed_Value(coefficient.MoveSpeed);
        stat.Armour = objectData.GetArmour_Value(coefficient.Armour);
        stat.AttackDamage = objectData.GetAttackDamage_Value(coefficient.AttackDamage);
    }

    private void Start()
    {

    }
    private void Update()
    {
        if (IsStatusDataLoadComplete == true)
        {
            ConfirmSkillTriger();
        }
    }
}
