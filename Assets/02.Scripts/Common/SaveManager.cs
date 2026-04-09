using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveDataObj curData;
    private static readonly string[] DefaultMemoryTagNames =
    {
        "shared_childhood",
        "star_promise",
        "shared_dream",
        "co_creation",
        "unfinished_confession",
        "lover_memory",
        "self_voice",
        "split_self"
    };
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        curData = ReadCurJSON();
        NormalizeTagCollections(curData);
    }
    public string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot{slot}.json");
    }
    public void CreateSaveData(int slotNumber)
    {
        SaveDataObj newData = new SaveDataObj();
        newData.ID = (byte)slotNumber;
        newData.savedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        newData.memory_reconstruction_rate = curData.memory_reconstruction_rate;
        newData.ep1_open = curData.ep1_open;
        newData.ep1_isCaveUnlocked = curData.ep1_isCaveUnlocked;
        newData.ep1_isPuzzleCleared = curData.ep1_isPuzzleCleared;
        newData.ep2_open = curData.ep2_open;
        newData.ep2_paintClear = curData.ep2_paintClear;
        newData.ep2_spaceClear = curData.ep2_spaceClear;
        newData.ep3_open = curData.ep3_open;
        newData.ep3_paperClear = curData.ep3_paperClear;
        newData.ep3_jumpClear = curData.ep3_jumpClear;
        newData.ep4_open = curData.ep4_open;
        newData.ep4_puzzle1Clear = curData.ep4_puzzle1Clear;
        newData.ep4_puzzle2Clear = curData.ep4_puzzle2Clear;
        newData.ep4_puzzle3Clear = curData.ep4_puzzle2Clear;
        NormalizeTagCollections(curData);
        newData.MemoryTag = CloneTags(curData.MemoryTag);
        newData.CoreTag = newData.MemoryTag;
        newData.isFirstEnterAtS3CP0 = curData.isFirstEnterAtS3CP0;
        newData.isFirstEnterAtEP3Lobby = curData.isFirstEnterAtEP3Lobby;
        string json = JsonUtility.ToJson(newData,true);
        File.WriteAllText(GetSavePath(slotNumber), json);  //선택한 슬롯에 세이브 데이터를 저장
        File.WriteAllText(Path.Combine(Application.persistentDataPath, $"CurData.json"), json);  //현재 데이터를 저장한 데이터로 갱신
        SaveUIManager.instance.CloseSavePopup();
        StartCoroutine(SaveUIManager.instance.SaveAlarm());
    }
    public SaveDataObj LoadSaveData(int slotNumber)
    {
        string path = GetSavePath(slotNumber);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"세이브 파일 없음: {path}");
            return null;
        }
        string json = File.ReadAllText(path);
        SaveDataObj data = JsonUtility.FromJson<SaveDataObj>(json);
        return data;
    }
    public static SaveDataObj ReadCurJSON()
    {
        SaveDataObj newData = new SaveDataObj();
        string path = Path.Combine(Application.persistentDataPath, $"CurData.json");
        if (!File.Exists(path))  //아직 파일이 없는 상태인 경우 기본 파일을 생성
        {
            SaveDataObj defaultSave = new SaveDataObj();
            CreateCurData(path, defaultSave);
            return defaultSave;
        }
        string jsonFile = File.ReadAllText(path);
        newData = JsonUtility.FromJson<SaveDataObj>(jsonFile);
        NormalizeTagCollections(newData);
        return newData;
    }
    public static void CreateCurData(string path, SaveDataObj dataObj)
    {
        dataObj.ID = 0;
        dataObj.savedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        dataObj.ep1_open = false;
        dataObj.ep1_isCaveUnlocked = false;
        dataObj.ep1_isPuzzleCleared = false;
        dataObj.ep2_open = false;
        dataObj.ep2_paintClear = false;
        dataObj.ep2_spaceClear = false;
        dataObj.ep3_open = false;
        dataObj.ep3_paperClear = false;
        dataObj.ep3_jumpClear = false;
        dataObj.ep4_open = false;
        dataObj.ep4_puzzle1Clear = false;
        dataObj.ep4_puzzle2Clear = false;
        dataObj.ep4_puzzle3Clear = false;
        dataObj.memory_reconstruction_rate = 0;
        dataObj.MemoryTag = CreateDefaultMemoryTags();
        dataObj.CoreTag = dataObj.MemoryTag;
        dataObj.isFirstEnterAtS3CP0 = false;
        dataObj.isFirstEnterAtEP3Lobby = false;
        string json = JsonUtility.ToJson(dataObj, true);
        File.WriteAllText(path, json);
    }
    public void WriteCurJSON()
    {
        SaveDataObj newData = new SaveDataObj();
        newData.ID = curData.ID;
        newData.savedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        newData.ep1_open = curData.ep1_open;
        newData.ep1_isCaveUnlocked = curData.ep1_isCaveUnlocked;
        newData.ep1_isPuzzleCleared = curData.ep1_isPuzzleCleared;
        newData.ep2_open = curData.ep2_open;
        newData.ep2_paintClear = curData.ep2_paintClear;
        newData.ep2_spaceClear = curData.ep2_spaceClear;
        newData.ep3_open = curData.ep3_open;
        newData.ep3_paperClear = curData.ep3_paperClear;
        newData.ep3_jumpClear = curData.ep3_jumpClear;
        newData.ep4_open = curData.ep4_open;
        newData.ep4_puzzle1Clear = curData.ep4_puzzle1Clear;
        newData.ep4_puzzle2Clear = curData.ep4_puzzle2Clear;
        newData.ep4_puzzle3Clear = curData.ep4_puzzle2Clear;
        newData.memory_reconstruction_rate = curData.memory_reconstruction_rate;
        NormalizeTagCollections(curData);
        newData.MemoryTag = CloneTags(curData.MemoryTag);
        newData.CoreTag = newData.MemoryTag;
        newData.isFirstEnterAtS3CP0 = curData.isFirstEnterAtS3CP0;
        newData.isFirstEnterAtEP3Lobby = curData.isFirstEnterAtEP3Lobby;
        string json = JsonUtility.ToJson(newData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, $"CurData.json"), json);  //현재 데이터 파일을 갱신
    }

    private static void NormalizeTagCollections(SaveDataObj dataObj)
    {
        if (dataObj == null)
        {
            return;
        }

        if ((dataObj.MemoryTag == null || dataObj.MemoryTag.Count == 0) &&
            (dataObj.CoreTag == null || dataObj.CoreTag.Count == 0))
        {
            dataObj.MemoryTag = CreateDefaultMemoryTags();
            dataObj.CoreTag = dataObj.MemoryTag;
            return;
        }

        if (dataObj.MemoryTag == null || dataObj.MemoryTag.Count == 0)
        {
            dataObj.MemoryTag = dataObj.CoreTag;
        }

        if (dataObj.CoreTag == null || dataObj.CoreTag.Count == 0)
        {
            dataObj.CoreTag = dataObj.MemoryTag;
        }
    }

    private static List<IsTagGet> CreateDefaultMemoryTags()
    {
        List<IsTagGet> tags = new List<IsTagGet>(DefaultMemoryTagNames.Length);
        foreach (string tagName in DefaultMemoryTagNames)
        {
            tags.Add(new IsTagGet
            {
                TagName = tagName,
                tagGet = false
            });
        }

        return tags;
    }

    private static List<IsTagGet> CloneTags(List<IsTagGet> source)
    {
        List<IsTagGet> cloned = new List<IsTagGet>();
        if (source == null)
        {
            return cloned;
        }

        foreach (IsTagGet tag in source)
        {
            if (tag == null)
            {
                continue;
            }

            cloned.Add(new IsTagGet
            {
                TagName = tag.TagName,
                tagGet = tag.tagGet
            });
        }

        return cloned;
    }
}
