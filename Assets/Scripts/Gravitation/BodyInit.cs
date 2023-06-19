using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class BodyInit : MonoBehaviour
{
    [Range(500000f, 3000000f)]
    public float mainBodyMass;  //恒星质量
    [Range(1f, 100f)]
    public float mainBodyDensity = 10f;   //恒星密度
    [Range(1, 10000)]
    public int geneNumber = 10;   //生成数量

    private static float planetMinPos;  //行星生成最小距离
    [Range(1f, 10000f)]
    public float planetMaxPos = 300f;   //行星生成最大距离

    public GameObject sun = BodyTools.sun;  //获取恒星
    public Vector3 mainBodyPos = new(0, 0, 0);  //恒星初始位置
    public List<(GameObject planet, BodyBehavior behavior)> bodyList = new();   //星体列表

    [Range(1f, 12f)]
    public float geneSpeed = 1f;    //后续生成的行星生成速度
    public float G;    //引力常量
    [Range(0.01f, 10f)]
    public float calcSpeed = 1f;    //步长

    [SerializeField]
    public ShaderCal shaderCal;

    [Range(1, 500)]
    public int minMass, maxMass;    //行星最小/大质量
    [Range(1, 10)]
    public int minDen, maxDen;  //行星最小/大密度

    private float sunDiam;  //恒星直径
    [System.NonSerialized]
    public bool generated = false;
    public bool isReGene = true;    //是否后续生成?
    public Color sunColor;  //恒星颜色
    public Mutex mut = new Mutex();

    //开始
    void Start()
    {
        UnsafeUtility.SetLeakDetectionMode(Unity.Collections.NativeLeakDetectionMode.Enabled);

        //加载资源并读取到工具类中
        BodyTools.body = Resources.Load<GameObject>("Prefabs/Body");
        ShaderCal.GPUPosVelCal = Resources.Load<ComputeShader>("GPUPosVelCal");
        Sprite sunSprite = Resources.Load<Sprite>("Textures/Sun");
        
        //参数初始化
        ArgumentsUpdate();

        //生成恒星
        sun = BodyTools.SunInit(sunColor, mainBodyPos, mainBodyMass, mainBodyDensity, sunSprite);
        BodyBehavior sunBehavior = sun.GetComponent<BodyBehavior>();
        sunBehavior.bodyInit = this;
        sunDiam = sunBehavior.diam;

        //计算并写入行星生成最小/大距离
        planetMinPos = sunDiam + 20f;
        BodyTools.GenerateRange(planetMinPos, planetMaxPos);

        //将星体写入数组
        bodyList.Add((sun, sunBehavior));
        for (int number = 0; number < geneNumber; number++)
        {
            GameObject planet = BodyTools.PlanetInit();
            BodyBehavior behavior = planet.GetComponent<BodyBehavior>();
            behavior.bodyInit = this;
            bodyList.Add((planet, behavior));
        }

        shaderCal = new(this);
        generated = true;
    }

    //移除行星
    public void RemoveStar(Guid guid)
    {
        int ret = bodyList.FindIndex(it => it.behavior.guid == guid);
        if (ret != -1)
        {
            bodyList.RemoveAt(ret);
            // 重新计算
            shaderCal = new(this);
        }
    }

    //当准备好时,计算并更新星体位置,被BodyUpdate引用
    public void UpdateStar()
    {
        if (shaderCal.isReady) shaderCal.PosVelUpdate();
    }

    //更新
    void FixedUpdate()
    {
        //参数更新
        ArgumentsUpdate();
        
        //如果后续生成为true以及现存星体数量不足初始生成数量,生成新的行星
        if (bodyList.Count < geneNumber && isReGene)
        {
            if (UnityEngine.Random.Range(0f, 1f) > (1f / ((float)geneSpeed + .005f)))
            {
                GameObject planet = BodyTools.PlanetInit();
                BodyBehavior behavior = planet.GetComponent<BodyBehavior>();
                behavior.bodyInit = this;
                bodyList.Add((planet, behavior));
                shaderCal = new(this);
            }
        }
    }

    /// <summary>
    /// 参数更新
    /// </summary>
    private void ArgumentsUpdate()
    {
        BodyTools.mainBodyMass = mainBodyMass;
        BodyTools.mainBodyPos = mainBodyPos;
        BodyTools.G = G;
        BodyTools.minMass = minMass;
        BodyTools.maxMass = maxMass;
        BodyTools.minDen = minDen;
        BodyTools.maxDen = maxDen;
    }
}
