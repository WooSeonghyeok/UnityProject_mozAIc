using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class HintStyleRuleFile  //전체 JSON 파일 데이터
{
    public List<HintStyleRule_Data> HintStyleRule;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class HintStyleRule_Data : Enums
{
    public string styleId;
    public int directnessLevel;
    public string styleDescription;
    public string examplePattern;
    public bool useForAi;
}