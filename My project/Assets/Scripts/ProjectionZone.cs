using UnityEngine;

//영역에 구슬이 들어오면 ProjectionManager를 통해 Projection을 생성
public class ProjectionZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb"))
        {
            Orb orb = other.GetComponent<Orb>();
            if (orb != null)
            {
                Vector3 projectionPosition = transform.position;
                Debug.Log($"ProjectionZone: Creating projection for {orb.orbData.orbName} at position {projectionPosition}");
                // 프로젝션 생성 요청
                //transform.position : 오브젝트의 현재 좌표
                ProjectionManager.Instance.CreateProjection(orb, transform.position);
                
                // 구슬을 비활성화하거나 다른 처리
                other.gameObject.SetActive(false);
            }
        }
    }
}