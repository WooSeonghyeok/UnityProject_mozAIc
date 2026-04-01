using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class DialogueFile  //전체 JSON 파일 데이터
{
    public List<Dialogue_Data> Dialogue;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class Dialogue_Data : Enums
{
    public string dialogueId;
    public string sceneId;
    public string speakerId;
    public DialogueType dialogueType;
    public string text;
    public string[] emotionTag;
    public string conditionGroupId;
    public string eventGroupId;
    public string inputSlotId;
    public string nextDialogueId;
    public bool isSkippable;
    public bool autoPlay;
    public int priority;
}