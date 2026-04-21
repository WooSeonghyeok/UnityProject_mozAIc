using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class ConditionFile  //전체 JSON 파일 데이터
{
    public List<Condition_Data> Condition;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class Condition_Data : Enums
{
    public string conditionGroupId;
    public int order;
    public string[] targetKey;
    public ConditionOperator c_operator;
    public string value;
    public Logic logicToNext;
    public string failBehavior;
    public string fallbackDialogueId;
}