using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BodyBehavior : MonoBehaviour
{
    public Vector3 pos; //位置
    public Vector3 vel; //速度
    public float diam, mass, density;   //直径/质量/密度
    public bool mainBody = false;   //是否为恒星?
    public BodyInit bodyInit;
    public Guid guid = Guid.NewGuid();  //UUID
    //bool isReady = false;

    //销毁参数
    float endtime = 0;
    float time = 0;
    Color color;    //星体颜色

    TrailRenderer trailRender;
    SpriteRenderer spriteRenderer;

    //开始销毁
    public void StartDestroy()
    {
        if (this == null) { return; }
        if (mainBody) { Debug.LogError("日被销毁了？"); }
        bodyInit.RemoveStar(guid);
        endtime = 1f;//trailRender.time / 5f;
        time = 0;
    }

    //初始化信息
    public void InitInformation(Vector3 pos, Vector3 vel, float diam, float mass, float density, Color color)
    {
        //传入参数
        this.pos = pos;
        this.vel = vel;
        this.diam = diam;
        this.mass = mass;
        this.density = density;
        //this.isReady = true;
        this.color = color;

        trailRender = GetComponent<TrailRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //尾迹颜色
        trailRender.startColor = color * 0.7f;
        trailRender.endColor = color * 0.3f;

        //当为恒星时,关闭尾迹
        if (mainBody)
        {
            trailRender.enabled = false;
        }

    }

    private void Start(){}

    private void Update()
    {
        //更新位置/直径
        gameObject.transform.position = pos;
        gameObject.transform.localScale = new Vector3(diam, diam, 1f);

        //如果是恒星,把恒星质量传入工具类
        if (mainBody)
        {
            BodyTools.mainBodyMass = mass;
        } 
        //如果不是,销毁处理/更新
        else
        {
            trailRender.enabled = bodyInit.enableTrail;
            if (endtime > 0)
            {
                if (time >= endtime)
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
                else
                {
                    time += Time.fixedDeltaTime;
                    float percent = 1.0f - (time / endtime);
                    trailRender.startColor = color * 0.7f * percent;
                    trailRender.endColor = color * 0.3f * percent;
                    spriteRenderer.color = spriteRenderer.color * percent;
                }
            } else if (pos.magnitude > 10000) { this.StartDestroy(); }
            //尾迹参数更新
            TrailUpdate();
        }

    }

    //触发器
    public void Trigger(GameObject other)
    {
        if (other == null) return;
        // Debug.Log("Entered trigger with: " + gameObject.name + " and " + other.name);
        GameObject thatGameObject = other.gameObject;
        BodyBehavior thatBody = thatGameObject.GetComponent<BodyBehavior>();

        if (mainBody && bodyInit.enableSunMove)
        {
            this.mass = Mathf.Min(thatBody.mass + this.mass, 10000000f);
            this.diam = BodyTools.StarDiam(mass, density);
            thatBody.StartDestroy();
        }
        else if (thatBody.mainBody && bodyInit.enableSunMove)
        {
            thatBody.mass = Mathf.Min(thatBody.mass + this.mass, 10000000f);
            thatBody.diam = BodyTools.StarDiam(thatBody.mass, thatBody.density);
            this.StartDestroy();
        }
        else
        {
            if (bodyInit.enableCollision) {
                if (this.mass > thatBody.mass)
                {
                    this.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
                    this.mass = Mathf.Min(thatBody.mass + this.mass, 2000000f);
                    this.diam = BodyTools.StarDiam(mass, density);
                    thatBody.StartDestroy();
                }
            }
        }

    }

    void TrailUpdate()
    {
        //宽度更新
        trailRender.startWidth = diam * 0.3f;

        //拖尾更新
        //float time = 100f * Mathf.Pow(1.1f, -0.2f * vel.magnitude) / 10f;
        float time = Mathf.Pow(300f * Mathf.Pow(1.05f,-1.9f * vel.magnitude),2f) + 2f;
        trailRender.time = MathF.Min(Mathf.Max(time, 1f), 20f) + (trailRender.startWidth * 2f);
        //trailRender.time = 100f;

    }

}
