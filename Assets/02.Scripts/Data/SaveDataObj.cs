using System;
using System.Collections.Generic;
[System.Serializable]
public class SaveDataObj
{
    public byte ID;
    public string savedTime;
    public List<IsStageLock> StageLock;
    public int MemoryPoint;
    public List<IsTagGet> MemoryTag;
    public bool isFirstEnterAtS3CP0;
}
[Serializable]
public class IsStageLock
{
    public int stageID;
    public bool stageLock;
    public List<IsCPLock> CheckpointLock;
}
[Serializable]
public class IsCPLock
{
    public int CheckpointID;
    public bool cpLock;
}
[Serializable]
public class IsTagGet
{
    public string TagName;
    public bool tagGet;
}