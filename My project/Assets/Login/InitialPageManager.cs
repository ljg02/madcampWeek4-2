using UnityEngine;
using UnityEngine.UI;

public class InitialPageManager : MonoBehaviour
{
    [Header("캔버스 참조")]
    public GameObject initialPageCanvas;   // 초기 페이지 캔버스
    public GameObject loginCanvas;         // 로그인 캔버스
    public GameObject registerCanvas;      // 회원가입 캔버스

    [Header("버튼 참조")]
    public Button loginButton;             // 로그인 버튼
    public Button registerButton;          // 회원가입 버튼

    void Start()
    {
        // 초기 페이지는 활성화, 다른 캔버스는 비활성화
        initialPageCanvas.SetActive(true);
        loginCanvas.SetActive(false);
        registerCanvas.SetActive(false);

        // 버튼 클릭 이벤트에 리스너 추가
        loginButton.onClick.AddListener(ShowLoginCanvas);
        registerButton.onClick.AddListener(ShowRegisterCanvas);
    }

    // 로그인 캔버스를 활성화하고 초기 페이지를 비활성화
    public void ShowLoginCanvas()
    {
        initialPageCanvas.SetActive(false);
        loginCanvas.SetActive(true);
    }

    // 회원가입 캔버스를 활성화하고 초기 페이지를 비활성화
    public void ShowRegisterCanvas()
    {
        initialPageCanvas.SetActive(false);
        registerCanvas.SetActive(true);
    }
}