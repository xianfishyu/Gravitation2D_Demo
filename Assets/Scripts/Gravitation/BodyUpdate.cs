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

    void Update()
    {
        bodyInit.UpdateStar();
    }
}
