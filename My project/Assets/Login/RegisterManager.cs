using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button registerButton;

    private string registerUrl = "http://localhost:5000/user/signup"; // 회원가입 API URL

    private void Awake()
    {
        registerButton.onClick.AddListener(OnRegisterButtonPressed);
    }

    private void OnRegisterButtonPressed()
    {
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        Debug.Log(email);
        Debug.Log(password);
        Debug.Log(confirmPassword);

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
        string jsonBody = JsonUtility.ToJson(new { email, password });
        UnityWebRequest request = new UnityWebRequest(registerUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        
        Debug.Log(request.result);
        Debug.Log(UnityWebRequest.Result.Success);
        
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
    }
}
