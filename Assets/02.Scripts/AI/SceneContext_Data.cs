using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class SceneContextFile  //전체 JSON 파일 데이터
{
    public List<SceneContext_Data> SceneContext;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class SceneContext_Data : Enums
{
    public string sceneId;
    public string sceneName;
    public string[] moodTags;
    public string narrativePurpose;
    public string[] allowedTopics;
    public string[] bannedTopics;
    public int maxAiSentenceCount;
    public int maxAiChars;
    public bool isMajorCutscene;
}