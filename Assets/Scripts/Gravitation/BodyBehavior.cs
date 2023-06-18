using System;
using UnityEngine;

public class BodyBehavior : MonoBehaviour
{
    public Vector3 pos;
    public Vector3 vel;
    public float diam, mass, density;
    public bool mainBody;
    public BodyInit bodyInit;
    public Guid guid = Guid.NewGuid();
    bool isReady = false;

    


    public void Destroy()
    {
        int ret = bodyInit.bodyList.FindIndex(it => it.behavior.guid == this.guid);
        if (ret != -1)
        {
            bodyInit.bodyList.RemoveAt(ret);

            // 重新计算
            float dt = bodyInit.shaderCal.dt;
            float G = bodyInit.shaderCal.G;
            bodyInit.shaderCal = new(bodyInit.bodyList.ToArray());
            bodyInit.shaderCal.dt = dt;
            bodyInit.shaderCal.G = G;
        }
        UnityEngine.Object.Destroy(gameObject);
    }

    public void InitInformation(Vector3 vel, float diam, float mass, float density)
    {
        this.vel = vel;
        this.diam = diam;
        this.mass = mass;
        this.density = density;
        this.isReady = true;
    }

    private void Start()
    {
        if(mainBody)
            GetComponent<TrailRenderer>().autodestruct = false;
    }

    private void Update()
    {
        if (pos.magnitude > 9000) { this.Destroy(); }
        diam = BodyTools.StarDiam(mass, density);
        transform.localScale = new Vector3(diam, diam, 1f);

        if(mainBody)
        {
            BodyTools.mainBodyMass = mass;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject thatGameObject = collision.gameObject;
        BodyBehavior thatBody = thatGameObject.GetComponent<BodyBehavior>();
        if (mainBody) {
            this.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
            this.mass = Mathf.Min(thatBody.mass + this.mass, 3000000f);
            thatBody.Destroy();
        }
        else if(thatBody.mainBody)
        {
            thatBody.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
            thatBody.mass = Mathf.Min(thatBody.mass + this.mass, 3000000f);
            this.Destroy();
        }
        else
        {
            if (this.mass > thatBody.mass)
            {
                this.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
                this.mass = Mathf.Min(thatBody.mass + this.mass, 100000f);
                thatBody.Destroy();
            }
            else
            {
                thatBody.vel = ((this.mass * this.vel) + (thatBody.mass * thatBody.vel)) / (this.mass + thatBody.mass);
                thatBody.mass = Mathf.Min(thatBody.mass + this.mass, 100000f);
                this.Destroy();
            }
        }
    }
}
