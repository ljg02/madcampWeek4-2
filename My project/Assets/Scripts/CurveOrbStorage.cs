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
    private Queue<GameObject> orbQueue = new Queue<GameObject>();
    // 선반으로 이동시킬 오브젝트 대기 큐
    private HashSet<GameObject> processedOrbs = new HashSet<GameObject>(); // 처리된 오브젝트 추적
    private int[] currentShelfStorageIndices; // 각 선반의 현재 저장 인덱스
    // currentShelfStorageIndices[0] : 0번 선반의 다음 저장 위치
    private bool isProcessing = false;

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
        }

        // 각 선반의 저장 인덱스 초기화
        currentShelfStorageIndices = new int[shelfCurves.Count];
        for (int i = 0; i < currentShelfStorageIndices.Length; i++)
        {
            currentShelfStorageIndices[i] = 0;
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

        Sequence sequence = DOTween.Sequence();

        while (orbQueue.Count > 0)
        {
            GameObject orb = orbQueue.Dequeue();

            // 저장 가능한 위치 찾기
            Vector3? targetPosition = GetNextAvailablePosition();
            if (targetPosition == null)
            {
                Debug.LogWarning("모든 선반이 가득 찼습니다.");
                // 큐에 남아 있는 구슬을 다시 넣고 종료
                orbQueue.Enqueue(orb);
                break;
            }

            // Rigidbody 물리 비활성화
            Rigidbody rb = orb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // DOTween을 사용하여 구슬 이동 애니메이션 추가
            Vector3 finalPosition = targetPosition.Value;
            Tween moveTween = orb.transform.DOMove(finalPosition, moveDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    // 부모 설정
                    orb.transform.SetParent(transform);
                    orb.transform.position = finalPosition; // 정확한 위치 설정
                    orb.transform.localRotation = Quaternion.identity;
                });

            sequence.Join(moveTween);
        }

        // 모든 구슬 애니메이션 완료 대기
        yield return sequence.WaitForCompletion();

        isProcessing = false;
    }



    // 다음 사용할 저장 위치를 반환
    private Vector3? GetNextAvailablePosition()
    {
        for (int i = 0; i < allStoragePositions.Count; i++)
        {
            if (currentShelfStorageIndices[i] < allStoragePositions[i].Count)
            {
                Vector3 pos = allStoragePositions[i][currentShelfStorageIndices[i]];
                currentShelfStorageIndices[i]++;
                return pos;
            }
        }

        // 모든 선반이 가득 찼을 때
        return null;
    }
    
    // 구슬이 선반에 놓였는지 확인
    public bool IsPositionOnShelf(Vector3 position)
    {
        foreach (var shelf in allStoragePositions)
        {
            foreach (var pos in shelf)
            {
                if (Vector3.Distance(position, pos) < 0.5f) // 거리 임계값 조정
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 구슬을 선반에 다시 등록
    public void RegisterOrbToShelf(GameObject orb, Vector3 position)
    {
        for (int i = 0; i < allStoragePositions.Count; i++)
        {
            for (int j = 0; j < allStoragePositions[i].Count; j++)
            {
                if (Vector3.Distance(position, allStoragePositions[i][j]) < 0.5f) // 거리 임계값 조정
                {
                    currentShelfStorageIndices[i] = Mathf.Max(currentShelfStorageIndices[i], j + 1);
                    break;
                }
            }
        }
    }
}
