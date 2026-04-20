using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class NPCPersonalityCoreFile  //전체 JSON 파일 데이터
{
    public List<NPCPersonalityCore_Data> NPCPersonalityCore;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class NPCPersonalityCore_Data : Enums
{
    public string npcId;
    public string coreTrait;
    public string description;
    public int lockWeight;
}