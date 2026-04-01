using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneVisualStateData
{
    public string blur;
    public string saturation;
    public string bgm;
    public string environmentSound;
}

[Serializable]
public class SceneContextData
{
    public string sceneId;
    public string sceneName;
    public string episodeId;
    public string mood;
    public string goal;
    public string recoveredElement;
    public List<string> missingElements;
    public SceneVisualStateData visualState;
    public List<string> allowedTopics;
    public List<string> bannedTopics;
    public List<string> specialRules;
}
