using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BodyBehavior : MonoBehaviour
{
    public Vector3 pos;
    public Vector3 vel;
    public float diam, mass, density;
    public bool mainBody = false;
    public BodyInit bodyInit;
    public Guid guid = Guid.NewGuid();
    //bool isReady = false;
    float endtime = 0;
    float time = 0;
    Color color;

    TrailRenderer trailRender;
    SpriteRenderer spriteRenderer;

    public void StartDestroy()
    {
        if (endtime > 0) { return; }
        if (mainBody) { Debug.LogError("日被销毁了？"); }
        bodyInit.RemoveStar(guid);
        endtime = trailRender.time / 5f;
        time = 0;
    }

    private void OnDestroy()
    {
        // if (endtime > 0) return;
        // int ret = bodyInit.bodyList.FindIndex(it => it.behavior.guid == this.guid);
        // if (ret != -1)
        // {
        //     bodyInit.bodyList.RemoveAt(ret);
        //     bodyInit.shaderCal = new(this.bodyInit);
        // }
    }

    public void InitInformation(Vector3 pos, Vector3 vel, float diam, float mass, float density, Color color)
    {
        this.pos = pos;
        this.vel = vel;
        this.diam = diam;
        this.mass = mass;
        this.density = density;
        //this.isReady = true;
        this.color = color;

        trailRender = GetComponent<TrailRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        trailRender.startColor = color * 0.7f;
        trailRender.endColor = color * 0.3f;

        if (mainBody)
        {
            trailRender.enabled = false;
        }

    }

    private void Start()
    {

    }

    

    private void Update()
    {
        gameObject.transform.position = pos;
        gameObject.transform.localScale = new Vector3(diam, diam, 1f);
        if (mainBody)
        {
            BodyTools.mainBodyMass = mass;
        } 
        else
        {
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

            TrailUpdate();
        }

    }

    //void OnTriggerEnter2D(Collider2D other)
    public void Trigger(GameObject other)
    {
        if (other == null) return;
        // Debug.Log("Entered trigger with: " + gameObject.name + " and " + other.name);
        GameObject thatGameObject = other.gameObject;
        BodyBehavior thatBody = thatGameObject.GetComponent<BodyBehavior>();
        if (mainBody)
        {
            this.mass = Mathf.Min(thatBody.mass + this.mass, 500000000f);
            this.diam = BodyTools.StarDiam(mass, density);
            thatBody.StartDestroy();
        }
        else if (thatBody.mainBody)
        {
            thatBody.mass = Mathf.Min(thatBody.mass + this.mass, 500000000f);
            thatBody.diam = BodyTools.StarDiam(thatBody.mass, thatBody.density);
            this.StartDestroy();
        }
        else
        {
            if (this.mass > thatBody.mass)
            {
                this.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
                this.mass = Mathf.Min(thatBody.mass + this.mass, 20000000f);
                this.diam = BodyTools.StarDiam(mass, density);
                thatBody.StartDestroy();
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
