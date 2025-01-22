using UnityEngine;
using DG.Tweening;

public class AutoOrbStorage : MonoBehaviour
{
    public Transform storageParent; // 구슬을 저장할 부모 트랜스폼
    public float moveDuration = 1f; // 애니메이션 지속 시간 (초)

    void Awake()
    {
        if (storageParent == null)
        {
            storageParent = transform.Find("StorageParent");
            if (storageParent == null)
            {
                Debug.LogError("StorageParent 오브젝트를 찾을 수 없습니다.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb"))
        {
            StoreOrb(other.gameObject);
        }
    }

    private void StoreOrb(GameObject orb)
    {
        // Rigidbody 물리 비활성화
        Rigidbody rb = orb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // 물리 계산 비활성화
            rb.velocity = Vector3.zero; // 현재 속도 초기화
            rb.angularVelocity = Vector3.zero; // 현재 각속도 초기화
        }

        // DOTween을 사용하여 위치와 회전 애니메이션 적용
        orb.transform.DOMove(storageParent.position, moveDuration).SetEase(Ease.InOutQuad);
        orb.transform.DORotate(storageParent.rotation.eulerAngles, moveDuration).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                // 애니메이션 완료 후 부모 설정
                orb.transform.SetParent(storageParent);
                orb.transform.localPosition = Vector3.zero;
                orb.transform.localRotation = Quaternion.identity;
            });
    }
}