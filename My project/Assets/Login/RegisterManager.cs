using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string password;

    public RegisterRequest(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}
public class RegisterManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backButton;
    
    [SerializeField] public GameObject initialPageCanvas;   // 초기 페이지 캔버스
    [SerializeField] public GameObject loginCanvas;         // 로그인 캔버스
    [SerializeField] public GameObject registerCanvas;      // 회원가입 캔버스

    private string registerUrl = "http://localhost:5000/user/signup"; // 회원가입 API URL

    private void Awake()
    {
        registerButton.onClick.AddListener(OnRegisterButtonPressed);
        backButton.onClick.AddListener(ShowInitialCanvas);
    }
    
    public void ShowLoginCanvas()
    {
        registerCanvas.SetActive(false);
        loginCanvas.SetActive(true);
    }

    // 회원가입 캔버스를 활성화하고 초기 페이지를 비활성화
    public void ShowInitialCanvas()
    {
        registerCanvas.SetActive(false);
        initialPageCanvas.SetActive(true);
    }

    private void OnRegisterButtonPressed()
    {
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            statusText.text = "Please fill all fields.";
            return;
        }

        if (password != confirmPassword)
        {
            statusText.text = "Passwords do not match.";
            return;
        }

        StartCoroutine(Register(email, password));
    }
    
    private IEnumerator Register(string email, string password)
    {   
        string jsonBody = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";
        
        UnityWebRequest request = new UnityWebRequest(registerUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Register successful: " + request.downloadHandler.text);
            statusText.text = "Registration successful!";
        }
        else
        {
            Debug.LogError("Register failed: " + request.error);
            statusText.text = $"Register failed: {request.error}";
        }
        
        ShowLoginCanvas();
    }
    private void OnDisable()
    {
        registerButton.onClick.RemoveListener(OnRegisterButtonPressed);
        backButton.onClick.RemoveListener(ShowInitialCanvas);
    }
}
