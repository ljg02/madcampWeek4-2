using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;
using System; // 이벤트를 사용하기 위해 추가


public class BeamProjector : MonoBehaviour
{
    // Screen Plane에 할당된 Renderer
    public Renderer screenRenderer;

    // VideoPlayer 컴포넌트 (비디오 재생 시 사용)
    public VideoPlayer videoPlayer;
    
    public ParticleSystem beamParticleSystem; // Particle System 참조
    
    public GameObject beamCone; // Cone 객체 참조
    
    public Light directionalLight; // 전역 조명 (Directional Light) 참조
    public Light pointLight; // PointLight 참조
    private float originalDirectionalIntensity; // 원래 Directional Light의 강도 저장
    private float targetPointIntensity = 5f; // Point Light의 목표 강도 설정 (필요에 따라 조정)

    // 현재 트리거 영역에 있는 Orb
    private Orb currentOrb;
    private bool isProcessing = false; // 상태 관리 변수 추가
    //private bool isMoving = false; // 이동 상태를 추적하는 플래그 추가
    private Tween vibrateTween; // 진동 트윈을 저장할 변수 추가
    private bool isVibrating = false; // 진동 상태 플래그

    // 초기 알파 값 설정
    private void Awake()
    {
        if (screenRenderer != null)
        {
            // Material Instance 생성하여 공유되지 않도록 함
            screenRenderer.material = new Material(screenRenderer.material);
            SetScreenAlpha(0f);
        }
        else
        {
            Debug.LogError("Screen Renderer가 할당되지 않았습니다.");
        }
        
        // 빔 Particle System 비활성화
        if (beamParticleSystem != null)
        {
            beamParticleSystem.Stop();
        }
        else
        {
            Debug.LogError("Beam Particle System이 할당되지 않았습니다.");
        }
        
        // 빔 Cone 비활성화
        if (beamCone != null)
        {
            beamCone.SetActive(false);
        }
        else
        {
            Debug.LogError("Beam Cone이 할당되지 않았습니다.");
        }
        
        // 조명 초기화
        if (directionalLight != null)
        {
            originalDirectionalIntensity = directionalLight.intensity;
            directionalLight.enabled = true;
        }
        else
        {
            Debug.LogError("Directional Light가 할당되지 않았습니다.");
        }
        
        // PointLight 초기화 (비활성화)
        if (pointLight != null)
        {
            pointLight.intensity = 0f; // 초기에는 Point Light를 끕니다.
            pointLight.enabled = false; // 초기에는 비활성화
        }
        else
        {
            Debug.LogError("PointLight가 할당되지 않았습니다.");
        }
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
            ProjectOrbContent(orb.orbData, orb);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"triggerExit, processing : {isProcessing}, Vibrating: {isVibrating}.");
        // 이동 중이라면 OnTriggerExit를 무시
        if (isVibrating) return;
        if (!isProcessing) return; // 재생 중이 아니라면 무시
        
        Orb orb = other.GetComponent<Orb>();
        if (orb != null && orb == currentOrb)
        {
            ClearScreen();
        }
    }
    
    private Vector3 GetHoverPosition()
    {
        // Define the hover height above the BeamProjector
        float hoverHeight = 0.5f; // Adjust as needed
        return transform.position + Vector3.up * hoverHeight;
    }

    private void ProjectOrbContent(OrbData orbData, Orb orb)
    {
        if (orbData.orbVideo != null && videoPlayer != null)
        {
            // 비디오 클립 설정 및 재생
            videoPlayer.clip = orbData.orbVideo;
            videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
            screenRenderer.material.mainTexture = videoPlayer.targetTexture;
            videoPlayer.Play();

            // Disable Rigidbody physics
            Rigidbody rb = orb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            isVibrating = true;
            
            SetBeamColors(orbData.orbColor);

            // Define the hover positions
            Vector3 initialHoverPosition = GetHoverPosition() + Vector3.up * 0.5f; // slightly above
            Vector3 finalHoverPosition = GetHoverPosition(); // final hover position

            // Create the main sequence
            Sequence mainSequence = DOTween.Sequence();
            
            // Set isMoving to true before starting movement
            //isMoving = true;

            // Append fade-in and move-up simultaneously
            mainSequence.Append(SetScreenAlpha(1f, 1f).SetEase(Ease.InOutQuad));
            //mainSequence.Join(orb.transform.DOMove(initialHoverPosition, 0.5f).SetEase(Ease.OutCubic));

            // Then move down to final hover position
            //mainSequence.Append(orb.transform.DOMove(finalHoverPosition, 0.5f).SetEase(Ease.InCubic));

            // After movement, activate beam and enable glow
            mainSequence.AppendCallback(() =>
            {
                //isMoving = false; // 이동 완료
                ActivateBeam();
                if (orb != null)
                {
                    orb.EnableGlow();
                    StartVibration(orb); // 진동 시작
                    
                    // Orb의 클릭 이벤트 구독
                    orb.OnOrbClicked += HandleOrbClicked;
                }
                else
                {
                    Debug.LogWarning("current orb is null@@@");
                }
            });

            // Start the sequence
            mainSequence.Play();
        }
        else if (orbData.orbImage != null && screenRenderer != null)
        {
            // 이미지 텍스처 설정
            screenRenderer.material.mainTexture = orbData.orbImage.texture;
            //videoPlayer.Stop();

            // Disable Rigidbody physics
            Rigidbody rb = orb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            isVibrating = true;
            
            SetBeamColors(orbData.orbColor);

            // Define the hover positions
            Vector3 initialHoverPosition = GetHoverPosition() + Vector3.up * 0.5f; // slightly above
            Vector3 finalHoverPosition = GetHoverPosition(); // final hover position

            // Create the main sequence
            Sequence mainSequence = DOTween.Sequence();
            
            // Set isMoving to true before starting movement
            //isMoving = true;

            // Append fade-in and move-up simultaneously
            mainSequence.Append(SetScreenAlpha(1f, 1f).SetEase(Ease.InOutQuad));
            //mainSequence.Join(orb.transform.DOMove(initialHoverPosition, 0.5f).SetEase(Ease.OutCubic));

            // Then move down to final hover position
            //mainSequence.Append(orb.transform.DOMove(finalHoverPosition, 0.5f).SetEase(Ease.InCubic));

            // After movement, activate beam and enable glow
            mainSequence.AppendCallback(() =>
            {
                //isMoving = false; // 이동 완료
                ActivateBeam();
                if (orb != null)
                {
                    orb.EnableGlow();
                    StartVibration(orb); // 진동 시작
                    
                    // Orb의 클릭 이벤트 구독
                    orb.OnOrbClicked += HandleOrbClicked;
                }
                else
                {
                    Debug.LogWarning("current orb is null@@@");
                }
            });

            // Start the sequence
            mainSequence.Play();
        }
        else
        {
            Debug.LogWarning("OrbData에 이미지나 비디오가 설정되지 않았습니다.");
            isProcessing = false; // 데이터가 없을 경우 처리 상태 해제
        }
    }
    
    private void HandleOrbClicked(Orb orb)
    {
        // Orb가 클릭되면 진동을 멈추고 화면을 클리어
        if (orb == currentOrb)
        {
            ClearScreen();
        }
    }

    private void ClearScreen()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            videoPlayer.targetTexture = null;
        }
        
        isVibrating = false;
        
        Sequence fadeOutSequence = DOTween.Sequence();

        fadeOutSequence.Append(SetScreenAlpha(1f, 1f));
        fadeOutSequence.Append(SetScreenAlpha(0f, 1f));

        fadeOutSequence.AppendCallback(() =>
        {
            Debug.Log("Fade-out completed.");
            isProcessing = false; // 페이드 아웃 완료 후 처리 상태 해제
            // 빔 비활성화 및 구슬 발광 비활성화
            DeactivateBeam();
            if (currentOrb != null)
            {
                currentOrb.DisableGlow();
                StopVibration(currentOrb); // 진동 중지
                
                // Orb의 클릭 이벤트 구독 해제
                currentOrb.OnOrbClicked -= HandleOrbClicked;
            }
            currentOrb = null; // Orb 제거 후 참조 해제
        });

        fadeOutSequence.Play();
    }
    
    // Tween을 반환하도록 수정
    private Tween SetScreenAlpha(float alpha, float duration = 0f)
    {
        if (screenRenderer == null)
        {
            Debug.LogError("Screen Renderer가 null입니다.");
            return null;
        }
        
        Color color = screenRenderer.material.color;
        // DOTween을 사용하여 알파 값을 변경하고 Tween 반환
        return DOTween.To(() => color.a, x => 
        {
            color.a = x;
            screenRenderer.material.color = color;
        }, alpha, duration);
    }
    
    private void StartVibration(Orb orb)
    {
        if (orb == null) return;
        
        //isVibrating = true; // 진동 상태 플래그 설정

        // 진동 파라미터 설정
        float vibrationAmplitude = 0.5f; // 진동 폭 조절
        float vibrationDuration = 1f;    // 각 진동의 지속 시간

        // 현재 위치 저장
        Vector3 originalPosition = orb.transform.position;

        // 지속적인 진동 트윈 생성
        vibrateTween = orb.transform.DOMoveY(
                originalPosition.y + vibrationAmplitude,
                vibrationDuration
            )
            .SetLoops(-1, LoopType.Yoyo) // 무한 반복, Yoyo 방식
            .SetEase(Ease.InOutSine);
    }

    private void StopVibration(Orb orb)
    {
        if (vibrateTween != null && vibrateTween.IsActive())
        {
            vibrateTween.Kill(); // 트윈 중지
        }
        
        //isVibrating = false; // 진동 상태 플래그 해제

        if (orb != null)
        {
            // 원래 위치로 복귀 (필요 시)
            orb.transform.DOMoveY(GetHoverPosition().y, 0.2f).SetEase(Ease.OutSine);
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        // 비디오가 준비되면 재생
        vp.Play();
    }
    
    // ActivateBeam 메서드 수정
    private void ActivateBeam()
    {
        if (beamCone != null)
        {
            beamCone.SetActive(true);
        }
        
        if (beamParticleSystem != null)
        {
            beamParticleSystem.Play();
        }
        
        // 화면 어둡게 하기 (예: Alpha 0.5)
        DimScreen(0.5f, 1f);
        
        // Directional Light 비활성화 및 PointLight 활성화
        if (directionalLight != null)
        {
            directionalLight.DOIntensity(0f, 1f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                directionalLight.enabled = false; // 완전히 어두워지면 비활성화
            });
        }
        if (pointLight != null)
        {
            pointLight.enabled = true; // 활성화
            pointLight.DOIntensity(targetPointIntensity, 1f).SetEase(Ease.InOutQuad);
        }
    }

    // DeactivateBeam 메서드 수정
    private void DeactivateBeam()
    {
        if (beamCone != null)
        {
            beamCone.SetActive(false);
        }
        
        if (beamParticleSystem != null)
        {
            beamParticleSystem.Stop();
        }
        
        // 화면 원래대로 되돌리기 (Alpha 0)
        DimScreen(0f, 1f);
        
        // Point Light 서서히 어둡게
        if (pointLight != null)
        {
            pointLight.DOIntensity(0f, 1f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                pointLight.enabled = false; // 완전히 어두워지면 비활성화
            });
        }
        // Directional Light 서서히 밝게
        if (directionalLight != null)
        {
            directionalLight.enabled = true; // 활성화
            directionalLight.DOIntensity(originalDirectionalIntensity, 1f).SetEase(Ease.InOutQuad);
        }
    }
    
    private void SetBeamColors(Color targetColor)
    {
        // beamCone의 색상 설정
        if (beamCone != null)
        {
            Renderer beamRenderer = beamCone.GetComponent<Renderer>();
            if (beamRenderer != null)
            {
                // Material 인스턴스 생성하여 공유되지 않도록 함
                beamRenderer.material = new Material(beamRenderer.material);
                // 기존 색상 가져오기
                Color currentColor = beamRenderer.material.GetColor("_Color");

                // 새로운 색상 생성 (RGB는 orbColor에서, Alpha는 기존 유지)
                Color newColor = new Color(targetColor.r, targetColor.g, targetColor.b, currentColor.a);

                // Albedo 색상 설정
                //beamRenderer.material.SetColor("_Color", newColor);

                // Emission 비활성화
                //beamRenderer.material.DisableKeyword("_EMISSION");
                //beamRenderer.material.SetColor("_EmissionColor", Color.black); // Emission 색상을 검정색으로 설정하여 비활성화
            }
            else
            {
                Debug.LogError("BeamCone Renderer가 할당되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogError("BeamCone이 할당되지 않았습니다.");
        }

        // pointLight의 색상 설정
        if (pointLight != null)
        {
            pointLight.color = new Color(targetColor.r, targetColor.g, targetColor.b, pointLight.color.a);
        }
        else
        {
            Debug.LogError("PointLight가 할당되지 않았습니다.");
        }
    }

    
    private void DimScreen(float targetAlpha, float duration)
    {
        if (directionalLight != null)
        {
            // DOTween을 사용하여 조명 강도 변경
            //directionalLight.DOIntensity(targetAlpha > 0 ? originalLightIntensity * 0.1f : originalLightIntensity, duration).SetEase(Ease.InOutQuad);
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
        
        // 현재 orb가 있다면 이벤트 구독 해제
        if (currentOrb != null)
        {
            currentOrb.OnOrbClicked -= HandleOrbClicked;
        }
    }
}
