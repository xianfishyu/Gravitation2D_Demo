using UnityEngine;
/*
public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 2f;
    public float minZoom = 1f;
    public float maxZoom = 10f;

    private Camera mainCamera;
    private float currentZoom = 1f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 获取鼠标滚轮输入值并更新缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // 获取键盘方向键输入值并更新摄像机位置
        float xInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(xInput, yInput, 0f).normalized;
        float speed = moveSpeed * currentZoom;
        transform.position += direction * speed * Time.deltaTime;
        mainCamera.orthographicSize = currentZoom;
    }
}
*/

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;    // 相机移动速度
    public float zoomSpeed = 5f;    // 相机缩放速度
    public float minZoom = 2f;      // 相机最小缩放比例
    public float maxZoom = 20f;     // 相机最大缩放比例

    private Vector3 mouseOrigin;    // 鼠标按下的位置
    private bool isDragging;        // 是否正在拖动鼠标
    private Camera mainCamera;      // 主摄像机
    private float zoom = 150f;       // 初始缩放比例

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // WASD移动
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.position += new Vector3(horizontal, vertical, 0f) * moveSpeed * zoom * Time.deltaTime;

        // 鼠标滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoom -= scroll * zoomSpeed;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        mainCamera.orthographicSize = zoom;

        // 按住鼠标中键移动
        if (Input.GetMouseButtonDown(2))
        {
            mouseOrigin = Input.mousePosition;
            isDragging = true;
        }
        if (!Input.GetMouseButton(2))
        {
            isDragging = false;
        }
        if (isDragging)
        {
            Vector3 pos = mainCamera.ScreenToViewportPoint(mouseOrigin - Input.mousePosition);
            Vector3 move = new Vector3(pos.x * moveSpeed, pos.y * moveSpeed, 0);
            transform.position += move * zoom;
            mouseOrigin = Input.mousePosition;
        }
    }
}
