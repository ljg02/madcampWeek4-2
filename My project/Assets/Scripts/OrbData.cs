using UnityEngine;
using UnityEngine.Video; // VideoClip을 사용하기 위해 추가

[System.Serializable]
public class OrbData
{
    public string orbName;
    public Sprite orbImage; // 이미지 연동 시 사용
    public VideoClip orbVideo; // 영상 연동 시 사용
    // 추가적인 데이터 필드 추가 가능
}