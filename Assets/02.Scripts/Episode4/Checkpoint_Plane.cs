using System;
using System.Collections.Generic;
using UnityEngine;
public class Checkpoint_Plane : MonoBehaviour
{
    public int stageNum;  //0-base이므로 스테이지 값에서 1을 빼서 입력
    public int cpNum;
    public Transform spawnPos;
    public bool isCheck = false;
    private readonly string playerTag = "Player";
    public event Action S3CP0FirstCheck;
    public static Dictionary<int, Dictionary<int, bool>> cpProgress = new Dictionary<int, Dictionary<int, bool>>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isCheck = true;
            SaveCheckpointProgress();
            if(stageNum == 3 && cpNum == 0)
            {
                S3CP0FirstCheck?.Invoke();
            }
        }
    }
    private void SaveCheckpointProgress()
    {
        if (!cpProgress.ContainsKey(stageNum))
        {
            cpProgress[stageNum] = new Dictionary<int, bool>();
        }
        cpProgress[stageNum][cpNum] = true;
        SaveManager.instance.curData.StageLock[stageNum].CheckpointLock[cpNum].cpLock = !cpProgress[stageNum][cpNum];
    }
    public static bool IsCheckpointCleared(int stageNum, int cpNum)
    {
        if (cpProgress.ContainsKey(stageNum) && cpProgress[stageNum].ContainsKey(cpNum))
        {
            return cpProgress[stageNum][cpNum];
        }
        return false;
    }
}