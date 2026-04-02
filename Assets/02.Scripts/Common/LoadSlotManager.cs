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
            if (slotData.ep4_open) lastStage = 4;
            else if(slotData.ep3_open) lastStage = 3;
            else if(slotData.ep2_open) lastStage = 2;
            else if(slotData.ep1_open) lastStage = 1;
        }
        slotCheck.text = $"Stage {lastStage}";
    }
    public void SaveGame(int slotID)
    {
        SaveManager.instance.CreateSaveData(slotID);
    }
    public void LoadGame()
    {
        slotData = SaveManager.instance.LoadSaveData(slotIdx);
        if (slotData == null)
        {
            Debug.LogError("로드할 세이브 데이터가 없습니다.");
            return;
        }
        SaveManager.instance.curData = slotData;
        File.WriteAllText(Path.Combine(Application.persistentDataPath,$"CurData.json"), JsonUtility.ToJson(SaveManager.instance.curData, true));
        SceneManager.LoadSceneAsync($"LobbyScene");
    }
}