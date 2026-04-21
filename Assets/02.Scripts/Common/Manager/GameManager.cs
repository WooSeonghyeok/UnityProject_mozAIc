using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool lookLock = false;
    public bool isCutsceneMode = false;
    public Image lookLockImg;
    public Image zoomCtrlImg;
    public Sprite zoomInImg;
    public Sprite zoomOutImg;
    public string openingScene;
    public string endingScene;
    private PlayerInput user;
    private readonly string playerTag = "Player";
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GetOptionValue();
        }
        else Destroy(gameObject);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        user = FindUser();
        GetOptionValue();
        MouseState();
        lookLockImg.gameObject.SetActive(!(scene.name == openingScene || scene.name == endingScene));  //오프닝, 엔딩 신에서만 마우스 커서 이미지를 비활성화
    }
    private PlayerInput FindUser()
    {
        var userObj = GameObject.FindGameObjectWithTag(playerTag);
        if (userObj == null) return null;
        else return userObj.GetComponent<PlayerInput>();
    }
    private static void GetOptionValue()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1f);
        AudioListener.volume = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        AudioListener.volume = PlayerPrefs.GetFloat("Ambient_Volume", 1f);
        AudioListener.volume = PlayerPrefs.GetFloat("UI_Volume", 1f);
        AudioListener.volume = PlayerPrefs.GetFloat("SFX_Volume", 1f);
        AudioListener.volume = PlayerPrefs.GetFloat("Sensitivity", 0.5f);
    }
    public void OnCursorLock(InputAction.CallbackContext context)  //시선 고정 on/off
    {
        if (context.started)
        {
            lookLock = !lookLock;
            if (!isCutsceneMode) MouseState();
        }
    }
    public void ShowMouseState(bool x)
    {
        lookLockImg.color = lookLock ? Color.red : Color.green;
        lookLockImg.gameObject.SetActive(x);
        if (user != null && user.cameraSwitcher != null)
        {
            zoomCtrlImg.sprite = (user.cameraSwitcher.isFirstPerson) ? zoomOutImg : zoomInImg;
            zoomCtrlImg.gameObject.SetActive(x);
        }
        else zoomCtrlImg.gameObject.SetActive(false);
    }
    public void MouseState() => ShowMouseState(true);
    public void CutsceneMode(bool b)
    {
        isCutsceneMode = b;
        ShowMouseState(!b);
    }
}
