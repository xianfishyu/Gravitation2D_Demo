using System;
using UnityEngine;

[Serializable]
public class ShaderCal
{
    //缓冲区
    static public ComputeShader GPUPosVelCal;
    public ComputeBuffer inputPos, inputVel, inputMass, inputParameter, outputPos, outputVel;
    //配置
    public BodyInit bodyInit;

    //id
    int kernelIndex;
    //各种列表
    public (GameObject planet, BodyBehavior behavior)[] bodyArray;
    public Vector3[] pos;
    public Vector3[] vel;
    public float[] mass;
    //public float[] radius;
    [NonSerialized] public bool isReady = false;

    public ShaderCal((GameObject planet, BodyBehavior behavior)[] bodyList)
    {
        this.bodyArray = bodyList;
        pos = new Vector3[bodyArray.Length];
        vel = new Vector3[bodyArray.Length];
        mass = new float[bodyArray.Length];

        for (int i = 0; i < bodyArray.Length; i++)
        {
            pos[i] = bodyArray[i].planet.transform.position;
            vel[i] = bodyArray[i].behavior.vel;
            mass[i] = bodyArray[i].behavior.mass;
        }
    
        //初始化id
        kernelIndex = GPUPosVelCal.FindKernel("CSMain");

        isReady = true;
    }


    //初始化
    void BufferInit(int count)
    {
        //初始化缓冲区
        inputPos = new ComputeBuffer(count, sizeof(float) * 3);
        inputVel = new ComputeBuffer(count, sizeof(float) * 3);
        inputMass = new ComputeBuffer(count, sizeof(float));
        outputPos = new ComputeBuffer(count, sizeof(float) * 3);
        outputVel = new ComputeBuffer(count, sizeof(float) * 3);
    }

    void BufferDestory()
    {
        inputPos.Dispose();
        inputVel.Dispose();
        inputMass.Dispose();
        outputPos.Dispose();
        outputVel.Dispose();
    }

    //计算
    (Vector3[], Vector3[]) GPUCal(Vector3[] pos, Vector3[] vel, float[] mass)
    {
        //初始化参数
        BufferInit(pos.Length);

        

        //set缓冲区
        inputPos.SetData(pos);
        inputVel.SetData(vel);
        inputMass.SetData(mass);
        //将缓冲区内容写入到shader中
        GPUPosVelCal.SetFloat("G", bodyInit.G);
        GPUPosVelCal.SetFloat("dt", bodyInit.dt);
        GPUPosVelCal.SetInt("count", pos.Length);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputPos", inputPos);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputVel", inputVel);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputMass", inputMass);
        GPUPosVelCal.SetBuffer(kernelIndex, "outputPos", outputPos);
        GPUPosVelCal.SetBuffer(kernelIndex, "outputVel", outputVel);
        //shader计算
        GPUPosVelCal.Dispatch(kernelIndex, pos.Length, 1, 1);
        //定义输出数组
        Vector3[] posList = new Vector3[pos.Length];
        Vector3[] velList = new Vector3[vel.Length];
        //获取输出数组

        outputPos.GetData(posList);
        outputVel.GetData(velList);

        BufferDestory();
        //返回
        return (posList, velList);
    }

    /// <summary>
    /// 将计算出来的Pos/Vel赋值到原来的GO里
    /// </summary>
    public void PosVelUpdate()
    {
        (Vector3[] pos, Vector3[] vel) ret = GPUCal(this.pos, this.vel, this.mass);
        this.pos = ret.pos;
        this.vel = ret.vel;

        for (int i = 1; i < bodyArray.Length; i++)
        {
            bodyArray[i].planet.transform.position = this.pos[i];
            bodyArray[i].behavior.pos = this.pos[i];
            bodyArray[i].behavior.vel = this.vel[i];
        }
    }

}