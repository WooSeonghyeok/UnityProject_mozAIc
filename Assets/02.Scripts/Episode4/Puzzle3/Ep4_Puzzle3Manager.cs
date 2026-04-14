using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class Ep4_Puzzle3Manager : MonoBehaviour
{
    [Header("조각/수집")]
    [SerializeField] private int collectedPieceCount = 0;
    [SerializeField] private int requiredPieceCount = 7;
    [Header("출력 UI")]
    public GameObject pieceBox;
    public Text pieceCnt;
    public TextboxCtrl_Ep4 cutscene;
    private bool isMidCutsceneOn;
    [Header("옵션 이벤트")]
    [Tooltip("모든 조각을 수집했을 때 호출되는 이벤트. PuzzleComplete가 구독합니다.")]
    public UnityEvent onAllCollected;
    private bool _allPiecesEventRaised = false;  // 이벤트가 중복 호출되지 않도록 보호
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            pieceCnt.text = $"{collectedPieceCount} / {requiredPieceCount}";
            pieceBox.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pieceBox.SetActive(false);
        }
    }
    public void AddPiece()
    {
        collectedPieceCount++;
        Debug.Log($"[Ep4_3Manager] 악보 조각 수집: {collectedPieceCount}/{requiredPieceCount}");
        pieceCnt.text = $"{collectedPieceCount} / {requiredPieceCount}";
        if (collectedPieceCount == 1) cutscene.StartCoroutine(cutscene._manager.TalkSay(TextboxManager.TalkType.player, "분명 무언가의 일부인 소리."));  // 첫 조각 획득 시 대사 출력
        if ((float)(collectedPieceCount) / (float)(requiredPieceCount) >= 0.5f && !isMidCutsceneOn)
        {
            cutscene.StartCoroutine(cutscene._manager.TalkSay(TextboxManager.TalkType.player, "이건 멈춘 노래가 아니다. 끝을 기다리고 있던 노래다."));
            isMidCutsceneOn = true;
        }
        if (collectedPieceCount >= requiredPieceCount && !_allPiecesEventRaised)
        {
            _allPiecesEventRaised = true;
            if (SaveManager.instance != null) SaveManager.instance.curData.ep4_puzzle3Clear = true;
            try
            {
                onAllCollected?.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Ep4_3Manager] onAllPiecesCollected 호출 중 예외: {ex.Message}");
            }
        }
    }
}