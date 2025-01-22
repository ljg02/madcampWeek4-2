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
    private void Awake()
    {
        loginButton.onClick.AddListener(OnLoginButtonPressed);
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

        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 요청 전송
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            statusText.text = "Login successful!";
            
            // 응답 처리
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            Debug.Log("Token: " + response.token);
        }
        else
        {
            statusText.text = $"Login failed: {request.error}";
        }
        request.Dispose(); 
    }
    private void OnDisable()
    {
        loginButton.onClick.RemoveListener(OnLoginButtonPressed);
    }

}
