using UnityEngine;
using System.Collections.Generic;

public class ShelfCurve : MonoBehaviour
{
    public List<Transform> controlPoints; // 베지어 곡선을 위한 제어점들
    public int resolution = 50; // 곡선을 따라 생성할 지점의 수

    private List<Vector3> shelfPositions = new List<Vector3>();

    void Start()
    {
        GenerateShelfPositions();
    }

    // 베지어 곡선 계산 (3점 베지어 곡선 예시)
    private void GenerateShelfPositions()
    {
        if (controlPoints.Count < 3)
        {
            Debug.LogError("베지어 곡선을 위해 최소 3개의 제어점이 필요합니다.");
            return;
        }

        // 3점 베지어 곡선 예시 (시작점, 제어점, 끝점)
        Vector3 p0 = controlPoints[0].position;
        Vector3 p1 = controlPoints[1].position;
        Vector3 p2 = controlPoints[2].position;

        shelfPositions.Clear();

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = CalculateQuadraticBezierPoint(t, p0, p1, p2);
            shelfPositions.Add(point);
        }
    }

    // 3점 베지어 곡선 계산 함수
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    // 생성된 선반 지점들 반환
    public List<Vector3> GetShelfPositions()
    {
        return shelfPositions;
    }
}