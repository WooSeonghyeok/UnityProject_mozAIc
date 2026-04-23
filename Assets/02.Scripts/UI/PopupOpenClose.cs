using UnityEngine;
using UnityEngine.UI;
public class PopupOpenClose : MonoBehaviour
{
    public GameObject Popup;
    public int sort = 0;
    private Canvas Can;
    private void Awake ()
    {
        Can = Popup.GetComponent<Canvas>();
        if (Can == null)
        {
            Can = Popup.AddComponent<Canvas>();
        }
        var raycaster = Popup.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            Popup.AddComponent<GraphicRaycaster>();
        }
    }
    public void OpenPopup()
    {
        Popup.SetActive(true);
        Can.overrideSorting = true;
        Can.sortingOrder = sort;
        GameManager.Instance.lookLock = true;
        GameManager.Instance.MouseState();
    }
    public void ClosePopup()
    {
        Popup.SetActive(false);
        Can.overrideSorting = false;
        Can.sortingOrder = 0;
        GameManager.Instance.lookLock = false;
        GameManager.Instance.MouseState();
    }
}