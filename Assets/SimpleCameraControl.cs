using UnityEngine;

public class SimpleCameraControl : MonoBehaviour
{
    public float moveSpeed = 4.0f;        // 歩く速度
    public float mouseSensitivity = 2.0f; // マウスの感度
    public float playerHeight = 1.6f;     // プレイヤーの目の高さ（身長）

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 rot = transform.localRotation.eulerAngles;
        rotationY = rot.y;
        rotationX = rot.x;
    }

    void Update()
    {
        // --- 1. マウスで視線移動 ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0.0f);

        // --- 2. キーボードで前後左右に移動 (W, A, S, D) ---
        float moveForward = Input.GetAxis("Vertical");   // W, S
        float moveSideways = Input.GetAxis("Horizontal"); // A, D

        Vector3 moveDirection = (transform.forward * moveForward) + (transform.right * moveSideways);
        moveDirection.y = 0; // 移動方向の計算では上下を一旦無視

        // 新しい位置を計算（まずは平面移動）
        Vector3 newPosition = transform.position + (moveDirection.normalized * moveSpeed * Time.deltaTime);

        // --- 3. 階段と地面の自動吸い付きシステム ---
        RaycastHit hit;
        Vector3 rayStartPoint = new Vector3(newPosition.x, newPosition.y - 0.2f, newPosition.z);
        
        if (Physics.Raycast(rayStartPoint, Vector3.down, out hit, 10.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            newPosition.y = hit.point.y + playerHeight;
        }
        else
        {
            float targetY = 0.0f + playerHeight;
            newPosition.y = Mathf.Lerp(transform.position.y, targetY, 5.0f * Time.deltaTime);
        }

        // 最終的な位置をカメラに反映
        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}