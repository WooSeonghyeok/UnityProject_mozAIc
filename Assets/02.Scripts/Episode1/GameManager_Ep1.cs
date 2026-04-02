using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_Ep1 : MonoBehaviour
{
    public static GameManager_Ep1 Instance;

    [Header("EP.1 진행 상태")]
    public bool isCaveUnlocked = false;      // 별 5개 달성으로 동굴이 열렸는지
    public bool isPuzzleCleared = false;     // 제단 퍼즐을 클리어했는지

    [Header("연결할 NPC")]
    public NPCData lunaNpcData;              // 루나 NPCData 연결

    [Header("기억 단계 설정")]
    public MemoryRevealStage startStage = MemoryRevealStage.FaintFeeling;  // 희미한 기억
    public MemoryRevealStage caveOpenStage = MemoryRevealStage.Partial;    // 부분적 기억
    public MemoryRevealStage puzzleClearStage = MemoryRevealStage.Strong;  // 강렬한 기억
    public MemoryRevealStage fullMemoryStage = MemoryRevealStage.Full;     // 모든 단계 달성 후 완전한 기억 단계

    [Header("첫 별 힌트")]
    public bool hasPlayedFirstStarHint = false;

    [Header("동굴 진입 상태")]
    public bool hasEnteredCave = false;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // 게임 시작 시 루나 기억 단계를 초기값으로 맞춤
        if (lunaNpcData != null)
        {
            lunaNpcData.SetRevealStage(startStage);
            Debug.Log($"[GameManager_Ep1] 시작 단계 설정: {startStage}");
        }
        else
        {
            Debug.LogWarning("[GameManager_Ep1] lunaNpcData가 연결되지 않았습니다.");
        }
    }
    public void OnFirstStarCollected()
    {
        // 이미 첫 별 힌트를 재생했으면 다시 실행하지 않음
        if (hasPlayedFirstStarHint)
            return;

        hasPlayedFirstStarHint = true;

        // 일반 채팅창 대신 말풍선 UI로 5초간 출력
        if (lunaNpcData != null && ChatNPCManager.instance != null)
        {
            ChatNPCManager.instance.PlayNpcBubbleDialogue(lunaNpcData, "first_star_hint");
        }
    }

    public void OnCaveUnlocked()
    {
        // 이미 처리했으면 중복 실행 방지
        if (isCaveUnlocked)
            return;

        isCaveUnlocked = true;

        // 동굴이 열리면 루나의 기억 단계를 한 단계 올림
        if (lunaNpcData != null)
        {
            lunaNpcData.SetRevealStage(caveOpenStage);
            Debug.Log($"[GameManager_Ep1] 동굴 개방 -> 루나 기억 단계 상승: {caveOpenStage}");
        }
    }

    public void OnPuzzleCleared()
    {
        // 이미 처리했으면 중복 실행 방지
        if (isPuzzleCleared)
            return;

        isPuzzleCleared = true;

        // 퍼즐이 클리어되면 루나 기억 단계를 더 올림
        if (lunaNpcData != null)
        {
            lunaNpcData.SetRevealStage(puzzleClearStage);
            Debug.Log($"[GameManager_Ep1] 퍼즐 클리어 -> 루나 기억 단계 상승: {puzzleClearStage}");
        }
    }

    public void OnEnterCave()
    {
        if (hasEnteredCave)
            return;

        hasEnteredCave = true;
        Debug.Log("[GameManager_Ep1] 동굴 진입 상태 활성화");
    }

    public void SetLunaRevealStage(MemoryRevealStage newStage)
    {
        // 컷신, 이벤트 등에서 수동으로 단계 조정할 때 사용
        if (lunaNpcData == null)
        {
            Debug.LogWarning("[GameManager_Ep1] lunaNpcData가 없어 단계 변경 실패");
            return;
        }

        lunaNpcData.SetRevealStage(newStage);
        Debug.Log($"[GameManager_Ep1] 수동 단계 변경: {newStage}");
    }
    //아직 진엔딩 여부를 설정하지 않아서 일단 강렬한 기억 단계까지만 구현, 진엔딩 달성 시 fullMemoryStage로 변경
    private IEnumerator RecoverFullMemory()
    {
        yield return new WaitForSeconds(2f); // 연출 시간

        if (lunaNpcData != null)
        {
            lunaNpcData.SetRevealStage(fullMemoryStage);
            Debug.Log("[GameManager_Ep1] 완전 기억 복원 완료");
        }
    }
}
