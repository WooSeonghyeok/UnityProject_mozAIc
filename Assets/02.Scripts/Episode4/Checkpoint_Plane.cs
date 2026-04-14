using System;
using System.Collections.Generic;
using UnityEngine;
public class Checkpoint_Plane : MonoBehaviour
{
    public int cpNum;
    public Transform spawnPos;
    public bool isCheck = false;
    private readonly string playerTag = "Player";
    public event Action S3FirstCheck;
    public static Dictionary<int, Dictionary<int, bool>> cpProgress = new Dictionary<int, Dictionary<int, bool>>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isCheck = true;
            SaveCheckpointProgress();
            if(cpNum == 0) S3FirstCheck?.Invoke();
        }
    }
    private void SaveCheckpointProgress()
    {
        if (SaveManager.instance == null) return;
        if (!cpProgress.ContainsKey(3))
        {
            cpProgress[3] = new Dictionary<int, bool>();
        }
        switch (cpNum)
        {
            case 0: cpProgress[3][0] = SaveManager.instance.curData.ep4_open; break;
            case 1: cpProgress[3][1] = SaveManager.instance.curData.ep4_puzzle1Clear; break;
            case 2: cpProgress[3][2] = SaveManager.instance.curData.ep4_puzzle2Clear; break;
            case 3: cpProgress[3][3] = SaveManager.instance.curData.ep4_puzzle3Clear; break;
        }
    }
    public static bool IsCheckpointCleared(int cpNum)
    {
        if (cpProgress.ContainsKey(3) && cpProgress[3].ContainsKey(cpNum))
        {
            return cpProgress[3][cpNum];
        }
        return false;
    }
}