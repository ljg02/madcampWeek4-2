using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;

    public LoginRequest(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}
[System.Serializable]
public class LoginResponse
{
    public bool success;
    public string token;
}
public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput; // 이메일 입력 필드
    [SerializeField] private TMP_InputField passwordInput; // 비밀번호 입력 필드
    [SerializeField] private TextMeshProUGUI statusText; // 상태 메시지 표시
    [SerializeField] private Button loginButton;
    
    private string loginUrl = "http://localhost:5000/user/login"; // 백엔드 로그인 API URL
    
    [SerializeField] private Vector3 cameraTargetPosition = new Vector3(0, 10, -10); // 카메라가 이동할 목표 위치
    [SerializeField] private Quaternion cameraTargetRotation; // 카메라 목표 회전
    [SerializeField] private float cameraMoveDuration = 2.0f; // 카메라 이동 애니메이션 지속 시간
    [SerializeField] private GameObject loginUI; // 비활성화할 로그인 UI의 루트 GameObject
    
    [SerializeField] private CameraController cameraController; // CameraController 참조 추가
    
    private Vector3 cameraInitialPosition; // 카메라의 초기 위치
    private Quaternion cameraInitialRotation; // 카메라의 초기 회전
    private bool isCameraMoving = false; // 카메라 이동 중인지 여부
    private void Awake()
    {
        loginButton.onClick.AddListener(OnLoginButtonPressed);
    }
    
    private void Start()
    {
        // 카메라 초기 위치와 회전을 저장
        if (Camera.main != null)
        {
            cameraInitialPosition = Camera.main.transform.position;
            cameraInitialRotation = Camera.main.transform.rotation;
        }
        else
        {
            Debug.LogError("Main Camera not found.");
        }
        
        // CameraController가 할당되었는지 확인하고, 초기 상태를 비활성화
        if (cameraController != null)
        {
            cameraController.allowControl = false;
        }
        else
        {
            Debug.LogError("CameraController is not assigned in the LoginManager.");
        }
    }
    // 로그인 버튼이 눌렸을 때 호출
    public void OnLoginButtonPressed()
    {
        Debug.Log("Login button clicked!");
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter your email and password.";
            return;
        }

        StartCoroutine(Login(email, password));
    }

    private IEnumerator Login(string email, string password)
    {
        // JSON 요청 생성
        string jsonBody = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";
        Debug.Log(email);
        Debug.Log(password);
        Debug.Log(jsonBody);

        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 요청 전송
        yield return request.SendWebRequest();
        Debug.Log(request);
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 응답 처리
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            Debug.Log("Token: " + response.token);
            // 카메라 부드럽게 이동
            StartCoroutine(MoveCamera(cameraTargetPosition, cameraTargetRotation, cameraMoveDuration));

            // 로그인 UI 비활성화
            HideLoginUI();

            // CameraController 활성화
            EnableCameraControl();
        }
        else
        {
            statusText.text = $"Login failed: {request.error}";
        }
        request.Dispose(); 
    }
    
    private IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        if (Camera.main == null || isCameraMoving)
        {
            yield break;
        }

        isCameraMoving = true;

        Vector3 startPosition = Camera.main.transform.position;
        Quaternion startRotation = Camera.main.transform.rotation;

        Vector3 endPosition = targetPosition;
        Quaternion endRotation = targetRotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            Camera.main.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            Camera.main.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = endPosition;
        Camera.main.transform.rotation = endRotation;

        isCameraMoving = false;

        Debug.Log($"Camera moved to {targetPosition} over {duration} seconds.");
    }
    
    public void ShowLoginUI()
    {
        if (loginUI != null)
        {
            loginUI.SetActive(true);
            Debug.Log("Login UI has been shown.");
        }
        else
        {
            Debug.LogError("Login UI GameObject is not assigned.");
        }
    }

    private void HideLoginUI()
    {
        if (loginUI != null)
        {
            loginUI.SetActive(false);
            Debug.Log("Login UI has been hidden.");
        }
        else
        {
            Debug.LogError("Login UI GameObject is not assigned.");
        }
    }
    
    private void EnableCameraControl()
    {
        if (cameraController != null)
        {
            cameraController.allowControl = true;
            Debug.Log("CameraController has been enabled.");
        }
        else
        {
            Debug.LogError("CameraController is not assigned.");
        }
    }
    
    private void DisableCameraControl()
    {
        if (cameraController != null)
        {
            cameraController.allowControl = false;
            Debug.Log("CameraController has been disabled.");
        }
        else
        {
            Debug.LogError("CameraController is not assigned.");
        }
    }
    
    private void OnDisable()
    {
        loginButton.onClick.RemoveListener(OnLoginButtonPressed);
    }
    
    public void ResetCamera()
    {
        if (Camera.main != null && !isCameraMoving)
        {
            StartCoroutine(MoveCamera(cameraInitialPosition, cameraInitialRotation, cameraMoveDuration));
            Debug.Log("Camera has been reset to initial position.");
        }
        
        // CameraController 비활성화
        DisableCameraControl();
    }

}
