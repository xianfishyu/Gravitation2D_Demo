using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior : MonoBehaviour
{
    public int mass;
    public float density;
    public float radius;
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 Acc;
    public Color col;

    private float dt = 0.001f;
    public int mainAstar; //if == 1
    public float G = 0.01f;
    public int mainStarMass = 2000000;
    LimitedQueue<Vector3> posQueue = new LimitedQueue<Vector3>(100);
    int maximum = 100;
    int minimum = 3;

    void Start()
    {
        //获取组件
        Transform transform = GetComponent<Transform>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        TrailRenderer trailRenderer = GetComponent<TrailRenderer>();

        //初始化
        Initialization(transform,spriteRenderer,trailRenderer);
    }

    void FixedUpdate()
    {
        //获取组件
        Transform transform = GetComponent<Transform>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        TrailRenderer trailRenderer = GetComponent<TrailRenderer>();

        //星体数据更改(debug)
        AstarUpdate(transform,spriteRenderer);

        //坐标更新
        if(mainAstar == 0)
        {
            //PositionUpdate(transform,spriteRenderer);
            //UpdatePositionAndVelocity();
            NewUpdatePositionAndVelocity();
        }

        //碰撞检测
        Collision();

        //距离过远销毁
        Eliminate();
    }

//初始化
    void Initialization(Transform transform,SpriteRenderer spriteRenderer,TrailRenderer trailRenderer)
    {
        if(mainAstar == 0)
        {
            mass = Random.Range(10,1000);
            density = Random.Range(100,200);
            col = new Color(Random.Range(0.1f,1.0f), Random.Range(0.1f,1.0f), Random.Range(0.1f,1.0f));
            pos = new Vector3(Random.Range(minimum,maximum) * RandomNegation(),Random.Range(minimum,maximum) * RandomNegation(),0);
            Vector3 direction = Vector3.Cross(pos,new Vector3(0,0,-1));
            vel = Mathf.Pow(mainStarMass*G/pos.magnitude,1.0f/2.0f) * direction.normalized;
        }
        if(mainAstar == 1)
        {
            mass = mainStarMass;
            density = 2000;
            col = Color.yellow;
            pos = new Vector3(0,0,0);
            vel = new Vector3(0,0,0);
        }

        AstarUpdate(transform,spriteRenderer);
        Line(pos,trailRenderer);
    }
    void AstarUpdate(Transform transform,SpriteRenderer spriteRenderer)
    {
        radius = Mathf.Pow(3*mass/4/Mathf.PI/density,1.0f/3.0f);
        transform.localScale = new Vector3(radius,radius,1);
        spriteRenderer.color = col;
        transform.position = pos;
    }

//旧有的坐标更新
    void PositionUpdate(Transform transform,SpriteRenderer spriteRenderer)
    {
        pos = pos + vel*dt;
        transform.position = pos;
        Acc = GetAcc(); 
        vel = vel + Acc*dt;
    }
    Vector3 GetAcc()
    {
        GameObject[] astarList = GameObject.FindGameObjectsWithTag("Astar");
        Acc = new Vector3 (0,0,0);
        foreach (GameObject aster in astarList)
        {
            if(aster == gameObject)
            {
                continue;
            }
            Vector3 r = aster.transform.position - pos;
            Behavior b = aster.GetComponent<Behavior>();
            Acc = Acc +(G * b.mass * r /(r.magnitude * r.magnitude * r.magnitude)); 
        }

        return Acc;
    }
    
    int RandomNegation()
    {
        int random = Random.Range(0, 2) * 2 - 1;
        return random;
    }


    


// 四阶龙格-库塔法计算位置和速度
    void NewUpdatePositionAndVelocity()
    {
        Acc = GetAcc();
        // 计算中间变量
        Vector3 k1 = vel * dt;
        Vector3 l1 = GetAccRK4(pos, vel) * dt;
        Vector3 k2 = (vel + 0.5f * l1) * dt;
        Vector3 l2 = GetAccRK4(pos + 0.5f * k1, vel + 0.5f * l1) * dt;
        Vector3 k3 = (vel + 0.5f * l2) * dt;
        Vector3 l3 = GetAccRK4(pos + 0.5f * k2, vel + 0.5f * l2) * dt;
        Vector3 k4 = (vel + l3) * dt;
        Vector3 l4 = GetAccRK4(pos + k3, vel + l3) * dt;

        // 计算位置和速度
        pos = pos + 1.0f / 6.0f * (k1 + 2.0f * k2 + 2.0f * k3 + k4);
        vel = vel + 1.0f / 6.0f * (l1 + 2.0f * l2 + 2.0f * l3 + l4);
    }

    Vector3 GetAccRK4(Vector3 pos,Vector3 vel)
    {
        GameObject[] astarList = GameObject.FindGameObjectsWithTag("Astar");
        Acc = new Vector3 (0,0,0);
        foreach (GameObject aster in astarList)
        {
            if(aster == gameObject)
            {
                continue;
            }
            Vector3 r = aster.transform.position - pos;
            Behavior b = aster.GetComponent<Behavior>();
            Acc = Acc +(G * b.mass * r /(r.magnitude * r.magnitude * r.magnitude)); 
        }
        return Acc;
    }
//旧有的RK4
    void UpdatePositionAndVelocity()
    {
        Acc = GetAcc();
        // 计算中间变量
        Vector3 k1 = vel * dt;
        Vector3 l1 = Acc * dt;
        Vector3 k2 = (vel + 0.5f * l1) * dt;
        Vector3 l2 = Acc * dt;
        Vector3 k3 = (vel + 0.5f * l2) * dt;
        Vector3 l3 = Acc * dt;
        Vector3 k4 = (vel + l3) * dt;
        Vector3 l4 = Acc * dt;

        // 计算位置和速度
        pos = pos + 1.0f / 6.0f * (k1 + 2.0f * k2 + 2.0f * k3 + k4);
        vel = vel + 1.0f / 6.0f * (l1 + 2.0f * l2 + 2.0f * l3 + l4);
    }
    
//绘制轨迹

    void Line(Vector3 pos,TrailRenderer trailRenderer)
    {
        //posQueue.Enqueue(pos);
        //Vector3[] point = posQueue.ToArray();
        //trailRenderer.SetPositions(point);
        trailRenderer.startColor = col;
        trailRenderer.endColor = col;
    }

//碰撞检测
    void Collision()
    {
        GameObject[] astarList = GameObject.FindGameObjectsWithTag("Astar");
        foreach (GameObject aster in astarList)
        {
            if(aster == gameObject)
            {
                continue;
            }
            Behavior b = aster.GetComponent<Behavior>();
            Vector3 length = aster.transform.position - pos;
            float r = radius + b.radius - 3;
            if(length.magnitude < (r/2+2))
            {
                if(mass < b.mass)
                {
                    b.mass = b.mass + mass;
                    b.vel = (b.mass * b.vel + mass* vel)/(b.mass + mass);
                    Destroy(gameObject);
                }
            }
            
        }
    }

//距离过远销毁
    void Eliminate()
    {
        if(pos.magnitude > 400)
        {
            Destroy(gameObject);
        }
    }
}

public class LimitedQueue<T> : Queue<T>
{
    private int _limit;

    public int Limit
    {
        get { return _limit; }
        set
        {
            _limit = value;
            while (Count > _limit)
            {
                Dequeue();
            }
        }
    }

    public LimitedQueue(int limit)
    {
        _limit = limit;
    }

    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        while (Count > _limit)
        {
            Dequeue();
        }
    }
}
