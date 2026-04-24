using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class QuitManager : MonoBehaviour
{
    public GameObject QuitPopup;
    private bool isQuitOpen = false; 
    private Canvas Can;
    private void Awake()
    {
        Can = QuitPopup.GetComponent<Canvas>();
        if (Can == null)
        {
            Can = QuitPopup.AddComponent<Canvas>();
        }
        var raycaster = QuitPopup.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            QuitPopup.AddComponent<GraphicRaycaster>();
        }
    }
    public void OnQuit(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if (isQuitOpen) return;
            OpenQuitPopup();
        }
    }
    public void OpenQuitPopup()
    {
        if (isQuitOpen) return;
        QuitPopup.SetActive(true);
        isQuitOpen = true;
        Can.overrideSorting = true;
        Can.sortingOrder = 700;
        GameManager.Instance.openPopupCnt++;
        GameManager.Instance.OnPopupChanged();
    }
    public void OnNoButton()
    {
        QuitPopup.SetActive(false);
        isQuitOpen = false;
        Can.overrideSorting = false;
        Can.sortingOrder = 0;
        GameManager.Instance.openPopupCnt--;
        GameManager.Instance.OnPopupChanged();
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
