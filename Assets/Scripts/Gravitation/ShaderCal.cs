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
    public ComputeBuffer inputPos, inputVel, inputMass, inputRadius, outputPos, outputVel, collisionFlagsBuffer, collisionIndexBuffer;

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



    public ShaderCal(BodyInit bodyInit)
    {
        kernelIndex = GPUPosVelCal.FindKernel("CSMain");
        this.bodyInit = bodyInit;
        count = bodyInit.bodyList.Count;

        //初始化id
        RentPool();
        SetData();
        isReady = true;
    }

    public void SetData()
    {
        for (int i = 0; i < count; i++)
        {
            bodyArray[i] = bodyInit.bodyList[i];
        }
    }


    //初始化
    public void BufferInit()
    {
        //初始化缓冲区
        inputPos = new ComputeBuffer(count, sizeof(float) * 3);
        inputVel = new ComputeBuffer(count, sizeof(float) * 3);
        inputRadius = new ComputeBuffer(count, sizeof(float));
        inputMass = new ComputeBuffer(count, sizeof(float));
        outputPos = new ComputeBuffer(count, sizeof(float) * 3);
        outputVel = new ComputeBuffer(count, sizeof(float) * 3);
        collisionFlagsBuffer = new ComputeBuffer(count, sizeof(int));
        collisionIndexBuffer = new ComputeBuffer(count, sizeof(int));
    }

    public void BufferDestory()
    {
        inputPos.Dispose();
        inputVel.Dispose();
        inputMass.Dispose();
        inputRadius.Dispose();
        outputPos.Dispose();
        outputVel.Dispose();
        collisionFlagsBuffer.Dispose();
        collisionIndexBuffer.Dispose();
    }

    //计算
    void GPUCal()
    {
        //set缓冲区
        inputPos.SetData(this.pos, 0, 0, count);
        inputVel.SetData(this.vel, 0, 0, count);
        inputMass.SetData(mass, 0, 0, count);
        inputRadius.SetData(radiusArray, 0, 0, count);
        //将缓冲区内容写入到shader中
        GPUPosVelCal.SetFloat("G", bodyInit.G);
        GPUPosVelCal.SetFloat("dt", bodyInit.calcSpeed * Time.smoothDeltaTime);
        GPUPosVelCal.SetInt("count", count);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputPos", inputPos);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputVel", inputVel);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputMass", inputMass);
        GPUPosVelCal.SetBuffer(kernelIndex, "inputRadius", inputRadius);
        GPUPosVelCal.SetBuffer(kernelIndex, "outputPos", outputPos);
        GPUPosVelCal.SetBuffer(kernelIndex, "outputVel", outputVel);
        GPUPosVelCal.SetBuffer(kernelIndex, "collisionFlagsBuffer", collisionFlagsBuffer);
        GPUPosVelCal.SetBuffer(kernelIndex, "collisionIndexBuffer", collisionIndexBuffer);
        //shader计算
        GPUPosVelCal.Dispatch(kernelIndex, count * 2, 1, 1);
        //获取输出数组
        outputPos.GetData(pos, 0, 0, count);
        outputVel.GetData(vel, 0, 0, count);
        collisionFlagsBuffer.GetData(collisionFlags, 0, 0, count);
        collisionIndexBuffer.GetData(collisionIndex, 0, 0, count);
    }

    /// <summary>
    /// 将计算出来的Pos/Vel赋值到原来的GO里
    /// </summary>
    public void PosVelUpdate()
    {
        BufferInit();
        
        for (int i = 0; i < count; i++)
        {
            // pos[i] = bodyArray[i].planet.transform.position;
            pos[i] = bodyArray[i].behavior.pos;
            vel[i] = bodyArray[i].behavior.vel;
            mass[i] = bodyArray[i].behavior.mass;
            radiusArray[i] = (bodyArray[i].behavior.diam) / 2f;
        }

        GPUCal();

        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                // bodyArray[i].planet.transform.position = this.pos[i];
                bodyArray[i].behavior.pos = this.pos[i];
                bodyArray[i].behavior.vel = this.vel[i];
            }
            int otherIndex = collisionIndex[i];
            if (collisionFlags[i] == 1)
            {
                try
                {
                    bodyArray[i].behavior.Trigger(
                        bodyArray[otherIndex].planet
                    );
                }
                catch (System.Exception)
                {
                    Debug.LogError(i);
                    Debug.LogError(otherIndex);
                }
            }
        }

        BufferDestory();
    }

    public void RentPool()
    {
        bodyArray = ArrayPool<(GameObject planet, BodyBehavior behavior)>.Shared.Rent(count);
        pos = ArrayPool<Vector3>.Shared.Rent(count);
        vel = ArrayPool<Vector3>.Shared.Rent(count);
        mass = ArrayPool<float>.Shared.Rent(count);
        radiusArray = ArrayPool<float>.Shared.Rent(count);
        collisionIndex = ArrayPool<int>.Shared.Rent(count);
        collisionFlags = ArrayPool<int>.Shared.Rent(count);
        
        // bodyArray = new (GameObject planet, BodyBehavior behavior)[count];
        // pos = new Vector3[count];
        // vel = new Vector3[count];
        // mass = new float[count];
        // radiusArray = new float[count];
        // collisionFlags = new int[count];
        // collisionIndex = new int[count];
    }
}