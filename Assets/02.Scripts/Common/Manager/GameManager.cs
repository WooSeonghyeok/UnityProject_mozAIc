using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool lookLock = false;
    public Image mouseImage;
    public string openingScene;
    public string endingScene;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1f);
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
        CursorState();
        mouseImage.gameObject.SetActive(!(scene.name == openingScene || scene.name == endingScene));  //오프닝, 엔딩 신에서만 마우스 커서 이미지를 비활성화
    }
    public void OnCursorLock(InputAction.CallbackContext context)  //시선 고정 on/off
    {
        if (context.started)
        {
            lookLock = !lookLock;
            CursorState();
        }
    }

    public void CursorState()
    {
        switch (lookLock)
        {
            case true:  mouseImage.color = Color.red;   break;
            case false: mouseImage.color = Color.green; break;
        }
    }
}
