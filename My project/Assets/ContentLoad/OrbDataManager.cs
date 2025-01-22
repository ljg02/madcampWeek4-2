using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using SFB;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OrbDataManager : MonoBehaviour
{
    public OrbData orbData;
    public VideoPlayer videoPlayer;
    public RawImage orbVideoDisplay; 
    public Image orbImageDisplay;
    public TextMeshProUGUI orbTextDisplay;
    public TMP_InputField textInputField;
    
    [SerializeField] private Button videoButton;
    [SerializeField] private Button imageButton;
    [SerializeField] private Button saveButton;
    
    private bool isVideoSelected = false;
    private bool isImageSelected = false;
    private bool isTextEntered = false;
    
    private void Awake()
    {
        videoButton.onClick.AddListener(LoadVideoFromFile);
        imageButton.onClick.AddListener(LoadImageFromFile);
        saveButton.onClick.AddListener(SaveDataToServer);
        saveButton.interactable = false;  // 초기에는 비활성화
    }
    
    public void LoadVideoFromFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Video", "", "mp4", false);
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
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", "png", false);
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
        isTextEntered = !string.IsNullOrEmpty(orbData.orbText);
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
        WWWForm form = new WWWForm();
        
        if (isTextEntered)
            form.AddField("text", orbData.orbText);

        if (isImageSelected && orbData.orbImage is not null)
        {
            byte[] imageBytes = orbData.orbImage.texture.EncodeToPNG();
            form.AddBinaryData("image", imageBytes, "image.png", "image/png");
        }

        if (isVideoSelected && !string.IsNullOrEmpty(videoPlayer.url))
        {
            byte[] videoBytes = System.IO.File.ReadAllBytes(videoPlayer.url.Replace("file://", ""));
            form.AddBinaryData("video", videoBytes, "video.mp4", "video/mp4");
        }

        using (UnityWebRequest request = UnityWebRequest.Post("http://localhost:5000/upload", form))
        {
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

    private void OnDisable()
    {
        videoButton.onClick.RemoveListener(LoadVideoFromFile);
        imageButton.onClick.RemoveListener(LoadImageFromFile);
        saveButton.onClick.RemoveListener(SaveDataToServer);
    }
}
