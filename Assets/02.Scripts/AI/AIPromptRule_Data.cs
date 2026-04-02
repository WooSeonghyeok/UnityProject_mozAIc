using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class AIPromptRuleFile  //전체 JSON 파일 데이터
{
    public List<AIPromptRule_Data> AIPromptRule;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class AIPromptRule_Data : Enums
{
    public string aiRuleId;
    public bool useSceneContext;
    public bool useNpcProfile;
    public bool usePersonalityBuild;
    public bool useEndingStatePackage;
    public string requiredEmotion;
    public int maxSentenceCount;
    public string[] bannedOutputTone;
    public string outputFormat;
    public string fallbackId;
}