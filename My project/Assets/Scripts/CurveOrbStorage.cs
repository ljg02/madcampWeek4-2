using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // DOTween 네임스페이스 추가

public class CurveOrbStorage : MonoBehaviour
{
    public List<ShelfCurve> shelfCurves; // 여러 선반 곡선을 리스트로 관리
    public float moveDuration = 1f; // 애니메이션 지속 시간 (초)
    public Ease easeType = Ease.InOutQuad; // DOTween 이징 타입

    private List<List<Vector3>> allStoragePositions = new List<List<Vector3>>(); // 모든 선반의 저장 위치
    // allStoragePositions[0] : 0번 선반 커브 상의 지점들 리스트
    private List<bool[]> allStorageStates = new List<bool[]>(); // 각 선반 자리 상태 관리 (true: 구슬 있음, false: 비어 있음)
    private Queue<GameObject> orbQueue = new Queue<GameObject>();
    // 선반으로 이동시킬 오브젝트 대기 큐
    private HashSet<GameObject> processedOrbs = new HashSet<GameObject>(); // 처리된 오브젝트 추적
    private bool isProcessing = false;
    
    public static CurveOrbStorage Instance { get; private set; } // 싱글톤 인스턴스

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (shelfCurves == null || shelfCurves.Count == 0)
        {
            Debug.LogError("ShelfCurves 리스트가 비어 있습니다.");
            return;
        }

        // 모든 ShelfCurve의 저장 위치를 가져옴
        foreach (var shelf in shelfCurves)
        {
            var positions = shelf.GetShelfPositions();
            allStoragePositions.Add(positions);
            // Debug.Log($"ShelfCurve {shelf.name} returned {positions.Count} positions.");
            // 각 선반의 자리 상태를 초기화 (모두 비어 있음)
            allStorageStates.Add(new bool[positions.Count]);
        }
        
        // 디버그 로그로 저장 위치 확인
        for (int i = 0; i < allStoragePositions.Count; i++)
        {
            Debug.Log($"Shelf {i} Positions: {string.Join(", ", allStoragePositions[i])}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb"))
        {
            // 이미 처리된 오브젝트인지 확인
            if (processedOrbs.Contains(other.gameObject))
            {
                Debug.Log($"Orb {other.gameObject.name} is already processed. Ignoring.");
                return;
            }
            
            // 오브젝트의 부모가 자신인지 확인, 이미 자식이면 트리거 동작하지 않게
            if (other.transform.parent == transform)
            {
                Debug.Log($"Orb {other.gameObject.name} is already on the shelf. Ignoring.");
                return;
            }
            
            processedOrbs.Add(other.gameObject); // 처리된 오브젝트로 등록
            Debug.Log($"Orb {other.gameObject.name} triggered");
            orbQueue.Enqueue(other.gameObject);
            if (!isProcessing)
            {
                StartCoroutine(ProcessQueue());
            }
        }
    }

    private IEnumerator ProcessQueue()
    {
        isProcessing = true;
        
        while (orbQueue.Count > 0)
        {
            GameObject orb = orbQueue.Dequeue();

            // 저장 가능한 위치 찾기
            (int shelfIndex, int positionIndex)? target = GetNextAvailablePosition();
            if (target == null)
            {
                Debug.LogWarning("모든 선반이 가득 찼습니다.");
                break;
            }

            // 위치 정보
            int shelfIndex = target.Value.shelfIndex;
            int positionIndex = target.Value.positionIndex;
            Vector3 targetPosition = allStoragePositions[shelfIndex][positionIndex];

            // 자리 상태 업데이트
            allStorageStates[shelfIndex][positionIndex] = true;

            // Rigidbody 물리 비활성화
            Rigidbody rb = orb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // DOTween을 사용하여 구슬 이동
            Tween moveTween = orb.transform.DOMove(targetPosition, moveDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    // 부모 설정
                    orb.transform.SetParent(transform);
                    orb.transform.position = targetPosition;
                    orb.transform.localRotation = Quaternion.identity;
                });

            yield return moveTween.WaitForCompletion();
        }

        isProcessing = false;
    }

    // 다음 사용할 저장 위치를 반환
    private (int shelfIndex, int positionIndex)? GetNextAvailablePosition()
    {
        for (int shelfIndex = 0; shelfIndex < allStorageStates.Count; shelfIndex++)
        {
            for (int positionIndex = 0; positionIndex < allStorageStates[shelfIndex].Length; positionIndex++)
            {
                if (!allStorageStates[shelfIndex][positionIndex]) // 비어 있는 자리 찾기
                {
                    return (shelfIndex, positionIndex);
                }
            }
        }
        return null; // 모든 자리가 가득 찼을 경우
    }
    
    // 구슬이 선반에서 빠질 때 자리 비우기
    public void ReleaseOrb(GameObject orb)
    {
        for (int shelfIndex = 0; shelfIndex < allStoragePositions.Count; shelfIndex++)
        {
            for (int positionIndex = 0; positionIndex < allStoragePositions[shelfIndex].Count; positionIndex++)
            {
                if (Vector3.Distance(orb.transform.position, allStoragePositions[shelfIndex][positionIndex]) < 0.5f)
                {
                    allStorageStates[shelfIndex][positionIndex] = false; // 자리 비우기
                    processedOrbs.Remove(orb); // 처리된 오브젝트에서 제거
                    Debug.Log($"Orb {orb.name} removed from shelf position {shelfIndex}, {positionIndex}.");
                    return;
                }
            }
        }
    }
}
