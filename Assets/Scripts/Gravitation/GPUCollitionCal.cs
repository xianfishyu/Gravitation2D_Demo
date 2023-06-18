using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GPUCollitionCal
{
    //定义各种数组
    public (GameObject planet, BodyBehavior behavior)[] bodyArray;
    public Vector3[] posArray;
    float[] radiusArray;
    public int[] colliderList;

    //定义计算用的东西
    static public ComputeShader GPUColliderCal;
    public ComputeBuffer inputPos, inputRadius, outputBuffer;
    int kernelIndex;
    int arrayLength;

    //类型初始化
    public GPUCollitionCal((GameObject planet, BodyBehavior behavior)[] bodyList)
    {
        this.bodyArray = bodyList;
        posArray = new Vector3[bodyArray.Length];
        radiusArray = new float[bodyArray.Length];

        for (int i = 0; i < bodyArray.Length; i++)
        {
            posArray[i] = bodyArray[i].planet.transform.position;
            radiusArray[i] = (bodyArray[i].behavior.diam) * 10f;
        }

        kernelIndex = GPUColliderCal.FindKernel("CSMain");
    }

    //计算初始化
    void Init(int count)
    {
        inputPos = new ComputeBuffer(count, sizeof(float) * 3);
        inputRadius = new ComputeBuffer(count,sizeof(float));
        outputBuffer = new ComputeBuffer(count,sizeof(int));
    }

    //GPU计算
    int[] GPUCal(Vector3[] posArray, float[] radiusArray)
    {
        Init(posArray.Length);
        arrayLength = posArray.Length;

        inputPos.SetData(posArray);
        inputRadius.SetData(radiusArray);

        GPUColliderCal.SetInt("arrayLength", arrayLength);
        GPUColliderCal.SetBuffer(kernelIndex, "inputPos", inputPos);
        GPUColliderCal.SetBuffer(kernelIndex, "inputRadius", inputRadius);
        GPUColliderCal.SetBuffer(kernelIndex, "outputBuffer", outputBuffer);

        GPUColliderCal.Dispatch(kernelIndex, posArray.Length, 1, 1);

        int[] outputList = new int[posArray.Length];

        outputBuffer.GetData(outputList);

        BufferDestory();

        return outputList;
    }

    void BufferDestory()
    {
        inputPos.Dispose();
        inputRadius.Dispose();
        outputBuffer.Dispose();
    }

    public void CollectionsUpdate()
    {
        colliderList = GPUCal(posArray, radiusArray);

        for (int i = 0; i < colliderList.Length; i++)
        {
            //Debug.Log(colliderList);
            if(colliderList[i] != -1)
            {
                Debug.Log("哪个对象:" + bodyArray[i].planet.name + " 与哪个碰撞:" + bodyArray[colliderList[i]].planet.name);
                // bodyArray[i]
                // .behavior
                // .Trigger(bodyArray[colliderList[i]]
                // .planet);
                // colliderList[colliderList[i]] = -1;
            }
        }
    }
}
