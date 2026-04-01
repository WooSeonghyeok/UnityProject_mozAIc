using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class AIFallbackFile  //전체 JSON 파일 데이터
{
    public List<AIFallback_Data> AIFallback;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class AIFallback_Data : Enums
{
    public string fallbackId;
    public FallbackType fallbackType;
    public string npcId;
    public string sceneId;
    public string fallbackText;
    public string nextDialogueId;
    public string eventGroupId;
}