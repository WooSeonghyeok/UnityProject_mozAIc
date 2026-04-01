using UnityEditor;
using UnityEngine;
public class QuitManager : MonoBehaviour
{
    public GameObject QuitPopup;
    public void OpenQuitPopup() => QuitPopup.SetActive(true);
    public void OnNoButton() => QuitPopup.SetActive(false);
    public void OnYesButton()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  //에디터 종료
#else
        Application.Quit();  //앱 종료
#endif
    }
}
