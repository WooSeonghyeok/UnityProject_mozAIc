using System.Collections;
using TMPro;
using UnityEngine;
public class SaveUIManager : MonoBehaviour
{
    public static SaveUIManager instance;
    public GameObject interactUI;
    public GameObject savedAlarm;
    public GameObject SavePopup;
    public TMP_Text savedText;
    private WaitForSeconds savedWS;
    public SaveDataObj curData;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
        savedWS = new WaitForSeconds(2f);
        interactUI.SetActive(false);
        savedAlarm.SetActive(false);
    }
    public void InteractUIOpen(bool b) => interactUI.SetActive(b);
    public void OpenSavePopup() => SavePopup.SetActive(true);
    public void CloseSavePopup() => SavePopup.SetActive(false);
    public IEnumerator SaveAlarm(int slotNumber)
    {
        curData = SaveManager.instance.curData;
        savedText.text = $"Slot {slotNumber} Saved";
        savedAlarm.SetActive(true);
        yield return savedWS;
        savedAlarm.SetActive(false);
    }
}