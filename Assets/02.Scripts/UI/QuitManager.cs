using UnityEditor;
using UnityEngine;
public class QuitManager : MonoBehaviour
{
    public GameObject QuitPopup;
    public void OpenQuitPopup()
    {
        QuitPopup.SetActive(true);
        GameManager.Instance.lookLock = true;
        GameManager.Instance.CursorState();
    }
    public void OnNoButton()
    {
        QuitPopup.SetActive(false);
        GameManager.Instance.lookLock = false;
        GameManager.Instance.CursorState();
    }
    public void OnYesButton()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  //에디터 종료
#else
        Application.Quit();  //앱 종료
#endif
    }
}
