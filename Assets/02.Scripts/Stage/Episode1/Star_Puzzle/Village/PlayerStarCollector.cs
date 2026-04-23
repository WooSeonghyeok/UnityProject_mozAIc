using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerStarCollector : MonoBehaviour
{
    [Header("획득한 별 목록")]
    public List<StarData> collectedStars = new List<StarData>();
    public event Action<int> OnStarCountChanged;  // 별 개수가 바뀔 때 호출할 이벤트

    [Header("별 5개 달성 오브젝트")]
    public GameObject targetObject;  // 활성화할 오브젝트
    public int requiredStarCount = 5;
    public void AddStar(StarData starData)  // 별 획득 처리
    {
        int oldPoint = Math.Clamp(SaveManager.instance.curData.memory_reconstruction_rate[3],0, 5);  //별 추가 전 점수 (최소 0점 ~ 최대 5점까지)
        collectedStars.Add(starData);  // 별 추가
        int newPoint = collectedStars.Count;  //별 추가 후 개수 출력
        SaveManager.instance.curData.memory_reconstruction_rate[3] = Math.Max(newPoint, oldPoint);  //획득한 별 개수만큼 에피소드 1 감정 점수 누적
        Debug.Log($"Episode1 감정 점수: {oldPoint} → {newPoint}");
        if (collectedStars.Count == 1)  // 첫 번째 별을 먹은 순간 EP1 이벤트 실행
        {
            //if (GameManager_Ep1.Instance != null)
            //{
            //    GameManager_Ep1.Instance.OnFirstStarCollected();
            //}
            if (EP1CutsceneTriggerManager.Instance != null)
            {
                EP1CutsceneTriggerManager.Instance.OnStarCollected(); // ⭐ ID 제거
            }

        }

        if (collectedStars.Count == requiredStarCount)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("targetObject가 할당되지 않았습니다!");
            }
        }

        OnStarCountChanged?.Invoke(collectedStars.Count);  // 현재 별 개수를 외부에 알림
    }
    public bool HasStar(string starId)  // 특정 ID의 별을 이미 가지고 있는지 확인
    {
        for (int i = 0; i < collectedStars.Count; i++)
        {
            if (collectedStars[i].starId == starId) return true;
        }
        return false;
    }
    public int GetStarCount()  // 현재 획득한 별 개수 반환
    {
        return collectedStars.Count;
    }
    public List<StarData> GetCollectedStars()  // 획득한 별 목록 복사본 반환
    {
        return new List<StarData>(collectedStars);  // 외부에서 원본 리스트를 직접 수정하지 못하게 복사본 반환
    }

}
