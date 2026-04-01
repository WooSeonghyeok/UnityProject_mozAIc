using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class MemoryTagFile  //전체 JSON 파일 데이터
{
    public List<MemoryTag_Data> MemoryTag;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class MemoryTag_Data : Enums
{
    public string tagId;
    public string category;
    public string displayName;
    public Importance importance;
    public string description;
    public bool requireForTrueEnding;
    public string relatedNpcId;
}