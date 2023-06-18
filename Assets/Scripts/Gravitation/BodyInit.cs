using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class BodyInit : MonoBehaviour
{
    [Range(500000f, 3000000f)]
    public float mainBodyMass;
    [Range(5f, 300f)] public float mainBodyDensity = 10f;
    [Range(1, 7500)] public int geneNumber = 10;
    private float G = 1f;

    private static float planetMinPos;
    [Range(1f, 3000f)] public float planetMaxPos = 300f;

    public GameObject sun = BodyTools.sun;
    public Vector3 mainBodyPos = new(0, 0, 0);
    public List<(GameObject planet, BodyBehavior behavior)> bodyList = new();

    [Range(1, 10)] public int geneSpeed = 1;


    [SerializeField]
    public ShaderCal shaderCal;

    private float sunDiam;
    [System.NonSerialized]
    public bool generated = false;
    public bool isReGene = true;
    public Color sunColor;

    void Start()
    {
        UnsafeUtility.SetLeakDetectionMode(Unity.Collections.NativeLeakDetectionMode.Enabled);
        BodyTools.body = Resources.Load<GameObject>("Prefabs/Body");
        ShaderCal.GPUPosVelCal = Resources.Load<ComputeShader>("GPUPosVelCal");


        BodyTools.mainBodyMass = mainBodyMass;
        BodyTools.mainBodyPos = mainBodyPos;
        BodyTools.G = G;

        sun = BodyTools.SunInit(sunColor, mainBodyPos, mainBodyMass, mainBodyDensity);
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
        shaderCal = new(bodyList.ToArray());
        generated = true;
    }

    void Update()
    {
        if (bodyList.Count < geneNumber && isReGene)
        {
            for (int i = 0; i < geneSpeed; i++)
            {
                GameObject planet = BodyTools.PlanetInit();
                BodyBehavior behavior = planet.GetComponent<BodyBehavior>();
                behavior.bodyInit = this;
                bodyList.Add((planet, behavior));
            }
        }
    }
}
