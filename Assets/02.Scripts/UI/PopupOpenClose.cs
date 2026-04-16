using UnityEngine;
public class PopupOpenClose : MonoBehaviour
{
    public GameObject Popup;
    public void OpenPopup() => Popup.SetActive(true);
    public void ClosePopup() => Popup.SetActive(false);
}
