using System.Collections.Generic;
[System.Serializable]
public class EndingRuleFile  //전체 JSON 파일 데이터
{
    public List<EndingRule_Data> EndingRule;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class EndingRule_Data  //개별 데이터 줄
{
    public string ruleId;
    public bool isTrueEnding;
    public int minReconstructionRate;
    public string[] requiredTags;
    public string[] requiredFlags;
    public string failDowngradeTo;
    public string outputEndingStatId;
    public string notes;
}