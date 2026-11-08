#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EverydayDevup.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>Opt-in checks against authored rank behavior, without screen or input automation.</summary>
public static class LunaHeroRankRegression
{
    public static IEnumerator Validate(Action<string> record)
    {
        Require(Application.isEditor || Debug.isDebugBuild, "Regression checks require Editor or Development Build.");
        var existing = new HashSet<LunaEntityLogic>(Object.FindObjectsByType<LunaEntityLogic>(FindObjectsInactive.Include));
        Random.State randomState = Random.state;
        try
        {
            Lene lene = CreateHero<Lene>("Lene");
            CheckLeneChance(lene, lene.STAR1, 40f);
            CheckLeneChance(lene, lene.STAR5, 80f);
            CheckLeneChance(lene, lene.ULT1, 100f);
            CheckLeneChance(lene, lene.ULT2, 100f);
            CheckLeneChance(lene, lene.STAR1, 40f);
            Require((float)Field(typeof(Lene), "AllyProcChance").GetRawConstantValue() == 40f,
                "Lene ally probability must remain at the authored 40% baseline.");
            record?.Invoke("Lene rank initialization: self proc chance 40/80/100%, ally baseline 40%, lower-rank reset verified.");

            CheckRoseDebuff(record);

            CheckSummonGate(CreateHero<Ria>("Ria"), record);
            CheckSummonGate(CreateHero<Lena>("Lena"), record);

            Scalet scarlet = CreateHero<Scalet>("Scarlet");
            Status target = CreateHero<Lene>("Lene").GetComponent<Status>();
            int curseSeed = FindCurseSeed();
            CheckCurse(scarlet, target, scarlet.ULT1, curseSeed, false);
            CheckCurse(scarlet, target, scarlet.ULT2, curseSeed, true);
            record?.Invoke("Scarlet real skill sequence: ULT1 leaves defenses unchanged; ULT2 successful 80% roll invokes Curse (armor/speed x0.8).");

            Matilda matildaPrefab = Resources.Load<GameObject>("Prefabs/01Heroes/Matilda").GetComponent<Matilda>();
            Require(matildaPrefab.minion1.GetComponent<Status>() == null && matildaPrefab.minion2.GetComponent<Status>() == null &&
                !matildaPrefab.minion1.CompareTag("Unit") && !matildaPrefab.minion2.CompareTag("Unit"),
                "Matilda projectile-like summons changed; review the documented ULT1 immunity limitation.");
            record?.Invoke("Matilda original mini/fire summons verified as non-Status, non-Unit attack effects; no invalid Invincible call added.");
        }
        finally
        {
            // Every assertion and direct skill call runs synchronously before the only yield.
            foreach (LunaEntityLogic entity in Object.FindObjectsByType<LunaEntityLogic>(FindObjectsInactive.Include))
                if (entity != null && !existing.Contains(entity)) LunaEntities.Hide(entity.gameObject);
            Random.state = randomState;
        }
        yield return null;
    }

    public static IEnumerator ValidateJoeReuse(Action<string> record)
    {
        Require(Application.isEditor || Debug.isDebugBuild, "Regression checks require Editor or Development Build.");
        Joe current = null;
        try
        {
            for (int show = 0; show < 3; show++)
            {
                current = CreateHero<Joe>("Joe", true);
                current.GetComponent<Status>().ResetRespwanData();
                current.ULT1();
                current.ULT1();
                Require(CountJoeAttackHandlers(current) == 1,
                    "Joe must own exactly one attack handler after repeated ULT1 initialization.");
                LunaEntities.Hide(current.gameObject);
                Require(CountJoeAttackHandlers(current) == 0,
                    "Joe attack handler survived native Entity hide.");
                current = null;
                yield return null; // Allow the native reusable pool to recycle before the next show.
            }
            record?.Invoke("Joe native show/hide repeated three times: exactly one attack callback after repeated rank initialization, zero after each hide.");
        }
        finally
        {
            if (current != null) LunaEntities.Hide(current.gameObject);
        }
    }

    private static int CountJoeAttackHandlers(Joe hero)
    {
        var animation = hero.transform.Find("Sprite").GetComponent<Spine.Unity.SkeletonAnimation>();
        var callbacks = (Delegate)Field(typeof(Spine.AnimationState), "Start").GetValue(animation.AnimationState);
        int count = 0;
        if (callbacks != null)
            foreach (Delegate callback in callbacks.GetInvocationList())
                if (ReferenceEquals(callback.Target, hero)) count++;
        return count;
    }

    private static void CheckRoseDebuff(Action<string> record)
    {
        bool original = Defined.SkillTrigger_Rose;
        try
        {
            Defined.SkillTrigger_Rose = false;
            Status enemy = CreateHero<Lene>("Lene").GetComponent<Status>();
            Status ally = CreateHero<Lene>("Lene").GetComponent<Status>();
            enemy.UnitSide = eUnitSide.Enemy;
            ally.UnitSide = eUnitSide.Allied;
            float enemyCoefficient = enemy.coefficient.AttackDamage;
            float allyCoefficient = ally.coefficient.AttackDamage;
            float enemyDamage = enemy.stat.AttackDamage;
            Defined.SkillTrigger_Rose = true;
            enemy.ConfirmSkillTriger();
            ally.ConfirmSkillTriger();
            Require(Mathf.Approximately(enemy.coefficient.AttackDamage, enemyCoefficient - 0.1f) &&
                Mathf.Approximately(ally.coefficient.AttackDamage, allyCoefficient),
                "Rose must debuff the enemy without changing the ally attack coefficient.");
            Defined.SkillTrigger_Rose = false;
            enemy.ConfirmSkillTriger();
            ally.ConfirmSkillTriger();
            Require(Mathf.Approximately(enemy.coefficient.AttackDamage, enemyCoefficient) &&
                Mathf.Approximately(enemy.stat.AttackDamage, enemyDamage) &&
                Mathf.Approximately(ally.coefficient.AttackDamage, allyCoefficient),
                "Rose must restore the same enemy's attack coefficient and computed damage without buffing allies.");
            record?.Invoke("Rose global debuff toggle: enemy attack coefficient and computed damage restore exactly; ally coefficient remains unchanged.");
        }
        finally
        {
            Defined.SkillTrigger_Rose = original;
        }
    }

    public static void ValidateEquipmentSlots(UISkill ui)
    {
        Require(Application.isEditor || Debug.isDebugBuild, "Regression checks require Editor or Development Build.");
        var skills = (IList)Field(typeof(UISkill), "equipDesignSkillDatas").GetValue(ui);
        var buttons = new HashSet<GameObject>();
        for (int i = 0; i < skills.Count; i++)
        {
            var info = (UISkill.SkillInfo)skills[i];
            Require(info.skillButton != null && info.skillButton.name == "Equip_" + i && buttons.Add(info.skillButton),
                "Equipped skill " + i + " does not own its matching distinct slot.");
        }
        Require(skills.Count == LunaDataTable.Instance.GetCurrentEquipDesignItemData().equipSkills.Count,
            "Equipped skill slot count does not match the current design.");
    }

    private static T CreateHero<T>(string name, bool reusable = false) where T : HeroSkillControl
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/01Heroes/" + name);
        Require(prefab != null, "Missing hero fixture " + name + ".");
        GameObject actor = LunaEntities.Spawn(prefab, new Vector3(5000f, 0f, 0f), Quaternion.identity, reusable: reusable);
        BaseAI ai = actor.GetComponent<BaseAI>();
        Require(ai != null, "Hero fixture lacks AI: " + name + ".");
        ai.EnsureEntityInitialized();
        T hero = actor.GetComponent<T>();
        Require(hero != null, "Hero fixture lacks skill module: " + name + ".");
        hero.EnsureEntityInitialized();
        return hero;
    }

    private static void CheckLeneChance(Lene hero, Action rank, float expected)
    {
        hero.GetComponent<Status>().ResetRespwanData();
        rank();
        Require(Mathf.Approximately((float)Field(typeof(Lene), "selfProcChance").GetValue(hero), expected),
            "Lene rank probability expected " + expected + ".");
    }

    private static void CheckSummonGate(HeroSkillControl hero, Action<string> record)
    {
        Type type = hero.GetType();
        Status status = hero.GetComponent<Status>();
        Action[] ranks = { hero.STAR5, hero.ULT1, hero.ULT2 };
        for (int i = 0; i < ranks.Length; i++)
        {
            status.ResetRespwanData();
            ranks[i]();
            status.unitState.IsLobbyMove = false;
            Field(type, "counter").SetValue(hero, 100f);
            type.GetMethod("OnEntityUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hero, new object[] { 0f, 0f });
            var summon = (GameObject)Field(type, "curMinion").GetValue(hero);
            if (i < 2) Require(summon == null, type.Name + " unlocked its ultimate summon before ULT2.");
            else
            {
                Require(summon != null && LunaEntities.IsSpawned(summon), type.Name + " did not summon at ULT2.");
                GameObject prefab = (GameObject)type.GetField("UltMinion").GetValue(hero);
                Require(summon.GetComponent<Status>().objectData == prefab.GetComponent<Status>().objectData,
                    type.Name + " ultimate spawned the wrong unit data.");
                LunaEntities.Hide(summon);
            }
        }
        record?.Invoke(type.Name + " real update: no ultimate at STAR5/ULT1; correct native summon at ULT2 after elapsed cooldown.");
    }

    private static int FindCurseSeed()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            Random.InitState(seed);
            Random.Range(0f, 0.5f); // Sequence delay consumes the first draw.
            Random.Range(1f, 100f); // Attack checks the target's zero boss-reflection probability.
            if (Random.Range(1f, 100f) <= 80f) return seed;
        }
        throw new InvalidOperationException("Hero rank regression: deterministic curse seed not found.");
    }

    private static void CheckCurse(Scalet hero, Status target, Action rank, int seed, bool expected)
    {
        hero.GetComponent<Status>().ResetRespwanData();
        rank();
        target.stat.Armour = 100f;
        target.stat.MoveSpeed = 5f;
        target.stat.DebuffAvoidance = 0f;
        Field(typeof(Scalet), "skillDamage").SetValue(hero, 0f); // Isolate curse from damage/death and floating text.
        Random.InitState(seed);
        var routine = (IEnumerator)typeof(Scalet).GetMethod("SequanceSkill", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(hero, new object[] { target.gameObject });
        Require(routine.MoveNext(), "Scarlet skill must begin with its authored animation delay.");
        Require(!routine.MoveNext(), "Scarlet skill sequence did not complete after the delay.");
        Require(Mathf.Approximately(target.stat.Armour, expected ? 80f : 100f) &&
            Mathf.Approximately(target.stat.MoveSpeed, expected ? 4f : 5f), "Scarlet curse rank behavior is incorrect.");
    }

    private static FieldInfo Field(Type type, string name)
    {
        FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        Require(field != null, "Missing verification field " + type.Name + "." + name + ".");
        return field;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Hero rank regression: " + message);
    }
}
#endif
