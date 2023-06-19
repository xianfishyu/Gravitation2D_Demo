using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class BodyInit : MonoBehaviour
{
    [Range(500000f, 3000000f)]
    public float mainBodyMass;
    [Range(1f, 300f)] public float mainBodyDensity = 10f;
    [Range(1, 10000)] public int geneNumber = 10;

    private static float planetMinPos;
    [Range(1f, 10000f)] public float planetMaxPos = 300f;

    public GameObject sun = BodyTools.sun;
    public Vector3 mainBodyPos = new(0, 0, 0);
    public List<(GameObject planet, BodyBehavior behavior)> bodyList = new();

    [Range(1f, 12f)] public float geneSpeed = 1f;
    public float G = 1f;
    [Range(-10f, 10f)] public float calcSpeed = 1f;

    [SerializeField]
    public ShaderCal shaderCal;

    [Range(1, 500)] public int minMass, maxMass;
    [Range(1, 10)] public int minDen, maxDen;

    private float sunDiam;
    [System.NonSerialized]
    public bool generated = false;
    public bool isReGene = true;
    public Color sunColor;
    public Mutex mut = new Mutex();

    void Start()
    {
        UnsafeUtility.SetLeakDetectionMode(Unity.Collections.NativeLeakDetectionMode.Enabled);
        BodyTools.body = Resources.Load<GameObject>("Prefabs/Body");
        ShaderCal.GPUPosVelCal = Resources.Load<ComputeShader>("GPUPosVelCal");

        ArgumentsUpdate();

        Sprite sunSprite = Resources.Load<Sprite>("Textures/Sun");
        sun = BodyTools.SunInit(sunColor, mainBodyPos, mainBodyMass, mainBodyDensity, sunSprite);
        BodyBehavior sunBehavior = sun.GetComponent<BodyBehavior>();
        sunBehavior.bodyInit = this;
        sunDiam = sunBehavior.diam;

        planetMinPos = sunDiam + 20f;
        BodyTools.GenerateRange(planetMinPos, planetMaxPos);

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

        Physics.sleepThreshold = 0.001f;
    }

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

    public void UpdateStar()
    {
        if (shaderCal.isReady) shaderCal.PosVelUpdate();
    }

    void FixedUpdate()
    {
        ArgumentsUpdate();

        if (bodyList.Count < geneNumber && isReGene)
        {
            if (UnityEngine.Random.Range(0f, 1f) > (1f / ((float)geneSpeed + .005f)))
            {
                GameObject planet = BodyTools.PlanetInit();
                BodyBehavior behavior = planet.GetComponent<BodyBehavior>();
                behavior.bodyInit = this;
                bodyList.Add((planet, behavior));
            }
        }

        BodyTools.G = G;
    }

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
