using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NpcProfileData
{
    public string npcId;
    public string name;
    public string episodeId;
    public string chatName;
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
