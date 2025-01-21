using UnityEngine;

//구슬이 트리거 영역에 들어오면 storageParent위치로 전송
public class OrbStorage : MonoBehaviour
{
    public Transform storageParent; // 구슬을 저장할 부모 트랜스폼

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb"))
        {
            StoreOrb(other.gameObject);
        }
    }

    private void StoreOrb(GameObject orb)
    {
        // Rigidbody 설정
        Rigidbody rb = orb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // 물리 계산 비활성화
            rb.velocity = Vector3.zero; // 현재 속도 초기화
            rb.angularVelocity = Vector3.zero; // 현재 각속도 초기화
        }

        // 구슬의 부모 설정
        orb.transform.SetParent(storageParent);

        // 구슬의 위치 및 회전 초기화
        orb.transform.localPosition = Vector3.zero; // 필요에 따라 조정
        orb.transform.localRotation = Quaternion.identity; // 필요에 따라 조정
    }
}
