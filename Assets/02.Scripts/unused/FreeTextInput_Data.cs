using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class FreeTextInputFile  //전체 JSON 파일 데이터
{
    public List<FreeTextInput_Data> FreeTextInput;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class FreeTextInput_Data : Enums
{
    public string inputSlotId;
    public string sceneId;
    public string sourceDialogueId;
    public string promptText;
    public string[] allowedIntents;
    public string[] bannedIntents;
    public int minLength;
    public int maxLength;
    public string aiInterpretRuleId;
    public string defaultNextDialogueId;
    public Dictionary<string, string>[] intentRoutingMap;
    public string scoreEffectNote;
    public string fallbackId;
}