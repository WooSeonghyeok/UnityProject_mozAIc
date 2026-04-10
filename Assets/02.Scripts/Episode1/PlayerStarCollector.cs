using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerStarCollector : MonoBehaviour
{
    [Header("획득한 별 목록")]
    public List<StarData> collectedStars = new List<StarData>();
    public event Action<int> OnStarCountChanged;  // 별 개수가 바뀔 때 호출할 이벤트
    public int memoryReconstructionRate = 0;  // 기억 재구성률, 별 획득 시 증가
    public void AddStar(StarData starData)  // 별 획득 처리
    {
        collectedStars.Add(starData);  // 별 추가
        if (collectedStars.Count == 1)  // 첫 번째 별을 먹은 순간 EP1 이벤트 실행
        {
            if (GameManager_Ep1.Instance != null)
            {
                GameManager_Ep1.Instance.OnFirstStarCollected();
            }
        }
        OnStarCountChanged?.Invoke(collectedStars.Count);  // 현재 별 개수를 외부에 알림
        memoryReconstructionRate += 1;  // 별 획득 시 기억 재구성률 1 증가
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
