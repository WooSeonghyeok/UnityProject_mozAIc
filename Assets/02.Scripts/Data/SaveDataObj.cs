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
    public int[] memory_reconstruction_rate = new int[13];
    /* 0 : 기본 점수, 1~3 : Episode1, 4~6 : Episode2, 7~9 : Episode3, 10~12 : Episode4
                      1/4/7/10 : 관계 점수, 2/5/8/11 : 퍼즐 점수, 3/6/9/12 : 감정 점수*/
    public List<IsTagGet> CoreTag;    // 진 엔딩 태그
    public List<NPCInfo> npcInformations;

    /* 연출 사용 여부 확인 데이터 */
    public bool isFirstEnterAtS3CP0;
    public bool isFirstEnterAtEP3Lobby;
    public bool isFirstEnterAtEP3_1;
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
