using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using TMPro;

//구슬에서 정보를 받아 UI에 띄우는 메서드를 정의한 클래스
//UI를 띄울 패널, 이미지 패널, 텍스트 패널을 연결해주면 동작
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject orbDisplayPanel; // OrbDisplay UI 패널
    public Image orbImage; // 구슬 이미지
    public VideoPlayer orbVideoPlayer; // 구슬 영상 플레이어 (필요 시)
    public TextMeshProUGUI orbNameText; // 구슬 이름 텍스트 (선택 사항)

    private void Awake()
    {
        // 싱글톤 패턴 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 구슬 정보 표시 메서드
    public void ShowOrb(Orb orb)
    {
        if (orbDisplayPanel == null || orbImage == null)
            return;

        // 구슬 데이터 가져오기
        OrbData data = orb.orbData;

        // 구슬 이름 설정 (선택 사항)
        if (orbNameText != null)
        {
            orbNameText.text = data.orbName;
        }

        // 구슬 이미지 설정
        if (data.orbImage != null)
        {
            orbImage.sprite = data.orbImage;
            orbImage.gameObject.SetActive(true);
        }
        else
        {
            orbImage.gameObject.SetActive(false);
        }

        // 구슬 영상 설정 (필요 시)
        if (orbVideoPlayer != null && data.orbVideo != null)
        {
            orbVideoPlayer.clip = data.orbVideo;
            orbVideoPlayer.gameObject.SetActive(true);
            orbVideoPlayer.Play();
        }
        else
        {
            if (orbVideoPlayer != null)
                orbVideoPlayer.gameObject.SetActive(false);
        }

        // UI 패널 활성화
        orbDisplayPanel.SetActive(true);

        // 애니메이션 시작 (예: 확대 효과)
        StartCoroutine(AnimateUI(orbDisplayPanel));
    }

    // UI 숨기기 메서드
    public void HideOrb()
    {
        if (orbDisplayPanel == null)
            return;

        // UI 패널 비활성화
        orbDisplayPanel.SetActive(false);

        // 영상 정지
        if (orbVideoPlayer != null)
        {
            orbVideoPlayer.Stop();
        }
    }

    // UI 애니메이션 Coroutine (간단한 확대 효과)
    private IEnumerator AnimateUI(GameObject uiElement)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = Vector3.one;

        uiElement.transform.localScale = initialScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            uiElement.transform.localScale = Vector3.Lerp(initialScale, finalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        uiElement.transform.localScale = finalScale;
    }
}
    