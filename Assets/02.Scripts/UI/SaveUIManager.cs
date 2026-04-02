using System.Collections;
using TMPro;
using UnityEngine;
public class SaveUIManager : MonoBehaviour
{
    public static SaveUIManager instance;
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
        savedAlarm.SetActive(false);
    }
    public void OpenSavePopup() => SavePopup.SetActive(true);
    public void CloseSavePopup() => SavePopup.SetActive(false);
    public IEnumerator SaveAlarm()
    {
        curData = SaveManager.ReadCurJSON();
        savedText.text = $"Slot {curData.ID} Saved";
        savedAlarm.SetActive(true);
        yield return savedWS;
        savedAlarm.SetActive(false);
    }
}