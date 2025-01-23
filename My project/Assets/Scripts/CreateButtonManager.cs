using UnityEngine;

public class CreateButtonManager : MonoBehaviour
{
    // 활성화할 Canvas를 Inspector에서 할당할 수 있도록 public으로 선언
    public GameObject targetCanvas;
    
    // create버튼의 Collider
    private Collider textCollider;

    private void Start()
    {
        // Collider 컴포넌트를 가져옵니다.
        textCollider = GetComponent<Collider>();

        if (textCollider == null)
        {
            Debug.LogError("Collider 컴포넌트가 없습니다. Text Mesh 오브젝트에 Collider를 추가해주세요.");
        }

        // Canvas가 활성화되어 있는지 확인하고, 활성화 상태에 따라 Collider를 설정합니다.
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false); // 시작 시 Canvas를 비활성화
        }
    }

    // OnMouseDown은 마우스 버튼이 클릭되었을 때 호출됩니다.
    private void OnMouseDown()
    {
        if (targetCanvas != null)
        {
            // Canvas를 활성화
            targetCanvas.SetActive(true);
            
            // Collider를 비활성화하여 추가 클릭을 방지
            if (textCollider != null)
            {
                textCollider.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("Target Canvas가 할당되지 않았습니다.");
        }
    }
}