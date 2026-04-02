using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NpcProfileData
{
    public string npcId;
    public string displayName;
    public string episodeId;
    public string roleType;
    public string baseRelationship;
    public string memoryTheme;
    public string recoveredElement;
    public string coreDescription;
    public string defaultTone;
    public List<string> speechStyle;
    public List<string> allowedTopics;
    public List<string> bannedTopics;
    public string systemPromptCore;
}
