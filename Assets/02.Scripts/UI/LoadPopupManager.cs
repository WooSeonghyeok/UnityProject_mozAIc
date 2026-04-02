using UnityEngine;
public class LoadPopupManager : MonoBehaviour
{
    public GameObject LoadPopup;
    public void OpenLoadPopup() => LoadPopup.SetActive(true);
    public void CloseLoadPopup() => LoadPopup.SetActive(false);
}