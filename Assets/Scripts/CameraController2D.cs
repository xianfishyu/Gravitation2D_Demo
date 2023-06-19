using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController2D : MonoBehaviour
{
    private Camera mainCamera;
    private BloomEffect bloomShader;

    public float scale;
    public Vector3 cameraPos;
    public Vector3 mousePos;
    
    private Vector3 vector3Zero,vector3Zero2,vector3Zero3;


    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        bloomShader = GetComponent<BloomEffect>();

        scale = mainCamera.orthographicSize;
        cameraPos = mainCamera.transform.position;
    }

    private void FixedUpdate()
    {
        KeyPosUpdate();
    }
    private void Update()
    {
        ScaleUpdate();
        MousePosUpdate();
    }

    void ScaleUpdate()
    {
        scale -= Input.GetAxis("Mouse ScrollWheel") * scale;
        mainCamera.orthographicSize = Vector3.SmoothDamp(new(mainCamera.orthographicSize,0,0),new(scale,0,0),ref vector3Zero,.1f).x;
    }

    void KeyPosUpdate()
    {
        cameraPos = mainCamera.transform.position;
        cameraPos = Vector3.SmoothDamp(cameraPos,cameraPos + (new Vector3(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"),0f) * scale),ref vector3Zero2,.3f);
        mainCamera.transform.position = cameraPos;
    }

    void MousePosUpdate()
    {
        if(Input.GetMouseButton(2))
        {   
            if(Input.GetMouseButtonDown(2))
            {
                mousePos = Camera.main.ScreenToWorldPoint(new(Input.mousePosition.x,Input.mousePosition.y,-10f));
            }
            Vector3 deltaPos =mousePos - Camera.main.ScreenToWorldPoint(new(Input.mousePosition.x,Input.mousePosition.y,-10f));
            mainCamera.transform.position = mainCamera.transform.position + deltaPos;
        }
    }

    public void SetBloom(bool status) {
        bloomShader.enabled = status;
    }
}
