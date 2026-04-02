using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveRockUnlock : MonoBehaviour
{
    [Header("별 조건")]
    public int requiredStarCount = 5; // 동굴을 열기 위해 필요한 별 개수

    [Header("연결할 플레이어")]
    public PlayerStarCollector playerCollector; // 플레이어의 별 수집 스크립트

    [Header("돌 오브젝트")]
    public GameObject caveRockClosed; // 현재 입구를 막고 있는 돌
    public GameObject caveRockOpened; // 열렸을 때 보여줄 돌(또는 열린 상태 오브젝트)
    [Header("사운드")]
    public AudioSource source;   
    public AudioClip openClip;       // 동굴 열릴 때 재생할 사운드

    private bool isUnlocked = false; // 이미 열렸는지 체크

    private void Start()
    {
        // 플레이어 수집 스크립트가 없으면 종료
        if (playerCollector == null)
        {
            Debug.LogWarning("PlayerStarCollector가 연결되지 않았습니다.");
            return;
        }

        // 별 개수 변화 이벤트 구독
        playerCollector.OnStarCountChanged += CheckUnlockCondition;

        // 시작 시에도 현재 별 개수 기준으로 바로 검사
        CheckUnlockCondition(playerCollector.GetStarCount());
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 구독 해제
        if (playerCollector != null)
        {
            playerCollector.OnStarCountChanged -= CheckUnlockCondition;
        }
    }

    void CheckUnlockCondition(int currentStarCount)
    {
        // 이미 열렸으면 다시 처리하지 않음
        if (isUnlocked) return;

        // 필요한 별 개수 이상인지 확인
        if (currentStarCount >= requiredStarCount)
        {
            UnlockCave();
        }
    }

    void UnlockCave()
    {
        isUnlocked = true;

        // 닫힌 돌 비활성화
        if (caveRockClosed != null)
        {
            caveRockClosed.SetActive(false);
        }

        // 열린 돌 활성화
        if (caveRockOpened != null)
        {
            caveRockOpened.SetActive(true);
        }

        // 사운드 재생
        if (source != null && openClip != null)
        {
            StartCoroutine(PlaySoundDelay());
        }

        Debug.Log("별을 모두 모아 동굴 입구가 열렸습니다.");

        // 게임매니저에 알려서 NPC 기억 업데이트
        if (GameManager_Ep1.Instance != null)
        {
            GameManager_Ep1.Instance.OnCaveUnlocked();
        }
    }

    IEnumerator PlaySoundDelay()
    {
        yield return new WaitForSeconds(0.5f);
        source.PlayOneShot(openClip);
    }
}
