using System;
using System.Collections.Generic;
[Serializable]
public class SaveDataObj
{
    /* 슬롯 데이터 */
    public byte ID;
    public string savedTime;

    /* 게임 진행 데이터 */
    public bool ep1_open;
    public bool ep1_isCaveUnlocked;
    public bool ep1_isPuzzleCleared;
    public bool ep2_open;
    public bool ep2_spaceClear;
    public bool ep2_paintClear;
    public bool ep3_open;
    public bool ep3_jumpClear;
    public bool ep3_paperClear;
    public bool ep4_open;
    public bool ep4_puzzle1Clear;
    public bool ep4_puzzle2Clear;
    public bool ep4_puzzle3Clear;
    public int memory_reconstruction_rate;
    public List<IsTagGet> CoreTag;    // 진 엔딩 태그
    public List<NPCInfo> npcInformations;

    /* 연출 사용 여부 확인 데이터 */
    public bool isFirstEnterAtS3CP0;
    public bool isFirstEnterAtEP3Lobby;
}
[Serializable]
public class IsTagGet
{
    public string TagName;
    public bool tagGet;
}
[Serializable]
public class NPCInfo
{
    public string npcId;
    public int Affinity;
    public List<MemoryKeyword> words;
}
[Serializable]
public class MemoryKeyword
{
    public string word;
    public int memoryRate;
    public bool isUsed = false;
}
