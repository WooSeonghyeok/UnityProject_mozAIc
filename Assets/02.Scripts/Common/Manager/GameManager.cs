using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool lookLock = true;
    public int openPopupCnt = 0;
    public bool isCutsceneMode = false;
    public Image lookLockImg;
    public Image zoomCtrlImg;
    public Sprite zoomInImg;
    public Sprite zoomOutImg;
    private readonly string startScene = "StartScene";
    private readonly string openingScene = "OpeningScene";
    private readonly string endingScene = "EndingScene";
    private bool cursorHold = true;
    private PlayerInput user;
    private UnityEngine.InputSystem.PlayerInput input;
    private readonly string playerTag = "Player";
    private QuitManager quitManager;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ShowMouseState(true);
            DontDestroyOnLoad(gameObject);
        }
        else
        { 
            Destroy(gameObject);
            return;
        }
        input = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (user != null) user.ChangeLookLock -= ChangeLookLock;
    }
    private void Start()
    {
        GetOptionValue();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        user = FindUser();
        if (user != null) user.ChangeLookLock += ChangeLookLock;
        if (scene.name == startScene) quitManager = FindObjectOfType<QuitManager>(true);
        else quitManager = null;
        GetOptionValue();
        lookLock = (scene.name == startScene || scene.name == endingScene);  //스타트, 엔딩 신에서만 시점 고정이 활성화된 상태로 시작
        cursorHold = scene.name == startScene || scene.name == openingScene || scene.name == endingScene;  //스타트, 오프닝, 엔딩 신에서는 시점 고정 UI를 비활성화
        ShowMouseState(true);
        StartCoroutine(RegisterNextFrame());
    }
    private System.Collections.IEnumerator RegisterNextFrame()
    {
        yield return null;  // 한 프레임 대기
        RegisterInputEvents();
    }
    private PlayerInput FindUser()
    {
        var userObj = GameObject.FindGameObjectWithTag(playerTag);
        if (userObj == null) return null;
        else return userObj.GetComponent<PlayerInput>();
    }
    private void RegisterInputEvents()
    {
        // 기존 바인딩 해제 후 재등록
        var quitAction = input.actions.FindAction("Quit", throwIfNotFound: false);
        if (quitAction == null) return;
        quitAction.performed -= OnQuit;
        quitAction.performed += OnQuit;
        quitAction.Enable();
    }
    private void OnQuit(InputAction.CallbackContext context)
    {
        if (quitManager == null) return;
        quitManager.OnQuit(context);
    }
    private static void GetOptionValue()
    {
        SoundManager.Instance.SetMasterVolume(PlayerPrefs.GetFloat("Volume", 1f));
        SoundManager.Instance.SetBGMVolume(PlayerPrefs.GetFloat("BGM_Volume", 1f));
        SoundManager.Instance.SetAmbientVolume(PlayerPrefs.GetFloat("Ambient_Volume", 1f));
        SoundManager.Instance.SetUIVolume(PlayerPrefs.GetFloat("UI_Volume", 1f));
        SoundManager.Instance.SetSFXVolume(PlayerPrefs.GetFloat("SFX_Volume", 1f));
    }
    public void ChangeLookLock()  //시선 고정 on/off 키 입력
    {
        if (!cursorHold && !isCutsceneMode)  //컷신 재생 중이 아닌 경우에 동작함
        {
            lookLock = !lookLock;
            ShowMouseState(true);
        }
    }
    public void OnPopupChanged()
    {
        lookLock = cursorHold || (openPopupCnt > 0);
        ShowMouseState(true);
    }
    public void CutsceneMode(bool b)
    {
        isCutsceneMode = b;
        if (user != null)
        {
            var move = user.GetComponent<PlayerMovement>();
            move.SetMoveLock(b);
        }
        ShowMouseState(!b);
    }
    public void ShowMouseState(bool x)
    {
        lookLockImg.color = lookLock ? Color.red : Color.green;
        lookLockImg.gameObject.SetActive(!cursorHold && x);
        if (user != null && user.cameraSwitcher != null)
        {
            zoomCtrlImg.sprite = (user.cameraSwitcher.isFirstPerson) ? zoomOutImg : zoomInImg;
            zoomCtrlImg.gameObject.SetActive(x);
        }
        else zoomCtrlImg.gameObject.SetActive(false);
        Cursor.visible = lookLock || isCutsceneMode;  //시선 잠금 상태 OR 컷신 재생 중에 커서 노출
    }
}