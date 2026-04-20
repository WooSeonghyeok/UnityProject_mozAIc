using UnityEngine;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1f);
        }
        else Destroy(gameObject);
        Cursor.lockState = CursorLockMode.Confined;  // 마우스 커서를 창 화면 내에 고정
    }
    public void OnCursorLock(InputAction.CallbackContext context)  //마우스 고정 버튼
    {
        if (context.started)
        {
            if (Cursor.lockState == CursorLockMode.Confined)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }
}
