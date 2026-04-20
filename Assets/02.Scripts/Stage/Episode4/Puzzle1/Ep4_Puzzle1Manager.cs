using System.Linq;
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
            piece.CollectMemory += MemoryCnt;
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
            StartCoroutine(cutscene._manager.TalkSay(TextboxManager.TalkType.voice, "없어진 게 아니야.\n흩어진 거지.", 1f, TextboxManager.Talker.core));
            isMidCutsceneOn = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pieceBox.SetActive(false);
        }
        bool MemoryComplete = memoryCollected >= totalCollected;  //조각 전부 수집 시 "split_self" 태그 획득
        var tag = SaveManager.instance.curData.CoreTag.FirstOrDefault(t => t.TagName == "split_self");
        if (tag != null) tag.tagGet = MemoryComplete;
        else
        {
            SaveManager.instance.curData.CoreTag.Add(new IsTagGet
            {
                TagName = "split_self",
                tagGet = MemoryComplete
            });
        }
    }
}
