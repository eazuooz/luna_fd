using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Spine.Unity;

public class UISkill : UILuna
{
    public UIReference refData;

    private List<SkillInfo> lunaSkillDatas
        = new List<SkillInfo>();
    private List<SkillInfo> equipDesignSkillDatas
        = new List<SkillInfo>();

    private GameObject summonEffect;


    private GameObject uiPlayer;
    public class SkillInfo
    {
        public SkillData skillData;
        public GameObject skillButton;
        public System.Func<SkillData, bool> skillFunc;
        public bool isActive;
        public float currentCoolTime;
        public bool completePassive;

        public GameObject slotEffect;
    }
    private Dictionary<string, System.Func<SkillData, bool>> skillFuncDictionary
        = new Dictionary<string, System.Func<SkillData, bool>>();
    private GameObject maskObject;


    public bool Auto { get; set; }
    public override void OnInit()
    {
        base.OnInit();

        uiPlayer = GameObject.FindWithTag("UIPlayer");
        maskObject = gameObject.transform.Find("mask").gameObject;
        maskObject.SetActive(false);
        Auto = false;


        InitializeSkillFunction();


        //List<string> equippedDesign = new List<string>();
        //equippedDesign.Add("LunaEquipmentWoodSword");
        //equippedDesign.Add("LunaEquipmentWoodShield");
        //equippedDesign.Add("LunaEquipmentWoodRing");

        EquipDesignItemData equipDesign 
            = LunaDataTable.Instance.GetCurrentEquipDesignItemData();

        int idx = 0;
        foreach (var equip in equipDesign.equipSkills)
        {
            string skillStr = equip.name;
            string buttonSkill = "Equip_" + idx.ToString();
            CreateEquipSkillButton(skillStr, buttonSkill, skillFuncDictionary[skillStr]);
        }
        
        ///


        List<string> equippedSkill 
            = LunaDataTable.Instance.playerData.equippedSkill;

        for (int i = 0; i < 3; i++)
        {
            string skillStr = equippedSkill[i];
            string buttonSkill = "Skill_Button_" + i.ToString();
            CreateSkillButton(skillStr, buttonSkill, skillFuncDictionary[skillStr]);
        }




        SkillEffectWarmPool();
        foreach (var item in lunaSkillDatas)
        {
            Transform temp = item.skillButton.transform.Find("SKillSlotEffect");
            item.slotEffect = temp.gameObject;
            item.slotEffect.SetActive(false);
        }

        summonEffect = Resources.Load<GameObject>("Prefabs/06Effects/Hero/SummonEffectYellow");

        //패시브 스킬 적용
        foreach (SkillInfo item in equipDesignSkillDatas)
        {
            if (item.skillData.type == SkillData.SkillType.Passive)
                item.skillButton.GetComponent<Button>().onClick.Invoke();
        }

    }

    public override void OnActive()
    {

    }

    public override void OnInActive() { }

    public override void OnUpdate()
    {


        SkillCoolTimeUpdate();
        if (Auto)
        {
            UseAllSkill();
        }
    }

    public override void OnLoop() { }

    public override void OnClear() { }

    private void InitializeSkillFunction()
    {
        skillFuncDictionary.Add("FireBall", FireBall);
        skillFuncDictionary.Add("Blizzard", Blizzard);
        skillFuncDictionary.Add("Healing", Healing);
        skillFuncDictionary.Add("Thunder", StartThunder);

        skillFuncDictionary.Add("GoldyBlessing", GoldyBlessing);
        skillFuncDictionary.Add("LoneBleesing", LoneBleesing);
        skillFuncDictionary.Add("WaterDropBarrier", WaterDropBarrier);
        skillFuncDictionary.Add("Dwarf", Dwarf);

        skillFuncDictionary.Add("AngerArmy", AngerArmy);
        skillFuncDictionary.Add("DemiGod", DemiGod);
        skillFuncDictionary.Add("SerenBlessing", SerenBlessing);
        skillFuncDictionary.Add("BeleifWave", BeleifWave);
        skillFuncDictionary.Add("KingSlime", KingSlime);
        skillFuncDictionary.Add("SkeletonArcher", SkeletonArcher);

        skillFuncDictionary.Add("LunaEquipmentWoodSword", LunaEquipmentWoodSword);
        skillFuncDictionary.Add("LunaEquipmentWoodShield", LunaEquipmentWoodShield);
        skillFuncDictionary.Add("LunaEquipmentWoodRing", LunaEquipmentWoodRing);

        skillFuncDictionary.Add("LunaEquipmentSkullSword", LunaEquipmentSkullSword);
        skillFuncDictionary.Add("LunaEquipmentSkullShield", LunaEquipmentSkullShield);
        skillFuncDictionary.Add("LunaEquipmentSkullRing", LunaEquipmentSkullRing);

        skillFuncDictionary.Add("LunaEquipmentRichSword", LunaEquipmentRichSword);
        skillFuncDictionary.Add("LunaEquipmentRichShield", LunaEquipmentRichShield);
        skillFuncDictionary.Add("LunaEquipmentRichRing", LunaEquipmentRichRing);

        skillFuncDictionary.Add("LunaEquipmentDragonSword", LunaEquipmentDragonSword);
        skillFuncDictionary.Add("LunaEquipmentDragonShield", LunaEquipmentDragonShield);
        skillFuncDictionary.Add("LunaEquipmentDragonRing", LunaEquipmentDragonRing);

        skillFuncDictionary.Add("LunaEquipmentSlimeSword", LunaEquipmentSlimeSword);
        skillFuncDictionary.Add("LunaEquipmentSlimeShield", LunaEquipmentSlimeShield);
        skillFuncDictionary.Add("LunaEquipmentSlimeRing", LunaEquipmentSlimeRing);

        skillFuncDictionary.Add("LunaEquipmentLeneSword", LunaEquipmentLeneSword);
        skillFuncDictionary.Add("LunaEquipmentLeneShield", LunaEquipmentLeneShield);
        skillFuncDictionary.Add("LunaEquipmentLeneRing", LunaEquipmentLeneRing);
    }
    private void CreateSkillButton(string soFileName, string buttonName, System.Func<SkillData, bool> skillFunc)
    {
        SkillInfo info = new SkillInfo();
        LunaDataTable.Instance.skiiTable.TryGetValue(soFileName, out info.skillData);
        info.skillButton = gameObject.transform.Find(buttonName).gameObject;

        info.skillButton.transform.Find("Skill_Button_Image").GetComponent<Image>().sprite
            = info.skillData.skillImage;
        info.skillFunc = skillFunc;
        info.skillButton.GetComponent<Button>().onClick.AddListener
            (
                () => UseSkillFunction(info, skillFunc)
            );
        info.isActive = false;
        info.completePassive = false;
        info.currentCoolTime = info.skillData.coolTime;
        lunaSkillDatas.Add(info);

        info.skillButton.transform.Find("Mana").Find("SkillPoint").GetComponent<Text>().text
            = info.skillData.mana.ToString();
    }
    private void CreateEquipSkillButton(string soFileName, string buttonName, System.Func<SkillData, bool> skillFunc)
    {
        SkillInfo info = new SkillInfo();
        LunaDataTable.Instance.skiiTable.TryGetValue(soFileName, out info.skillData);
        info.skillButton = gameObject.transform.Find(buttonName).gameObject;

        info.skillButton.transform.Find("Equip_Image").GetComponent<Image>().sprite
            = info.skillData.skillImage;
        info.skillFunc = skillFunc;
        info.skillButton.GetComponent<Button>().onClick.AddListener
            (
                () => UseSkillFunction(info, skillFunc)
            );
        info.isActive = false;
        info.completePassive = false;
        info.currentCoolTime = info.skillData.coolTime;
        equipDesignSkillDatas.Add(info);

        //info.skillButton.transform.Find("Mana").Find("SkillPoint").GetComponent<Text>().text
        //    = info.skillData.mana.ToString();
    }

    private void SkillEffectWarmPool()
    {
        foreach (var item in lunaSkillDatas)
        {
            if (item.skillData.isMultiple)
            {
                PoolManager.WarmPool(item.skillData.effectPrefab, 30);
            }
        }

        foreach (var item in equipDesignSkillDatas)
        {
            if (item.skillData.isMultiple)
            {
                PoolManager.WarmPool(item.skillData.effectPrefab, 30);
            }
        }

    }
    private void SkillCoolTimeTextUpdate(SkillInfo info)
    {
        if (!info.isActive)
        {
            info.skillButton.GetComponent<PlayerSkillCoolChecker>().skillCooltime
                = (int)info.currentCoolTime;
        }
        else
        {
            info.skillButton.GetComponent<PlayerSkillCoolChecker>().skillCooltime
                = 0;
        }
    }
    private void SkillCoolTimeUpdate()
    {
        foreach (SkillInfo info in lunaSkillDatas)
        {
            int mana = uiPlayer.GetComponent<UIPlayer>().Mana;
            if (mana < info.skillData.mana)
            {
                info.skillButton.transform.Find("Skill_Button_Image").GetComponent<Image>().color
                                    = Color.grey;
            }
            else
            {
                info.skillButton.transform.Find("Skill_Button_Image").GetComponent<Image>().color
                        = Color.white;
            }

            if (!info.isActive)
            {
                info.skillButton.transform.Find("Skill_Button_Image").GetComponent<Image>().color
                    = Color.grey;

                info.currentCoolTime -= Time.deltaTime;
                if (info.currentCoolTime <= 0.0f)
                {
                    info.skillButton.transform.Find("Skill_Button_Image").GetComponent<Image>().color
                        = Color.white;
                    info.isActive = true;
                    info.currentCoolTime = info.skillData.coolTime;

                    info.slotEffect.SetActive(true);
                }
                SkillCoolTimeTextUpdate(info);
            }
        }
    }

    private void UseAllSkill()
    {
        foreach (var info in lunaSkillDatas)
        {
            if (info.isActive)
            {
                UseSkillFunction(info, info.skillFunc);
            }
        }
    }
    #region LUNA_SKILL
    public void OnClickSkillAuto()
    {
        if (Auto)
        {
            Auto = false;
        }
        else
        {
            Auto = true;
        }
    }
    private void UseSkillFunction(SkillInfo info, System.Func<SkillData, bool> skill)
    {
        bool IsDeath
            = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().unitState.IsDeath;

        if (IsDeath == true)
            return;

        if (!(Game.Instance.dataManager.LunaPlayer.GetComponent<Status>().stat.Hp > 0.0f))
            return;

        int useMana = uiPlayer.GetComponent<UIPlayer>().Mana - info.skillData.mana;
        if (useMana < 0)
        {
            return;
        }

        if (info.skillData.type == SkillData.SkillType.Passive
            && info.completePassive == false)
        {
            info.completePassive = true;
            if (skill(info.skillData) == false)
                return;
        }

        if (info.isActive == true)
        {
            if (skill(info.skillData) == false)
                return;

            info.isActive = false;

            info.currentCoolTime = info.skillData.coolTime;
            info.slotEffect.SetActive(false);

            uiPlayer.GetComponent<UIPlayer>().Mana -= info.skillData.mana;
        }
    }
    public bool FireBall(SkillData data)
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(Game.Instance.dataManager.LunaPlayer, unit, data.range))
            {
                GameObject bullet
                    = PoolManager.SpawnObject(data.effectPrefab
                    , unit.GetComponent<Status>().ProjecttileArrivalLocation.transform.position + new Vector3(0, 2.5f + Random.Range(0.0f, 2.0f), 0)
                    , data.effectPrefab.transform.rotation);
                bullet.GetComponent<Projectile>().Attacker = Game.Instance.dataManager.LunaPlayer;
                bullet.GetComponent<Projectile>().Target = unit;
                bullet.GetComponent<Projectile>().ProjectilePower = data.damage;
                bullet.GetComponent<Projectile>().criticalShot = true;
                bullet.GetComponent<Projectile>().skillShot = true;
            }
        }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);

        return true;

    }

    private bool Blizzard(SkillData data)
    {
        Defined.EffectCreate(data.effectPrefab
            , Game.Instance.dataManager.LunaPlayer
            , Defined.eEffectPosition.UNDERFEET);

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistanceRight(Game.Instance.dataManager.LunaPlayer, unit, data.range))
            {
                unit.GetComponent<Status>().Attack(gameObject, data.damage, false, true);
                unit.GetComponent<Status>().TakeStun(2.0f);
            }
        }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);

        return true;
    }

    private bool Healing(SkillData data)
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(Game.Instance.dataManager.LunaPlayer, unit, data.range))
            {
                Defined.EffectCreate(data.effectPrefab, unit, Defined.eEffectPosition.TARGET, true);
                unit.GetComponent<Status>().Heal(data.damage);
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        return true;
    }

    private bool StartThunder(SkillData data)
    {
        StartCoroutine(Thunder(data));

        return true;
    }
    IEnumerator Thunder(SkillData data)
    {
        maskObject.SetActive(true);
        Vector3 clickedPos = Vector3.zero;
        bool clicked = false;
        while (!clicked)
        {
            //InfiniteLoopDetector.Run();
            Time.timeScale = 0.5f;
            if (Input.GetMouseButtonDown(0))
            {
                clicked = true;
                clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                clickedPos.y = 0;
                clickedPos.z = 0;
            }

            ///대충 작동하는내용
            yield return null;
        }
        Time.timeScale = 1.5f;
        maskObject.SetActive(false);

        GameObject player = Game.Instance.dataManager.LunaPlayer;
        List<GameObject> monsterList = new List<GameObject>();
        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(clickedPos, unit, data.range))
            {
                monsterList.Add(unit);
            }
        }, IdentifyFunc.IsEnemy);

        foreach (GameObject monster in monsterList)
        {
            Defined.EffectCreate(data.effectPrefab, monster
                , Defined.eEffectPosition.TARGET, true);

            monster.GetComponent<Status>().Attack(player, data.damage, false, true);
            monster.GetComponent<Status>().TakeStun(data.stunTime);

            yield return new WaitForSeconds(0.45f);
        }
    }

    private bool GoldyBlessing(SkillData data)
    {
        //- 스테이지 골드 획득량 +% 증가
        Defined.SkillTrigger_Goldy = 6.0f;

        return true;

        //->스킬 시전이 아니라 스킬을 끼면 스테이지에서 그냥 적용되도록 해야 함.
    }

    private bool LoneBleesing(SkillData data)
    {
        //론의 가호
        //-버튼을 누르면 10 ? 초간 파티 전체의 방어력이 ?% 상승한다.
        // - 스킬 유효기간을 10초로 할 경우 쿨타임을 30초? 정도로 길게 설정합니다.

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().coefficient.Armour += 1.0f; // 대충 100퍼센트
            unit.GetComponent<Status>().TakeLoneBlessingMode(10.0f);
        }, IdentifyFunc.IsAlly);

        return true;

    }

    //IEnumerator LoneBless(SkillData data)
    //{
    //    Identify.SelectUnit((unit) =>
    //    {
    //        unit.GetComponent<Status>().coefArmour += 1.0f; // 대충 100퍼센트
    //        unit.GetComponent<Status>().StatusAfterReady();
    //    }, Identify.IsAlly);

    //    yield return new WaitForSeconds(10.0f); // 대충 10초 <- 적용필요

    //    Identify.SelectUnit((unit) =>
    //    {
    //        unit.GetComponent<Status>().coefArmour -= 1.0f; // 원상복구
    //        unit.GetComponent<Status>().StatusAfterReady();
    //    }, Identify.IsAlly);
    //}

    private bool WaterDropBarrier(SkillData data)
    {
        //물방울 방패
        //-물방울 방패가 적용된 파티원 1명 5초간 받는 데미지 50 % 감소
        //- 레벨업을 통해 받는 데미지가 100 % 까지 올릴 수 있게 설정합니다.

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().TakePochiBarrier_ForPlayer(5.0f);
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsPlayer);

        return true;

        // 지금 포치 쉴드가 100% 뎀감인데 걍 별도로 이건 이렇게 저건 저렇게 적용시키는거임?
        // 아니면 포치 스킬도 50~100% 뎀감으로 수정?
    }

    GameObject SummonSkill(SkillData data, int Cap = 0, int amount = 1)
    {
        GameObject minion = PoolManager.Instance.spawnObject(data.effectPrefab, Defined.AllyTowerPosition, data.effectPrefab.transform.rotation);

        Defined.EffectCreate(summonEffect, minion, Defined.eEffectPosition.UNDERFEET, true);

        return minion;
    }

    private bool Dwarf(SkillData data)
    {
        // 방패 드워프 소환

        SummonSkill(data);

        return true;
    }

    private bool AngerArmy(SkillData data)
    {
        //        분노의 군단

        //-3초간 루나를 제외한 파티원 전체 광폭화

        //-광폭화가 적용된 캐릭터는 검은색 실루엣으로 변하고 공, 방, 공격 속도가 2배 증가합니다.

        //-레벨업으로 광폭화 스탯이 증가하게 할 예정입니다.

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().TakeBerserkMode(3.0f);

        }, IdentifyFunc.IsAlly, IdentifyFunc.IsSoldier);

        return true;
    }

    private bool DemiGod(SkillData data)
    {
        //        분신술

        //- 루나의 분신 2마리 5초? 간 소환

        // - 루나의 분신도 검은색 실루엣으로 표현이 되도록 적용합니다.

        //-루나의 분신은 루나의 스탯에서 공격 속도 / 이동 속도 2배 증가하고, 공격력/ 방어력은 50 % 감소합니다.

        //  - 루나의 분신은 평타 공격만 합니다.

        GameObject summon = SummonSkill(data);

        var ani = summon.transform.Find("Sprite").GetComponent<SkeletonAnimation>();
        Color tmpColor = ani.skeleton.GetColor();
        ani.skeleton.SetColor(tmpColor - new Color(0.7f, 0.7f, 0.7f, 0));

        return true;
    }

    private bool SerenBlessing(SkillData data)
    {
        //        세렌의 가호

        //-파티원 전체 10초? 간 모든 저항에 25 % 감소

        //- 레벨업을 통해 50 % 이상으로 저항 스탯을 올릴 수 있게 설정할 예정입니다.

        // - 독이나 출혈 등의 속성 특화 스테이지나 보스에 유리한 스킬입니다.
        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().TakeSerenBlessingMode(5.0f);
        }, IdentifyFunc.IsAlly);

        return true;
    }

    private bool BeleifWave(SkillData data)
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().TakeBeleifWaveMode(5.0f);
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        return true;

    }

    private bool KingSlime(SkillData data)
    {
        //        킹 슬라임 소환
        //- 킹 슬라임 소환
        //-킹 슬라임은 스테이지당 1회만 소환 가능하며, 굉장히 많은 소환력을 소비하도록 설정합니다.
        //- 레벨업으로 킹슬라임의 스탯을 올리게 됩니다.

        if (CheckSummon(data) >= data.maxSummonCount)
            return false;

        SummonSkill(data);

        return true;
    }

    private bool SkeletonArcher(SkillData data)
    {
        //해골 궁수 소환
        //- 해골 궁수 1마리 소환
        //-쿨타임을 20 ? 초 정도로 하고 필드에 최대 4마리까지 소환할 수 있게 설정합니다.
        //- 4마리가 모두 필드에 있으면 소환할 수 없으며, 1마리가 죽어 3마리가 되면 1마리를 소환할 수 있는 방식입니다.

        if (CheckSummon(data) >= data.maxSummonCount)
            return false;

        SummonSkill(data);

        return true;
    }

    private int CheckSummon(SkillData data)
    {
        int count = 0;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (unit.name.Substring(0, data.effectPrefab.name.Length) == data.effectPrefab.name)
                count++;

        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        return count;
    }





    private bool LunaEquipmentWoodSword(SkillData data)
    {
        Status stat = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>();
        
        //stat.stat.MultipleSkillDamage(data.multipleSkillDamageBuff);

        return true;
    }
    private bool LunaEquipmentWoodShield(SkillData data)
    {

        Status stat = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>();

        //stat.stat.MultipleSkillResistance(data.multipleSkillResistanceBuff);

        return true;
    }
    private bool LunaEquipmentWoodRing(SkillData data)
    {
        StartCoroutine(LunaEquipmentWoodRing_Coroutine(data));

        return true;
    }

    IEnumerator LunaEquipmentWoodRing_Coroutine(SkillData data)
    {
        while(true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(data.coolTime);

            IdentifyFunc.PickingUnits((unit) =>
            {
                unit.GetComponent<Status>().Heal(unit.GetComponent<Status>().objectData.GetCurrentLevelHP_Value() * 0.2f);
            }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
        }
    }






    private bool LunaEquipmentSkullSword(SkillData data)
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().MultipleAttackDamage((data.coefficient * 0.01f) + 1.0f);
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        return true;
    }
    private bool LunaEquipmentSkullShield(SkillData data)
    {
        StartCoroutine(LunaEquipmentSkullShield_Coroutine(data));

        return true;
    }

    IEnumerator LunaEquipmentSkullShield_Coroutine(SkillData data)
    {
        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(data.coolTime);

            IdentifyFunc.PickingUnits((unit) =>
            {
                unit.GetComponent<Status>().TakeBleeding(unit.GetComponent<Status>().objectData.GetCurrentLevelHP_Value() * 0.1f, data.burfTime);
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }
    private bool LunaEquipmentSkullRing(SkillData data)
    {
        StartCoroutine(LunaEquipmentSkullRing_Coroutine(data));

        return true;
    }

    IEnumerator LunaEquipmentSkullRing_Coroutine(SkillData data)
    {
        Status stat = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>();

        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(data.coolTime);

            stat.TakeBerserkMode(data.burfTime);
        }
    }





    private bool LunaEquipmentRichSword(SkillData data)
    {
        Defined.SkillTrigger_Goldy *= 1.1f;

        return true;
    }

    private bool LunaEquipmentRichShield(SkillData data)
    {
        Defined.SkillTrigger_Goldy *= 1.15f;

        return true;
    }

    private bool LunaEquipmentRichRing(SkillData data)
    {
        Defined.SkillTrigger_Goldy *= 1.15f;

        return true;
    }





    private bool LunaEquipmentDragonSword(SkillData data)
    {
        // 평타 50% 확률로 스킬데미지 50~70% 번개 찍기
        Status stat = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>();
        SkeletonAnimation skeletonAnimation = Game.Instance.dataManager.LunaPlayer.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        skeletonAnimation.state.Event += delegate (Spine.TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == "attack")
            {
                if (stat.AttackTarget != null && Defined.CanProbability(data.probablity))
                {
                    Defined.EffectCreate(data.effectPrefab, stat.AttackTarget, Defined.eEffectPosition.UNDERFEET);
                    stat.AttackTarget.GetComponent<Status>().Attack(Game.Instance.dataManager.LunaPlayer, stat.stat.AttackDamage * 2.0f * data.coefficient,false,true);
                }
            }
        };

        return true;
    }

    private bool LunaEquipmentDragonShield(SkillData data)
    {
        if (Game.Instance.dataManager.LunaPlayer.GetComponent<LunaCtrl_Dragon>() != null)
        {
            Game.Instance.dataManager.LunaPlayer.GetComponent<LunaCtrl_Dragon>().DragonSheildSkill((data.coefficient * 0.01f) + 1.0f);
        }

        return true;
    }

    private bool LunaEquipmentDragonRing(SkillData data)
    {
        StartCoroutine(LunaEquipmentDragonRing_Coroutine(data));

        return true;
    }

    IEnumerator LunaEquipmentDragonRing_Coroutine(SkillData data)
    {
        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(data.coolTime);

            FireBall(data);
        }
    }



    private bool LunaEquipmentLeneSword(SkillData data)
    {
        StartCoroutine(LunaEquipmentLeneSword_Coroutine(data));

        return true;
    }

    IEnumerator LunaEquipmentLeneSword_Coroutine(SkillData data)
    {
        Status stat = Game.Instance.dataManager.LunaPlayer.GetComponent<Status>();

        while (true)
        {
           // InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(data.coolTime);

            stat.Heal(stat.objectData.GetCurrentLevelHP_Value() * data.coefficient * 0.01f);
        }
    }

    private bool LunaEquipmentLeneShield(SkillData data)
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().stat.ArmourPercentForBoss = data.coefficient;
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        return true;
    }

    private bool LunaEquipmentLeneRing(SkillData data)
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().stat.AttackPercentForBoss = data.coefficient;
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        return true;
    }





    private bool LunaEquipmentSlimeSword(SkillData data)
    {
        StartCoroutine(LunaEquipmentSlimeSword_Coroutine(data));

        return true;
    }

    IEnumerator LunaEquipmentSlimeSword_Coroutine(SkillData data)
    {
        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(1.0f);

            if (CheckSummon(data) < data.maxSummonCount)
            {
                SummonSkill(data);

                yield return new WaitForSeconds(data.coolTime);
            }
        }
    }

    private bool LunaEquipmentSlimeShield(SkillData data)
    {
        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().MultipleAttackSpeed((data.coefficient * 0.01f) + 1.0f);
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        return true;
    }

    private bool LunaEquipmentSlimeRing(SkillData data)
    {
        StartCoroutine(LunaEquipmentSlimeRing_Coroutine(data));

        return true;
    }

    IEnumerator LunaEquipmentSlimeRing_Coroutine(SkillData data)
    {
        SummonSkill(data);
        yield return null;
        SummonSkill(data);
    }



    public void AllSkillAble()
    {
        //data.skil
    }

    #endregion
}
