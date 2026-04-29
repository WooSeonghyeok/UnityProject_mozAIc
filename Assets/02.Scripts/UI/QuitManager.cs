using UnityEngine;
using UnityEngine.InputSystem;
public class QuitManager : PopupOpenClose
{
    private bool isQuitOpen = false; 
    public void OnQuit(InputAction.CallbackContext context)
    {
        if (! isQuitOpen) OpenPopup();
    }
    public override void OpenPopup()
    {
        base.OpenPopup();
        isQuitOpen = true;
    }
    public void OnNoButton()
    {
        base.ClosePopup();
        isQuitOpen = false;
    }
    public void OnYesButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  //에디터 종료
#else
        Application.Quit();  //앱 종료
#endif
    }
}
