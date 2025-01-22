using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

public class BeamProjector : MonoBehaviour
{
    // Screen Plane에 할당된 Renderer
    public Renderer screenRenderer;

    // VideoPlayer 컴포넌트 (비디오 재생 시 사용)
    public VideoPlayer videoPlayer;

    // 현재 트리거 영역에 있는 Orb
    private Orb currentOrb;
    private bool isProcessing = false; // 상태 관리 변수 추가

    // 초기 알파 값 설정
    private void Awake()
    {
        SetScreenAlpha(0f);
    }
    
    void Start()
    {
        if (screenRenderer == null)
        {
            Debug.LogError("Screen Renderer가 할당되지 않았습니다.");
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"triggerEnter, processing : {isProcessing}.");
        if (isProcessing) return; // 이미 재생 중이라면 무시
        
        Orb orb = other.GetComponent<Orb>();
        if (orb != null)
        {
            currentOrb = orb;
            isProcessing = true; // 재생 시작
            ProjectOrbContent(orb.orbData);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"triggerEnter, processing : {isProcessing}.");
        if (isProcessing == false) return; // 재생 중이 아니라면 무시
        
        Orb orb = other.GetComponent<Orb>();
        if (orb != null && orb == currentOrb)
        {
            isProcessing = false; // 재생 끝
            ClearScreen();
            currentOrb = null;
        }
    }

    private void ProjectOrbContent(OrbData orbData)
    {
        if (orbData.orbVideo != null && videoPlayer != null)
        {
            // 비디오 클립 설정 및 재생
            videoPlayer.clip = orbData.orbVideo;
            videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
            screenRenderer.material.mainTexture = videoPlayer.targetTexture;
            SetScreenAlpha(0f).OnComplete(() =>
            {
                SetScreenAlpha(1f, 1f); // 페이드 인 애니메이션
            });
            videoPlayer.Play();
        }
        else if (orbData.orbImage != null && screenRenderer != null)
        {
            // 이미지 텍스처 설정
            screenRenderer.material.mainTexture = orbData.orbImage.texture;
            SetScreenAlpha(0f).OnComplete(() =>
            {
                SetScreenAlpha(1f, 1f); // 페이드 인 애니메이션
            });
        }
        else
        {
            Debug.LogWarning("OrbData에 이미지나 비디오가 설정되지 않았습니다.");
        }
    }

    private void ClearScreen()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            videoPlayer.targetTexture = null;
        }
        
        SetScreenAlpha(1f).OnComplete(() =>
        {
            if (screenRenderer != null)
            {
                screenRenderer.material.mainTexture = null;
                SetScreenAlpha(0f, 1f); // 페이드 아웃 애니메이션
            }
        });
    }
    
    // Tween을 반환하도록 수정
    private Tween SetScreenAlpha(float alpha, float duration = 0f)
    {
        Color color = screenRenderer.material.color;
        // DOTween을 사용하여 알파 값을 변경하고 Tween 반환
        return DOTween.To(() => color.a, x => 
        {
            color.a = x;
            screenRenderer.material.color = color;
        }, alpha, duration);
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // 비디오가 준비되면 재생
        vp.Play();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}
