using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;


public class Arrow : MonoBehaviour
{
    enum eArrowPath
    {
        Begin = 0,
        Middle = 1,
        End = 2,
    }

    private float moveTime = 2.0f;
    private float attackRange = 800.0f;
    private float height = 250.0f;

    private float destroyTime = 5.0f;

    private Vector3[] path = new Vector3[3];
    // Start is called before the first frame update

    private GameObject shooterObject;
    public GameObject ShooterObject
    {
        get { return shooterObject; }
        set { shooterObject = value; }
    }

    public float AttackRange
    {
        get { return attackRange; }
        set { attackRange = value; }
    }
    
    public Vector3 PathEnd
    {
        get { return path[2]; }
        set { path[2] = value; }
    }

    private void ResetTarget()
    {
        for (int i = 0; i < 3; i++)
        {
            path[i] = new Vector3();
        }
    }

    private void MoveArrow()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");

        foreach (GameObject enemy in enemies)
        {
            float distance = enemy.GetComponent<Transform>().position.x - shooterObject.GetComponent<Transform>().position.x;

            if (0 < distance && shooterObject.GetComponent<Status>().stat.AttackRange >= distance)
            {
                path[2] = enemy.transform.position;

                break;
            }
        }

        path[0] = transform.position;
        //path[2] = transform.position + new Vector3(attackRange, 0 ,0);
        path[1] = Vector3.Lerp(path[0], path[2], 0.5f);
        path[1].y += height;


        float rotateDegree = Mathf.Acos(Vector3.Dot(path[0].normalized, path[1].normalized));
        //float degree = Mathf.Acos(Vector3.Dot(Vector3.right.normalized, Vector3.up.normalized));

        rotateDegree = (Mathf.PI - (rotateDegree * 2)) * -Mathf.Rad2Deg;

        iTween.RotateTo(gameObject, iTween.Hash("z", rotateDegree, "time", moveTime));
        iTween.MoveTo(gameObject, iTween.Hash("path", path, "time", moveTime, "easeType", "easeOutCirc"));
    }

    private void OnEnable()
    {
        ResetTarget();
        MoveArrow();
    }

    private void OnDisable()
    {
        
    }

    void Start()
    {
        ResetTarget();
        MoveArrow();
    }

    // Update is called once per frame
    void Update()
    {
        if(destroyTime < 0.0f)
        {
            DestroyArrow();
            destroyTime = 5.0f;
        }

        destroyTime -= Time.deltaTime;

        Vector3 right = transform.TransformDirection(Vector3.right) * 100;
        Debug.DrawRay(transform.position, right, Color.green);
    }

    public void DestroyArrow()
    {
        PoolManager.ReleaseObject(this.gameObject);
    }
}
