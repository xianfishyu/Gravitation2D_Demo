using System;
using ImGuiNET;
using UImGui;
using UnityEngine;

public class Imgui : MonoBehaviour
{
    [SerializeField]
    private Camera _camera = null;

    private BodyInit bodyInit;
    private bool bloom = true;

    private void Awake()
    {
        bodyInit = GetComponent<BodyInit>();
        UImGuiUtility.Layout += OnLayout;
        UImGuiUtility.OnInitialize += OnInitialize;
        UImGuiUtility.OnDeinitialize += OnDeinitialize;
    }

    private void OnLayout(UImGui.UImGui obj)
    {
        ImGui.Begin("Config");
        if(ImGui.TreeNode("Drag"))
        {
            ImGui.DragInt("Gene Number", ref bodyInit.geneNumber, 10f, 1, 10000);
            ImGui.DragFloat("Gene Speed", ref bodyInit.geneSpeed, 0.5f, 1f, 20f);
            ImGui.DragFloat("Sim Speed", ref bodyInit.calcSpeed, 0.001f, 0.07f, 30.0f);
            ImGui.DragFloat("Gravity", ref bodyInit.G, 0.001f, 0.1f, 3f);
            ImGui.DragFloat("Planet Max Pos", ref bodyInit.planetMaxPos, 50f, 500f, 5000f);
            ImGui.DragIntRange2("Planet Gene Mass",ref bodyInit.minMass,ref bodyInit.maxMass,5f,1,500);
            ImGui.DragFloat("power", ref bodyInit.power, 0.1f, -3f, 3f);
            ImGui.DragFloat("size",ref _camera.GetComponent<CameraController2D>().scale,1f,100f,5000f);
        }
        if(ImGui.TreeNode("More"))
        {
            ImGui.Checkbox("Generate", ref bodyInit.isReGene);
            ImGui.Checkbox("Trail", ref bodyInit.enableTrail);
            ImGui.Checkbox("Collision", ref bodyInit.enableCollision);
            ImGui.Checkbox("enableSunCollision", ref bodyInit.enableSunCollision);
            ImGui.Checkbox("enableSunMove", ref bodyInit.enableSunMove);
            ImGui.Checkbox("Lock Gravity", ref bodyInit.lockG);
            if (ImGui.Checkbox("Bloom", ref bloom)) _camera.GetComponent<CameraController2D>().SetBloom(bloom);
        }
        ImGui.LabelText("Total Count", $"{bodyInit.bodyList.Count}");
        if (ImGui.Button("Reload")) bodyInit.ReloadScene();
        ImGui.End();
    }

    private void OnInitialize(UImGui.UImGui obj)
    {
        // runs after UImGui.OnEnable();
    }

    private void OnDeinitialize(UImGui.UImGui obj)
    {
        // runs after UImGui.OnDisable();
    }

    private void OnDisable()
    {
        UImGuiUtility.Layout -= OnLayout;
        UImGuiUtility.OnInitialize -= OnInitialize;
        UImGuiUtility.OnDeinitialize -= OnDeinitialize;
    }
}