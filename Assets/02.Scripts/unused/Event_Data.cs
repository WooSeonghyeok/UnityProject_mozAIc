using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class EventFile  //전체 JSON 파일 데이터
{
    public List<Event_Data> Event;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class Event_Data : Enums
{
    public string eventGroupId;
    public int order;
    public string triggerTiming;
    public string eventTag;
    public string eventKey;
    public string stringValue;
    public int intValue;
    public bool boolValue;
    public bool waitForComplete;
}