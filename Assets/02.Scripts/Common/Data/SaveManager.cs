using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveDataObj curData;
    public static readonly string[] DefaultMemoryTagNames =
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
    [Serializable]
    public class NPCInfoWrapper
    {
        public NPCInfo[] list;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        curData = ReadCurJSON();
    }
    public void CreateSaveData(int slotNumber)
    {
        curData.ID = (byte)slotNumber;
        curData.savedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string json = JsonUtility.ToJson(curData,true);
        File.WriteAllText(GetSavePath(slotNumber), json);  //선택한 슬롯에 세이브 데이터를 저장
        File.WriteAllText(Path.Combine(Application.persistentDataPath, $"CurData.json"), json);  //현재 데이터를 저장한 데이터로 갱신
        SaveUIManager.instance.CloseSavePopup();
        StartCoroutine(SaveUIManager.instance.SaveAlarm(slotNumber));
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
    public string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot{slot}.json");
    }
    public static SaveDataObj ReadCurJSON()
    {
        string path = Path.Combine(Application.persistentDataPath, $"CurData.json");
        if (!File.Exists(path))  //아직 파일이 없는 상태인 경우 기본 파일을 생성
        {
            SaveDataObj defaultSave = new SaveDataObj();
            CreateCurData(path, defaultSave);
            return defaultSave;
        }
        string jsonFile = File.ReadAllText(path);
        SaveDataObj newData = JsonUtility.FromJson<SaveDataObj>(jsonFile);
        return NormalizeSaveData(newData);
    }
    public void ResetCurData()
    {
        string path = Path.Combine(Application.persistentDataPath, $"CurData.json");
        SaveDataObj defaultSave = new SaveDataObj();
        CreateCurData(path, defaultSave);
        curData = defaultSave;
    }
    public static void CreateCurData(string path, SaveDataObj dataObj)
    {
        dataObj.ID = 0;
        dataObj.savedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        dataObj.memory_reconstruction_rate = new int[13];
        dataObj.memory_reconstruction_rate[0] = 5;  //기억 재구성 기본 점수 5점으로 시작
        dataObj.CoreTag = CreateDefaultMemoryTags();
        dataObj.npcInformations = LoadDefaultNPCInfo();
        dataObj.Played_EP3_LobbyIntro = false;
        dataObj.Played_EP3_Stage3_1Intro = false;
        dataObj.Played_EP3_Stage3_1Completion = false;
        dataObj.Played_EP3_Stage3_2Intro = false;
        dataObj.Played_EP3_ReturnedLobbyIntro = false;
        dataObj.isFirstEnterAtS3CP0 = false;
        dataObj.isFirstEnterAtEP3Lobby = false;
        dataObj.isFirstEnterAtEP3_1 = false;
        dataObj.Played_EP3_LobbyIntro = false;
        dataObj.Played_EP3_Stage3_1Intro = false;
        dataObj.Played_EP3_Stage3_1Completion = false;
        dataObj.Played_EP3_Stage3_2Intro = false;
        dataObj.Played_EP3_ReturnedLobbyIntro = false;
        string json = JsonUtility.ToJson(dataObj, true);
        File.WriteAllText(path, json);
    }

    public static SaveDataObj NormalizeSaveData(SaveDataObj dataObj)
    {
        if (dataObj == null)
        {
            dataObj = new SaveDataObj();
        }

        if (dataObj.memory_reconstruction_rate == null || dataObj.memory_reconstruction_rate.Length != 13)
        {
            int[] normalizedRate = new int[13];
            if (dataObj.memory_reconstruction_rate != null)
            {
                Array.Copy(dataObj.memory_reconstruction_rate, normalizedRate, Mathf.Min(dataObj.memory_reconstruction_rate.Length, normalizedRate.Length));
            }

            if (normalizedRate[0] <= 0)
            {
                normalizedRate[0] = 5;
            }

            dataObj.memory_reconstruction_rate = normalizedRate;
        }

        if (dataObj.CoreTag == null || dataObj.CoreTag.Count == 0)
        {
            dataObj.CoreTag = CreateDefaultMemoryTags();
        }
        else
        {
            foreach (string defaultTagName in DefaultMemoryTagNames)
            {
                bool exists = dataObj.CoreTag.Exists(tag => tag != null && tag.TagName == defaultTagName);
                if (!exists)
                {
                    dataObj.CoreTag.Add(new IsTagGet
                    {
                        TagName = defaultTagName,
                        tagGet = false
                    });
                }
            }
        }

        List<NPCInfo> defaultNpcInfo = LoadDefaultNPCInfo();
        if (dataObj.npcInformations == null)
        {
            dataObj.npcInformations = new List<NPCInfo>();
        }

        dataObj.npcInformations.RemoveAll(npc => npc == null || string.IsNullOrWhiteSpace(npc.npcId));

        foreach (NPCInfo defaultNpc in defaultNpcInfo)
        {
            if (defaultNpc == null || string.IsNullOrWhiteSpace(defaultNpc.npcId))
            {
                continue;
            }

            NPCInfo existingNpc = dataObj.npcInformations.Find(npc => npc.npcId == defaultNpc.npcId);
            if (existingNpc == null)
            {
                dataObj.npcInformations.Add(CloneNpcInfo(defaultNpc));
                continue;
            }

            if (existingNpc.words == null || existingNpc.words.Count == 0)
            {
                existingNpc.words = CloneMemoryKeywords(defaultNpc.words);
            }
        }

        if (dataObj.isFirstEnterAtEP3Lobby)
        {
            dataObj.Played_EP3_LobbyIntro = true;
        }

        if (dataObj.isFirstEnterAtEP3_1)
        {
            dataObj.Played_EP3_Stage3_1Intro = true;
        }

        return dataObj;
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
    public static List<NPCInfo> LoadDefaultNPCInfo()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/npc_Info_default");
        if (jsonFile == null)
        {
            Debug.LogError("[SaveManager] npc_Info_default.json 파일을 찾을 수 없습니다. 경로를 확인하세요.");
            return new List<NPCInfo>();
        }
        NPCInfo[] npcArray = JsonHelper.FromJson<NPCInfo>(jsonFile.text);
        // talkCount, isUsed 초기화
        foreach (var npc in npcArray)
        {
            npc.talkCount = 0;
            if (npc.words != null)
            {
                foreach (var w in npc.words)
                    w.isUsed = false;
            }
        }
        return new List<NPCInfo>(npcArray);
    }

    private static NPCInfo CloneNpcInfo(NPCInfo source)
    {
        if (source == null)
        {
            return null;
        }

        return new NPCInfo
        {
            npcId = source.npcId,
            Affinity = source.Affinity,
            talkCount = source.talkCount,
            words = CloneMemoryKeywords(source.words)
        };
    }

    private static List<MemoryKeyword> CloneMemoryKeywords(List<MemoryKeyword> source)
    {
        List<MemoryKeyword> clone = new List<MemoryKeyword>();
        if (source == null)
        {
            return clone;
        }

        foreach (MemoryKeyword keyword in source)
        {
            if (keyword == null)
            {
                continue;
            }

            clone.Add(new MemoryKeyword
            {
                word = keyword.word,
                memoryRate = keyword.memoryRate,
                isUsed = keyword.isUsed
            });
        }

        return clone;
    }
    public static void WriteCurJSON(SaveDataObj sourceData)
    {
        if (sourceData == null)
        {
            Debug.LogWarning("[SaveManager] 저장할 현재 데이터가 없습니다.");
            return;
        }
        sourceData.savedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string json = JsonUtility.ToJson(sourceData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "CurData.json"), json);
    }
    public void WriteCurJSON() =>  WriteCurJSON(curData);  //현재 데이터 파일을 갱신
    public int TotalScore()
    {
        return curData.memory_reconstruction_rate.Sum();
    }
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }
        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
