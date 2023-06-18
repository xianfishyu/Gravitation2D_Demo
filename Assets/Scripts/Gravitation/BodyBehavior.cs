using System;
using Unity.Mathematics;
using UnityEngine;

public class BodyBehavior : MonoBehaviour
{
    public Vector3 pos;
    public Vector3 vel;
    public float diam, mass, density;
    public bool mainBody;
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
        int ret = bodyInit.bodyList.FindIndex(it => it.behavior.guid == this.guid);
        if (ret != -1)
        {
            bodyInit.bodyList.RemoveAt(ret);

            // 重新计算
            bodyInit.shaderCal = new(bodyInit.bodyList.ToArray());
            bodyInit.shaderCal.bodyInit = bodyInit;
            //bodyInit.gPUCollitionCal = new(bodyInit.bodyList.ToArray());
        }
        endtime = trailRender.time / 5f;
        time = 0;
        // trailRender.enabled = false;
    }

    private void OnDestroy() {
        int ret = bodyInit.bodyList.FindIndex(it => it.behavior.guid == this.guid);
        if (ret != -1)
        {
            bodyInit.bodyList.RemoveAt(ret);

            bodyInit.shaderCal = new(bodyInit.bodyList.ToArray());
            //bodyInit.gPUCollitionCal = new(bodyInit.bodyList.ToArray());
            bodyInit.shaderCal.bodyInit = bodyInit;
        }
    }

    public void InitInformation(Vector3 vel, float diam, float mass, float density, Color color)
    {
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

        if (mainBody) {
            trailRender.enabled = false;
        }

    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (pos.magnitude > 9000) { this.StartDestroy(); }
        diam = BodyTools.StarDiam(mass, density);
        transform.localScale = new Vector3(diam, diam, 1f);

        if(mainBody)
        {
            BodyTools.mainBodyMass = mass;
        }

        if (endtime > 0) {
            if (time >= endtime) {
                UnityEngine.Object.Destroy(gameObject);
            } else {
                time += Time.deltaTime;
                float percent = 1.0f - (time / endtime);
                trailRender.startColor = color * 0.7f * percent;
                trailRender.endColor = color * 0.3f * percent;
                spriteRenderer.color = spriteRenderer.color * percent;
            }
        }

        IFVelToSlow();

    }

    void OnTriggerEnter2D(Collider2D other)
    //public void Trigger(GameObject other)
    {
        //Debug.Log("Entered trigger with: " + gameObject.name + " and " + other.name);
        GameObject thatGameObject = other.gameObject;
        BodyBehavior thatBody = thatGameObject.GetComponent<BodyBehavior>();
        if (mainBody) {
            this.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
            this.mass = Mathf.Min(thatBody.mass + this.mass, 3000000f);
            thatBody.StartDestroy();
        }
        else if(thatBody.mainBody)
        {
            thatBody.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
            thatBody.mass = Mathf.Min(thatBody.mass + this.mass, 3000000f);
            this.StartDestroy();
        }
        else
        {
            if (this.mass > thatBody.mass)
            {
                this.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
                this.mass = Mathf.Min(thatBody.mass + this.mass, 300000f);
                thatBody.StartDestroy();
            }
        }
    }

    void IFVelToSlow()
    {
        float time = 100f * Mathf.Pow(1.1f,-0.2f * vel.magnitude) / 10f;
        //float time = Mathf.Pow(300f * Mathf.Pow(1.05f,-1.9f * vel.magnitude),2f) + 2f;
        trailRender.time = MathF.Min(Mathf.Max(time, 1f), 20f);
        //trailRender.time = 100f;
    }

}
