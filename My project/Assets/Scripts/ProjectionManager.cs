using UnityEngine;

public class ProjectionManager : MonoBehaviour
{
    public static ProjectionManager Instance;

    public GameObject projectorPrefab; // Projector 프리팹

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

    // 프로젝션 생성 메서드
    public void CreateProjection(Orb orb, Vector3 position)
    {
        if (projectorPrefab != null && orb.orbData != null)
        {
            Debug.Log($"ProjectionManager: Instantiating projector for {orb.orbData.orbName} at position {position}");
            // Projector 프리팹 인스턴스화
            GameObject projectorObj = Instantiate(projectorPrefab, position, projectorPrefab.transform.rotation);

            // Projector 컴포넌트 설정
            Projector projector = projectorObj.GetComponent<Projector>();
            if (projector != null)
            {
                // Orb 데이터에서 텍스처 가져오기
                Texture projTexture = orb.orbData.orbImage != null ? orb.orbData.orbImage.texture : null;

                // 텍스처 할당
                Material projMaterial = projector.material;
                if (projMaterial != null && projTexture != null)
                {
                    projMaterial.mainTexture = projTexture;
                }
            }
            else
            {
                Debug.LogWarning("ProjectionManager: Projector component not found on the prefab.");
            }

            // 필요 시 추가 설정 (예: 지속 시간, 삭제 타이머 등)
            Destroy(projectorObj, 5f); // 5초 후 자동 삭제
        }
    }
}