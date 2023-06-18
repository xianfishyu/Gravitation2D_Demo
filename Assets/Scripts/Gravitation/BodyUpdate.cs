using UnityEngine;
/// <summary>
/// 更新逻辑
/// </summary>
public class BodyUpdate : MonoBehaviour
{
    BodyInit bodyInit;


    private void Start()
    {
        bodyInit = GetComponent<BodyInit>();
    }

    void FixedUpdate()
    {
        if (bodyInit.shaderCal.isReady)
        {
            bodyInit.shaderCal.PosVelUpdate();

            //bodyInit.gPUCollitionCal = new(bodyInit.shaderCal.bodyArray);
            //bodyInit.gPUCollitionCal.CollectionsUpdate();
        }
    }
}
