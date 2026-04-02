using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HintRuleEntry
{
    public string key;
    public string value;
}

[Serializable]
public class AiPromptRuleData
{
    public bool useNpcProfile;
    public bool useSceneContext;
    public bool usePersonalityBuild;
    public bool useAffinity;
    public int maxSentenceCount;
    public int minSentenceCount;
    public bool forceMaintainTone;
    public List<string> forbiddenStyles;
    public List<string> globalRules;

    // JsonUtility는 Dictionary를 바로 파싱하기 불편해서 리스트로 받는 쪽이 안전함
    public List<HintRuleEntry> hintRules;
}
