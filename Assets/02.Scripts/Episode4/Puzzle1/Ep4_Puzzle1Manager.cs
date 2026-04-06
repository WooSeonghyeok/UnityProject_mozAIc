using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Ep4_Puzzle1Manager : MonoBehaviour
{
    public GameObject Pillars;
    public Ep4_Puzzle1_MemoryPiece[] memoryPieces;
    public int memoryCollected = 0;
    [SerializeField] private int totalCollected;
    private void Awake()
    {
        memoryPieces = GetComponentsInChildren<Ep4_Puzzle1_MemoryPiece>();
    }
    void OnEnable()
    {
        totalCollected = memoryPieces.Length;
        foreach (var piece in memoryPieces)
        {
            piece.collectMemory += MemoryCnt;
        }
    }
    void MemoryCnt()
    {
        memoryCollected++;
        if (memoryCollected >= totalCollected)
        {
            foreach (IsTagGet lastTag in SaveManager.instance.curData.MemoryTag)
            {
                if (lastTag.TagName == "split_self") lastTag.tagGet = true;
            }
        }
    }
}
