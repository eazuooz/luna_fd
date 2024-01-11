using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;
using Spine.Unity;

public class MartinKing : MonoBehaviour
{
    public Status status { get; set; }
    private int skillControlCounter;

    public GameObject skillEffect_1;

    private SkeletonAnimation animation;
    private bool isCharge = false;

    // Start is called before the first frame update
    void Start()
    {
        status = GetComponent<Status>();
        status.unitState.IsTargetingSkill = false;
        skillControlCounter = 0;

        animation = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        animation.AnimationState.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "skill")
            {

            }
            else if (trackEntry.Animation.Name == "boom")
            {
                StartCoroutine(Boom());



                //Destroy(stage);

                //status.death = true;
            }
            else if (trackEntry.Animation.Name == "bomm ready")
            {

            }

        };

    }

    // Update is called once per frame
    void Update()
    {
        if (status.unitState.IsSkillActivaion == true
            && isCharge == false)
        {
            //status.skillOn = false;
            isCharge = true;
            Skill();
        }
    }

    void Skill()
    {
        if (status.UnitSide == eUnitSide.Enemy)
            StartCoroutine(Skill_1());
        //else
        //    StartCoroutine(Skill_2());
    }

    IEnumerator Boom()
    {
        transform.Find("Sprite").gameObject.SetActive(false);

        for (int i = 0; i < 50; i++)
        {
            float randX = Random.RandomRange(-20.0f, 0.0f);
            float randY = Random.RandomRange(-0.5f, 2.0f);

            Vector3 createPos = gameObject.transform.position;
            createPos.x += randX;
            createPos.y += randY;

            Vector3 size = new Vector3(2.0f, 2.0f, 1.0f);
            Defined.EffectCreate(skillEffect_1, createPos, size);

            yield return new WaitForSeconds(0.1f);
        }

        GameObject stage = Game.Instance.lunaSceneManager.CurrentStage;
        stage.GetComponent<BasicDungeon>().gameAllyTower.GetComponent<Status>().unitState.IsDeath = true;

        yield return null;
    }

    IEnumerator Skill_1()
    {

        if (animation.AnimationName == "skill")
        {
            animation.AnimationState.SetAnimation(0, "skill", true);
        }

        yield return new WaitForSeconds(40.0f);

        animation.AnimationState.SetAnimation(0, "boom ready", true);


        yield return new WaitForSeconds(20.0f);

        animation.AnimationState.SetAnimation(0, "boom", false);


        yield return null;
    }

    IEnumerator Skill_2()
    {
        float bleedDamage = 100.0f;

        GameObject tmp;

        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(0.3f);

            tmp = Defined.EffectCreate(skillEffect_1, gameObject.transform.position + new Vector3(i * 2.0f + 2.0f, 0, 0));

            Vector3 tmpScale = tmp.transform.Find("Sprite").transform.localScale;
            tmpScale.x *= -1;
            tmp.transform.Find("Sprite").transform.localScale = tmpScale;

            IdentifyFunc.PickingUnits((unit) =>
            {
                if ((IdentifyFunc.IsInDistance(tmp, unit, 2.0f)))
                {
                    unit.GetComponent<Status>().TakeBleeding(bleedDamage, 5.0f);
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }
}
