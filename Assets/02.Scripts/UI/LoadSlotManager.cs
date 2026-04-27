using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//버튼 터치 시 세이브 데이터를 로드하여 게임 시작
public class LoadSlotManager : MonoBehaviour
{
    public TMP_Text slotNum;
    public Text slotTime;
    public Text slotCheck;
    public int slotIdx;
    public SaveDataObj slotData;
    public Image slotImg;
    public Sprite ep4Slot;
    public Sprite ep3Slot;
    public Sprite ep2Slot;
    public Sprite ep1Slot;
    public Sprite ep0Slot;
    void OnEnable()
    {
        if (SaveManager.instance == null)
        {
            Debug.LogError("SaveManager.instance가 아직 초기화되지 않았습니다.");
            return;
        }
        slotData = SaveManager.instance.LoadSaveData(slotIdx);
        if (slotData == null)
        {
            slotNum.text = slotIdx.ToString();
            slotTime.text = "No Save Data";
            slotCheck.text = "-";
            return;
        }
        slotNum.text = slotData.ID.ToString();
        slotTime.text = slotData.savedTime;
        LastCheckpoint();
    }
    public void LastCheckpoint()
    {
        int lastStage = 0;
        if(slotData != null)
        {
            if (slotData.ep4_puzzle1Clear)
            {
                lastStage = 4;
                slotImg.sprite = ep4Slot;
            }
            else if (slotData.ep3_paperClear || slotData.ep3_jumpClear)
            {
                lastStage = 3;
                slotImg.sprite = ep3Slot;
            }
            else if (slotData.ep2_paintClear || slotData.ep2_spaceClear)
            {
                lastStage = 2;
                slotImg.sprite = ep2Slot;
            }
            else if (slotData.ep1_open)
            {
                lastStage = 1;
                slotImg.sprite = ep1Slot;
            }
            else
            {
                slotImg.sprite = ep0Slot;
            }
        }
        slotCheck.text = $"Stage {lastStage}";
    }
    public void SaveGame(int slotID) => SaveManager.instance.CreateSaveData(slotID);
    public void LoadGame()
    {
        if (SaveManager.instance == null) return;
        slotData = SaveManager.instance.LoadSaveData(slotIdx);
        if (slotData == null)
        {
            Debug.LogError("로드할 세이브 데이터가 없습니다.");
            return;
        }
        SaveManager.instance.curData = slotData;
        File.WriteAllText(Path.Combine(Application.persistentDataPath,$"CurData.json"), JsonUtility.ToJson(SaveManager.instance.curData, true));
        GameManager.Instance.openPopupCnt = 0;
        SceneManager.LoadSceneAsync($"LobbyScene");
    }
}