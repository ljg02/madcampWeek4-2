using UnityEngine;
using System.Collections;

//카메라와 triggerArea를 연결해주면 해당 영역에 구슬이 들어왔을 때 UIManager 수행
public class OrbShowUI : MonoBehaviour
{
    public Camera mainCamera; // 메인 카메라 참조
    public Collider triggerArea; // 저장소의 트리거 콜라이더

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb"))
        {
            Orb orb = other.GetComponent<Orb>();
            if (orb != null)
            {
                // UIManager를 통해 UI 표시
                UIManager.Instance.ShowOrb(orb);

                // 구슬 오브젝트 비활성화 또는 다른 처리
                other.gameObject.SetActive(false);
            }
        }
    }
}