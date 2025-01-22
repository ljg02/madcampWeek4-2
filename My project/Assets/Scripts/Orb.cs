using UnityEngine;
using DG.Tweening;

public class Orb : MonoBehaviour
{
    public OrbData orbData; // 각 구슬의 데이터
    
    private bool isGrabbed = false;
    private Vector3 offset;
    private Camera mainCamera;
    private Rigidbody rb;
    private float distanceToCamera = 3f;
    
    // Emission 관련 변수
    private Renderer orbRenderer;
    private Color originalEmissionColor;
    private Color glowColor = Color.white; // 원하는 발광 색상으로 설정

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody가 없으면 Orb 스크립트를 사용할 수 없습니다.");
        }
        
        orbRenderer = GetComponent<Renderer>();
        if (orbRenderer != null)
        {
            originalEmissionColor = orbRenderer.material.GetColor("_EmissionColor");
            DisableGlow();
        }
        else
        {
            Debug.LogError("Orb에 Renderer가 없습니다.");
        }
    }

    void Update()
    {
        if (isGrabbed)
        {
            MoveWithMouse();
        }
    }

    void OnMouseDown()
    {
        // 구슬을 잡았을 때
        isGrabbed = true;
        // 선반의 자식에서 탈출
        transform.SetParent(null);
        
        // 선반에서 자리 상태 제거
        CurveOrbStorage.Instance.ReleaseOrb(this.gameObject);
        
        // Rigidbody 물리 비활성화
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // 구슬을 카메라 앞 3 단위 위치로 이동시킴
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * distanceToCamera;
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.WorldToScreenPoint(targetPosition).z;
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        transform.DOMove(worldMousePosition, 0.5f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            // 이동 완료 후, 현재 마우스 위치와 구슬의 상대적 오프셋 계산
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = distanceToCamera;
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            offset = transform.position - worldMousePosition;
        });
    }

    void OnMouseUp()
    {
        // 구슬을 놓았을 때
        isGrabbed = false;

        // Rigidbody 물리 활성화
        rb.isKinematic = false;
    }

    private void MoveWithMouse()
    {
        // 마우스의 현재 위치를 가져와서 월드 좌표로 변환
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = distanceToCamera; // 카메라와의 거리를 z에 설정
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        Vector3 targetPosition = worldMousePosition + offset;

        // 원하는 방식으로 이동 (직접 위치 변경 또는 DOTween 사용)
        // 여기서는 DOTween을 사용하여 부드럽게 이동
        transform.DOMove(targetPosition, 0.1f).SetEase(Ease.Linear);
    }
    
    // 발광 활성화
    public void EnableGlow()
    {
        if (orbRenderer != null)
        {
            orbRenderer.material.EnableKeyword("_EMISSION");
            orbRenderer.material.SetColor("_EmissionColor", glowColor);
        }
    }

    // 발광 비활성화
    public void DisableGlow()
    {
        if (orbRenderer != null)
        {
            orbRenderer.material.DisableKeyword("_EMISSION");
            orbRenderer.material.SetColor("_EmissionColor", originalEmissionColor);
        }
    }
}