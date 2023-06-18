using UnityEngine;

public static class BodyTools
{
    public static GameObject body;
    public static float mainBodyMass;
    public static Vector3 mainBodyPos;
    public static string planetName = "planet";
    private static int count = 1; //行星的命名记述
    public static float planetMinPos;
    public static float planetMaxPos;
    public static float G;
    public static GameObject sun;

    /// <summary>
    /// 初始化并返回一个恒星Sun
    /// </summary>
    /// <param name="color">恒星颜色</param>
    /// <param name="pos">恒星位置</param>
    /// <param name="mass">恒星质量</param>
    /// <returns></returns>
    public static GameObject SunInit(Color color, Vector3 pos, float mass, float density)
    {
        GameObject sun = GameObject.Instantiate(body);
        sun.name = "Sun";
        SpriteRenderer spriteRenderer = sun.GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;

        float diam = StarDiam(mass, density);
        sun.transform.localScale = new Vector3(diam, diam, 1f);

        sun.transform.position = pos;
        BodyBehavior bodyBehavior = sun.GetComponent<BodyBehavior>();
        bodyBehavior.InitInformation(Vector3.zero, diam, mass,density);

        bodyBehavior.mainBody = true;

        return sun;
    }

    /// <summary>
    /// 初始化并返回一个行星planet
    /// </summary>
    /// <returns></returns>
    public static GameObject PlanetInit()
    {
        GameObject planet = GameObject.Instantiate(body);
        planet.name = planetName + " " + count.ToString();
        count++;
        SpriteRenderer spriteRenderer = planet.GetComponent<SpriteRenderer>();
        spriteRenderer.color = RandomColor();

        planet.transform.position = RandomPosition();

        float mass, density, diam;
        (mass, density, diam) = PlanetAttributeInit();
        planet.transform.localScale = new Vector3(diam, diam, 1f);
        Vector3 pos = RandomPosition();
        Vector3 vel = GetVelocity(pos);

        planet.transform.position = pos;

        BodyBehavior bodyBehavior = planet.GetComponent<BodyBehavior>();
        bodyBehavior.InitInformation(vel, diam, mass,density);

        bodyBehavior.mainBody = false;

        return planet;
    }

    /// <summary>
    /// 计算星体的直径
    /// </summary>
    /// <param name="mass">质量</param>
    /// <param name="density">密度</param>
    /// <returns></returns>
    public static float StarDiam(float mass, float density)
    {
        float diam = 2f * Mathf.Pow((3f * (mass / density )) / (4f * Mathf.PI), 1f / 3f);
        return diam;
    }

    public static float StarDensity(float mass)
    {
        float density = 1;
        return density;
    }

    /// <summary>
    /// 随机颜色
    /// </summary>
    /// <returns></returns>
    public static Color RandomColor()
    {
        return new Color(Random.Range(0f, 255f) / 255f, Random.Range(0f, 255f) / 255f, Random.Range(0f, 255f) / 255f, 1f);
    }

    /// <summary>
    /// 随机位置,在初始化脚本内修改最小/大值
    /// </summary>
    /// <returns></returns>
    public static Vector3 RandomPosition()
    {
        float angle = Random.Range(0f,2*Mathf.PI);
        float radius = Random.Range(planetMinPos,planetMaxPos);
        Vector3 pos = new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0) * radius;
        return pos;
    }

    /// <summary>
    /// 获取初始速度
    /// </summary>
    /// <param name="pos">行星位置</param>
    /// <returns>围绕恒星的速度</returns>
    public static Vector3 GetVelocity(Vector3 pos)
    {
        Vector3 target = mainBodyPos - pos;
        Vector3 velNom = Vector3.Cross(target, Vector3.forward).normalized;
        Vector3 vel = velNom * Mathf.Sqrt(G * mainBodyMass / target.magnitude);

        return vel;
    }

    /// <summary>
    /// 初始化质量,密度,直径
    /// </summary>
    /// <returns></returns>
    public static (float, float, float) PlanetAttributeInit()
    {
        float mass, density, diam;
        //mainBodyMass / 500f
        mass = Random.Range(10f, 1000f);
        density = Random.Range(10f, 20f);
        diam = StarDiam(mass, density);
        return (mass, density, diam);
    }

    /// <summary>
    /// 初始化初始化行星的位置范围
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public static void GenerateRange(float min, float max)
    {
        planetMinPos = min;
        planetMaxPos = max;
    }

    
}

