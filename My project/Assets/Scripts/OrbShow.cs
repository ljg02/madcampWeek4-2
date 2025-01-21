using UnityEngine;
using System.Collections;

public class OrbShow : MonoBehaviour
{
    public Transform storageParent; // 구슬을 저장할 부모 트랜스폼
    public Camera mainCamera; // 메인 카메라 참조
    public float animationDuration = 1f; // 애니메이션 지속 시간
    public Vector3 targetPositionOffset = new Vector3(0, 0, 2f); // 카메라 앞으로의 오프셋
    public Vector3 targetScale = new Vector3(2f, 2f, 2f); // 최종 스케일

    private bool isAnimating = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb") && !isAnimating)
        {
            StartCoroutine(AnimateOrb(other.gameObject));
        }
    }

    private IEnumerator AnimateOrb(GameObject orb)
    {
        isAnimating = true;

        // Rigidbody 설정
        Rigidbody rb = orb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // 물리 계산 비활성화
            rb.velocity = Vector3.zero; // 현재 속도 초기화
            rb.angularVelocity = Vector3.zero; // 현재 각속도 초기화
        }

        // 애니메이션 시작 전 원래 위치와 스케일 저장
        Vector3 initialPosition = orb.transform.position;
        Vector3 initialScale = orb.transform.localScale;

        // 카메라 앞의 목표 위치 계산
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * targetPositionOffset.z +
                                 mainCamera.transform.up * targetPositionOffset.y +
                                 mainCamera.transform.right * targetPositionOffset.x;

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;

            // 선형 보간을 사용하여 위치와 스케일 변경
            orb.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            orb.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 애니메이션 완료 후 정확한 위치와 스케일 설정
        orb.transform.position = targetPosition;
        orb.transform.localScale = targetScale;

        // 추가 처리: 예를 들어, UI에 표시하거나 다른 동작 수행
        // 현재는 애니메이션 완료 후 구슬을 비활성화
        //orb.SetActive(false);

        isAnimating = false;
    }
}
