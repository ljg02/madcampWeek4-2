using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("속도 설정")]
    public float rotationSpeed = 100.0f; // 마우스 회전 속도
    public float moveSpeed = 10.0f;       // 카메라 이동 속도
    public float zoomSpeed = 50.0f;       // 줌 속도
    public float minZoom = 5.0f;          // 최소 줌 거리
    public float maxZoom = 50.0f;         // 최대 줌 거리

    private float yaw = 0.0f;             // Y축 회전
    private float pitch = 0.0f;           // X축 회전
    private float currentZoom = 20.0f;    // 현재 줌 거리

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        currentZoom = Vector3.Distance(transform.position, Vector3.zero);
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
        HandleZoom();
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // 마우스 우클릭 시 회전
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -89f, 89f); // 상하 회전 제한

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            transform.rotation = rotation;
        }
    }

    void HandleMovement()
    {
        float moveX = 0.0f;
        float moveZ = 0.0f;
        float moveY = 0.0f;

        // XZ 평면 이동
        if (Input.GetKey(KeyCode.W))
            moveZ += 1.0f;
        if (Input.GetKey(KeyCode.S))
            moveZ -= 1.0f;
        if (Input.GetKey(KeyCode.A))
            moveX -= 1.0f;
        if (Input.GetKey(KeyCode.D))
            moveX += 1.0f;

        // Y축 이동
        if (Input.GetKey(KeyCode.Space))
            moveY += 1.0f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            moveY -= 1.0f;

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;

        // XZ 평면 이동 적용
        transform.position += new Vector3(move.x, 0, move.z) * moveSpeed * Time.deltaTime;

        // Y축 이동 적용
        transform.position += Vector3.up * moveY * moveSpeed * Time.deltaTime;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            currentZoom -= scroll * zoomSpeed * Time.deltaTime;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            Vector3 direction = transform.forward * currentZoom;
            transform.position = Vector3.zero - direction;
        }
    }
}
