using UnityEngine;
using UnityEngine.Video; // VideoClip을 사용하기 위해 추가
using UnityEngine.UI;

[System.Serializable]
public class OrbData
{
    public string orbName;
    public string orbText; //텍스트 연동
    public Sprite orbImage; // 이미지 연동 시 사용
    public VideoClip orbVideo; // 영상 연동 시 사용
    public Color orbColor; // 색상 연동 시 사용
    // 추가적인 데이터 필드 추가 가능
}