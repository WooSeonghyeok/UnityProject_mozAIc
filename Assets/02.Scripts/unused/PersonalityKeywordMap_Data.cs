using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class PersonalityKeywordMapFile  //전체 JSON 파일 데이터
{
    public List<PersonalityKeywordMap_Data> PersonalityKeywordMap;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class PersonalityKeywordMap_Data : Enums
{
    public string keywordId;
    public string keywordName;
    public string keywordGroup;
    public string targetEpisode;
    public int hintFreqeuncyDelta;
    public int hintDirectnessDelta;
    public int initiativeDelta;
    public int comfortDelta;
    public int emotionalExpressionDelta;
    public string memoryStyle;
    public string speechNote;
}