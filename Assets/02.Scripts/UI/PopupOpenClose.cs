using UnityEngine;
public class PopupOpenClose : MonoBehaviour
{
    public GameObject Popup;
    public void OpenPopup()
    {
        Popup.SetActive(true);
        GameManager.Instance.lookLock = true;
        GameManager.Instance.CursorState();
    }
    public void ClosePopup()
    {
        Popup.SetActive(false);
        GameManager.Instance.lookLock = false;
        GameManager.Instance.CursorState();
    }
}
