using System.Collections.Generic;
[System.Serializable]
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
    public int memoryPoint;
    public List<IsTagGet> MemoryTag;
    public bool isFirstEnterAtS3CP0;
}
public class IsTagGet
{
    public string TagName;
    public bool tagGet;
}