using System;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Buffers;

[Serializable]
public class ShaderCal
{
    //GPU参数区
    static public ComputeShader GPUPosVelCal;
    static int kernelIndex;
    public ComputeBuffer posBuffer, velBuffer, inputMass, inputRadius, collisionFlagsBuffer, collisionIndexBuffer;

    //配置
    [NonSerialized]
    public BodyInit bodyInit;

    //各种列表
    private Vector3[] pos;
    private Vector3[] vel;
    private float[] mass;
    private float[] radiusArray;
    private int[] collisionIndex;
    private int[] collisionFlags;
    private (GameObject planet, BodyBehavior behavior)[] bodyArray;

    public int count;
    public bool isReady = false;


    /// <summary>
    /// 当生成行星以及摧毁行星时调用
    /// </summary>
    /// <param name="bodyInit"></param>
    public ShaderCal(BodyInit bodyInit)
    {
        //初始化GPU参数
        kernelIndex = GPUPosVelCal.FindKernel("CSMain");

        //获得星体数量
        this.bodyInit = bodyInit;
        count = bodyInit.bodyList.Count;

        //初始化数据
        RentPool();
        SetData();
        BufferInit();

        isReady = true;
    }

    //将pos/vel/mass/radius等压入数组中
    //将星体列表中的元素压入数组中
    void SetData()
    {
        bodyInit.bodyList.CopyTo(bodyArray, 0);
        for (int i = 0; i < count; i++)
        {
            // pos[i] = bodyArray[i].planet.transform.position;
            pos[i] = bodyArray[i].behavior.pos;
            vel[i] = bodyArray[i].behavior.vel;
            mass[i] = bodyArray[i].behavior.mass;
            radiusArray[i] = (bodyArray[i].behavior.diam) / 2f;
        }
    }


    //初始化Buffer
    void BufferInit()
    {
        //初始化缓冲区
        posBuffer = new ComputeBuffer(count, sizeof(float) * 3);
        velBuffer = new ComputeBuffer(count, sizeof(float) * 3);
        inputRadius = new ComputeBuffer(count, sizeof(float));
        inputMass = new ComputeBuffer(count, sizeof(float));
        collisionFlagsBuffer = new ComputeBuffer(count, sizeof(int));
        collisionIndexBuffer = new ComputeBuffer(count, sizeof(int));

        //set缓冲区
        posBuffer.SetData(this.pos, 0, 0, count);
        velBuffer.SetData(this.vel, 0, 0, count);
        inputMass.SetData(mass, 0, 0, count);
        inputRadius.SetData(radiusArray, 0, 0, count);

        //将缓冲区内容写入到shader中
        GPUPosVelCal.SetInt("count", count);
        GPUPosVelCal.SetBuffer(kernelIndex, "Pos", posBuffer);
        GPUPosVelCal.SetBuffer(kernelIndex, "Vel", velBuffer);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputMass", inputMass);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputRadius", inputRadius);
        GPUPosVelCal.SetBuffer(kernelIndex, "collisionFlagsBuffer", collisionFlagsBuffer);
        GPUPosVelCal.SetBuffer(kernelIndex, "collisionIndexBuffer", collisionIndexBuffer);
    }

    //释放Buffer
    public void BufferDestory()
    {
        posBuffer.Dispose();
        velBuffer.Dispose();
        inputMass.Dispose();
        inputRadius.Dispose();
        collisionFlagsBuffer.Dispose();
        collisionIndexBuffer.Dispose();
    }

    //计算
    public void GPUCal()
    {
        //shader计算
        GPUPosVelCal.SetFloat("power",bodyInit.power);
        GPUPosVelCal.SetFloat("G", bodyInit.G);
        GPUPosVelCal.SetFloat("dt", bodyInit.simSpeed * Time.smoothDeltaTime);
        GPUPosVelCal.Dispatch(kernelIndex, count, 1, 1);
    }

    /// <summary>
    /// 将计算出来的Pos/Vel赋值到原来的GO里
    /// </summary>
    public void PosVelUpdate()
    {
        //获取输出数组
        posBuffer.GetData(pos, 0, 0, count);
        velBuffer.GetData(vel, 0, 0, count);
        collisionFlagsBuffer.GetData(collisionFlags, 0, 0, count);
        collisionIndexBuffer.GetData(collisionIndex, 0, 0, count);

        //读取并赋值
        for (int i = 0; i < count; i++)
        {
            if (bodyInit.enableSunMove || i > 0)
            {
                // bodyArray[i].planet.transform.position = this.pos[i];
                bodyArray[i].behavior.pos = this.pos[i];
                bodyArray[i].behavior.vel = this.vel[i];
            }
            int otherIndex = collisionIndex[i];
            if (collisionFlags[i] == 1)
            {
                bodyArray[i].behavior.Trigger(
                    bodyArray[otherIndex].planet
                );
            }
        }
    }

    //对象池
    void RentPool()
    {
        bodyArray = ArrayPool<(GameObject planet, BodyBehavior behavior)>.Shared.Rent(count);
        pos = ArrayPool<Vector3>.Shared.Rent(count);
        vel = ArrayPool<Vector3>.Shared.Rent(count);
        mass = ArrayPool<float>.Shared.Rent(count);
        radiusArray = ArrayPool<float>.Shared.Rent(count);
        collisionIndex = ArrayPool<int>.Shared.Rent(count);
        collisionFlags = ArrayPool<int>.Shared.Rent(count);
    }
}