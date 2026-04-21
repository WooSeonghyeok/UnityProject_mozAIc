using AI_Enum;
using System.Collections.Generic;
[System.Serializable]
public class EndingStatePackageFile  //전체 JSON 파일 데이터
{
    public List<EndingStatePackage_Data> EndingStatePackage;  //각 줄마다 별개의 데이터가 할당되도록
}
[System.Serializable]
public class EndingStatePackage_Data : Enums
{
    public string packageId;
    public bool isTrueEnding;
    public int memoryReconstructionRate;
    public string[] missedMemoryTags;
    public string npcBondLevel;
    public string puzzleStability;
    public string emotionalRecoveryState;
    public MergeState finalMergeState;
    public string aiTonePreset;
    public string cutscenePreset;
}