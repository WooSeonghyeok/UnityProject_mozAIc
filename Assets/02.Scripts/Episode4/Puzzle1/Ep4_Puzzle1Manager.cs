using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Ep4_Puzzle1Manager : MonoBehaviour
{
    public GameObject Pillars;
    public Ep4_Puzzle1_MemoryPiece[] memoryPieces;
    public int memoryCollected = 0;
    [SerializeField] private int totalCollected;
    public GameObject pieceBox;
    public Text pieceCnt;
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pieceCnt.text = $"{memoryCollected} / {totalCollected}";
            pieceBox.SetActive(true);
        }
    }
    void MemoryCnt()
    {
        if (SaveManager.instance == null) return;
        memoryCollected++;
        pieceCnt.text = $"{memoryCollected} / {totalCollected}";
        if (memoryCollected < totalCollected)
        {
            SaveManager.instance.curData.memory_reconstruction_rate += 5;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pieceBox.SetActive(false);
        }
    }
}
