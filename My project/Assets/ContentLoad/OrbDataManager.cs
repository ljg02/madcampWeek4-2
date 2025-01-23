using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using SFB;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class EmotionRequestData
{
    public string text;
}
public class OrbDataManager : MonoBehaviour
{
    public OrbData orbData;
    public VideoPlayer videoPlayer;
    public RawImage orbVideoDisplay; 
    public Image orbImageDisplay;
    public TextMeshProUGUI orbTextDisplay;
    public TMP_InputField textInputField;
    
    public GameObject createPageCanvas;   // 오브 생성 페이지 캔버스
    
    [SerializeField] private Button videoButton;
    [SerializeField] private Button imageButton;
    [SerializeField] private Button saveButton;
    
    private bool isVideoSelected = false;
    private bool isImageSelected = false;
    private bool isTextEntered = false;
    
    public GameObject orbPrefab; // 인스펙터에서 할당할 프리팹
    public Transform spawnParent; // 프리팹이 생성될 캔버스의 부모 (UI인 경우)
    
    private void Awake()
    {   
        textInputField.onValueChanged.AddListener(delegate { SetOrbText(); });
        videoButton.onClick.AddListener(LoadVideoFromFile);
        imageButton.onClick.AddListener(LoadImageFromFile);
        saveButton.onClick.AddListener(SaveDataToServer);
        saveButton.interactable = false;  // 초기에는 비활성화
    }
    
    public void LoadVideoFromFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Video", "", "mp4", false);
        Debug.Log(paths);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string videoPath = "file://" + paths[0];
            videoPlayer.url = videoPath;
            videoPlayer.Play();
            isVideoSelected = true;
            CheckSaveButton();
        }
    }

    public void LoadImageFromFile()
    {
        // Define extension filters
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            StartCoroutine(LoadImage(paths[0]));
        }
    }

    private IEnumerator LoadImage(string path)
    {
        using (var imageRequest = UnityWebRequestTexture.GetTexture("file://" + path))
        {
            yield return imageRequest.SendWebRequest();
            if (imageRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                orbData.orbImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                orbImageDisplay.sprite = orbData.orbImage;
                isImageSelected = true;
                CheckSaveButton();
            }
            else
            {
                Debug.LogError("Failed to load image: " + imageRequest.error);
            }
        }
    }

    public void SetOrbText()
    {
        orbData.orbText = textInputField.text;
        orbTextDisplay.text = orbData.orbText;
        isTextEntered = true;
        CheckSaveButton();
    }

    private void CheckSaveButton()
    {
        saveButton.interactable = isVideoSelected || isImageSelected || isTextEntered;
    }

    private void SaveDataToServer()
    {
        StartCoroutine(SendDataToServer());
    }

    private IEnumerator SendDataToServer()
    {
        // 프리팹 인스턴스화
        InstantiateOrbPrefab();
        createPageCanvas.SetActive(false);
        
        WWWForm form = new WWWForm();
        
        if (isTextEntered)
        {
            // var jsonData = new { text = orbData.orbText }; // 감정 분석용 JSON 데이터
            form.AddField("text",orbData.orbText);
        }
        if (isImageSelected && orbData.orbImage is not null)
        {
            byte[] imageBytes = orbData.orbImage.texture.EncodeToPNG();
            form.AddBinaryData("files", imageBytes, "image.jpg", "image/jpg");
        }

        if (isVideoSelected && !string.IsNullOrEmpty(videoPlayer.url))
        {
            byte[] videoBytes = System.IO.File.ReadAllBytes(videoPlayer.url.Replace("file://", ""));
            form.AddBinaryData("files", videoBytes, "video.mp4", "video/mp4");
        }
        
        using (UnityWebRequest request = UnityWebRequest.Post("http://localhost:5000/uploads", form))
        {
            Debug.Log(request.result);
            Debug.Log(UnityWebRequest.Result.Success);
            yield return request.SendWebRequest(); 
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data successfully sent to server.");
            }
            else
            {
                Debug.LogError("Error sending data: " + request.error);
            }
        }
    }
    
    private void InstantiateOrbPrefab()
    {
        if (orbPrefab != null && spawnParent != null)
        {
            // 프리팹을 spawnParent 위치 근처에 인스턴스화 (랜덤 위치 예시)
            Vector3 spawnPosition = spawnParent.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            Quaternion spawnRotation = Quaternion.identity; // 기본 회전
            GameObject newOrb = Instantiate(orbPrefab, spawnPosition, spawnRotation);

            // Orb 스크립트 컴포넌트 가져오기
            Orb orbScript = newOrb.GetComponent<Orb>();
            if (orbScript != null)
            {
                // orbData 할당
                orbScript.orbData = new OrbData
                {
                    orbName = orbData.orbName, // 필요 시 설정
                    orbText = orbData.orbText,
                    orbImage = orbData.orbImage,
                    orbVideo = orbData.orbVideo
                };
            }
            else
            {
                Debug.LogError("Orb 스크립트 컴포넌트를 프리팹에서 찾을 수 없습니다.");
            }

            // 데이터 초기화 (옵션)
            ResetOrbData();
        }
        else
        {
            if (orbPrefab == null)
                Debug.LogError("orbPrefab이 인스펙터에서 할당되지 않았습니다.");
            if (spawnParent == null)
                Debug.LogError("spawnParent가 인스펙터에서 할당되지 않았습니다.");
        }
    }
    
    private void ResetOrbData()
    {
        orbData.orbText = "";
        orbData.orbImage = null;
        orbData.orbVideo = null;
        isVideoSelected = false;
        isImageSelected = false;
        isTextEntered = false;
        saveButton.interactable = false;
        textInputField.text = "";
        orbImageDisplay.sprite = null;
        orbTextDisplay.text = "";
    }

    private void OnDisable()
    {
        videoButton.onClick.RemoveListener(LoadVideoFromFile);
        imageButton.onClick.RemoveListener(LoadImageFromFile);
        saveButton.onClick.RemoveListener(SaveDataToServer);
    }
}
