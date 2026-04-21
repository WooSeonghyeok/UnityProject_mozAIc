using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class HintTimingRuleFile  //전체 JSON 파일 데이터
{
    public List<HintTimingRule_Data> HintTimingRule;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class HintTimingRule_Data : Enums
{
    public string ruleId;
    public string npcId;
    public int stuckTimeSec;
    public int failCountThreshold;
    public int directnessBonus;
    public int initiativeBouns;
    public bool minimumFunctionalHint;
}