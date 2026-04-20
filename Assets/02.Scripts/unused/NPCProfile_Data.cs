using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class NPCProfileFile  //전체 JSON 파일 데이터
{
    public List<NPCProfile_Data> NPCProfile;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class NPCProfile_Data : Enums
{
    public string npcId;
    public string displayName;
    public RoleType roleType;
    public string baseRelationship;
    public string fixedCoreTraits;
    public string[] allowedTopics;
    public string[] bannedTopics;
    public string defaultTone;
    public string portraitSetId;
}