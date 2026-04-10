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
    public TextboxCtrl_Ep4 cutscene;
    private bool isMidCutsceneOn = false;
    private void Awake()
    {
        pieceBox.SetActive(false);
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
        if (memoryCollected >= (totalCollected / 2) && !isMidCutsceneOn)  //조각 절반 이상 수집 시점에 중간 대사 출력
        {
            StartCoroutine(cutscene._manager.TalkSay(TextboxManager.TalkType.player, "다시 지나가야 한다. 그때의 나처럼."));
            isMidCutsceneOn = true;
        }
        if (memoryCollected >= totalCollected)  //조각 전부 수집 시 "split_self" 태그 획득
        {
            foreach (var tag in SaveManager.instance.curData.CoreTag)
            {
                if (tag.TagName == "split_self")
                {
                    tag.tagGet = true;
                    break;
                }
            }
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
