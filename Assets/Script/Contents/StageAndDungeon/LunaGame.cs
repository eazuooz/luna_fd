using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LunaGame : MonoBehaviour
{
    public string stageName;

    public DungeonTable dungeonTable;
    public GameObject backgroundImage;
    //public GameObject player;

    public int rewardGold;

    public bool isStageStop { get; set; }
    protected GameObject stageCamera;

    public bool isStageWin { get; set; }
    public float StageCount { get; set; }

    private bool attachCamera;
    private bool followCamera;
    public bool AttachCamera { get { return attachCamera; } set { attachCamera = value; } }
    public bool FollowCamera { get { return followCamera; } set { followCamera = value; } }

    protected Vector3 holdPosition;
    private bool shakeOn;
    public bool ShakeOn { get { return shakeOn; } set { shakeOn = value; } }

    public void ShakeCameraOn(int duration)
    {
        if (shakeOn == false)
        {
            StartCoroutine(ShakeCamera(duration));
        }
    }


    public virtual void Init()
    {
        LunaDataTable.Instance.playerData.RewardGold = 0;
        // 글로벌스킬변수초기화 <- 리팩토링필수!!
        Defined.SkillTrigger_Roni = false;
        Defined.SkillTrigger_Rose = false;
        Defined.SkillTrigger_Roen1 = false;
        Defined.SkillTrigger_Roen2 = false;
        Defined.SkillTrigger_Roen3 = false;
        Defined.SkillTrigger_Elena = false;
        Defined.SkillTrigger_Pochi = false;
        Defined.SkillTrigger_Goldy = 0.0f;
    }

    public IEnumerator CameraMoveCheck(float Coef)
    {
        while (true)
        {
            //InfiniteLoopDetector.Run();
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 touchPos = Input.mousePosition;

                if (touchPos.y > Screen.height * 0.2f && touchPos.y < Screen.height * 0.9f)
                {
                    holdPosition = touchPos;

                }
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 touchPos = Input.mousePosition;

                if (touchPos.y > Screen.height * 0.2f && touchPos.y < Screen.height * 0.9f)
                {
                    AttachCamera = false;

                    float dis = touchPos.x - holdPosition.x;

                    if (dis > 0)
                    {
                        if (stageCamera.transform.position.x > -Coef)
                        {
                            stageCamera.transform.Translate(Vector2.left * dis * Time.deltaTime);
                        }
                    }
                    else
                    {
                        if (stageCamera.transform.position.x < Coef)
                        {
                            stageCamera.transform.Translate(Vector2.left * dis * Time.deltaTime);
                        }
                    }

                    holdPosition = touchPos;
                }
            }

            yield return null;
        }
    }


    public IEnumerator ShakeCamera(int duration)
    {
        AttachCamera = false;

        shakeOn = true;

        for (int i = 0; i < 2 * duration; i++)
        {
            stageCamera.transform.position += Vector3.left * 0.1f;

            yield return new WaitForSeconds(0.015f);

            stageCamera.transform.position += Vector3.right * 0.1f;

            yield return new WaitForSeconds(0.015f);
        }

        shakeOn = false;
    }

}