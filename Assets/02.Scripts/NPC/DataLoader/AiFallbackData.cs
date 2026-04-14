using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AiFallbackData
{
    public string fallbackId;
    public string triggerType;
    public string speakerScope;
    public string text;
}

[Serializable]
public class AiFallbackDataList
{
    public List<AiFallbackData> items;
}
