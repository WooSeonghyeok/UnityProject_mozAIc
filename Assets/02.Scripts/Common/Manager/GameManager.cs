using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool lookLock = true;
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
            MouseState();
            DontDestroyOnLoad(gameObject);
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
    private void Start()
    {
        GetOptionValue();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        user = FindUser();
        GetOptionValue();
        MouseState();
        lookLockImg.gameObject.SetActive(!(scene.name == openingScene || scene.name == endingScene));  //오프닝, 엔딩 신에서는 마우스 커서 이미지를 비활성화
    }
    private PlayerInput FindUser()
    {
        var userObj = GameObject.FindGameObjectWithTag(playerTag);
        if (userObj == null) return null;
        else return userObj.GetComponent<PlayerInput>();
    }
    private static void GetOptionValue()
    {
        SoundManager.Instance.SetMasterVolume(PlayerPrefs.GetFloat("Volume", 1f));
        SoundManager.Instance.SetBGMVolume(PlayerPrefs.GetFloat("BGM_Volume", 1f));
        SoundManager.Instance.SetAmbientVolume(PlayerPrefs.GetFloat("Ambient_Volume", 1f));
        SoundManager.Instance.SetUIVolume(PlayerPrefs.GetFloat("UI_Volume", 1f));
        SoundManager.Instance.SetSFXVolume(PlayerPrefs.GetFloat("SFX_Volume", 1f));
        AudioListener.volume = PlayerPrefs.GetFloat("Sensitivity", 0.5f);
    }
    public void OnLookLock(InputAction.CallbackContext context)  //시선 고정 on/off 키 입력
    {
        if (context.started)
        {
            if (!isCutsceneMode)  //컷신 재생 중이 아닌 경우에 동작함
            {
                lookLock = !lookLock;
                MouseState();
            }
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
        Cursor.visible = lookLock || isCutsceneMode;  //시선 잠금 상태 OR 컷신 재생 중에 커서 노출
    }
    public void MouseState() => ShowMouseState(true);
    public void CutsceneMode(bool b)
    {
        isCutsceneMode = b;
        ShowMouseState(!b);
    }
}
