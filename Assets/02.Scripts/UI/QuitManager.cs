using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class QuitManager : MonoBehaviour
{
    public GameObject QuitPopup;
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
    public void OpenQuitPopup()
    {
        QuitPopup.SetActive(true);
        Can.overrideSorting = true;
        Can.sortingOrder = 700;
        GameManager.Instance.openPopupCnt++;
        GameManager.Instance.lookLock = (GameManager.Instance.openPopupCnt > 0);
        GameManager.Instance.MouseStateChange();
    }
    public void OnNoButton()
    {
        QuitPopup.SetActive(false);
        Can.overrideSorting = false;
        Can.sortingOrder = 0;
        GameManager.Instance.openPopupCnt--;
        GameManager.Instance.lookLock = (GameManager.Instance.openPopupCnt > 0);
        GameManager.Instance.MouseStateChange();
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
