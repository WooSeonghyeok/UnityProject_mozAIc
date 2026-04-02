using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerStarCollector : MonoBehaviour
{
    [Header("획득한 별 목록")]
    public List<StarData> collectedStars = new List<StarData>();

    // 별 개수가 바뀔 때 호출할 이벤트
    public event Action<int> OnStarCountChanged;

    /// 별 획득 처리
    public void AddStar(StarData starData)
    {
        
        // 별 추가
        collectedStars.Add(starData);

        // 첫 번째 별을 먹은 순간 EP1 이벤트 실행
        if (collectedStars.Count == 1)
        {
            if (GameManager_Ep1.Instance != null)
            {
                GameManager_Ep1.Instance.OnFirstStarCollected();
            }
        }

        // 현재 별 개수를 외부에 알림
        OnStarCountChanged?.Invoke(collectedStars.Count);
    }

    /// 특정 ID의 별을 이미 가지고 있는지 확인
    public bool HasStar(string starId)
    {
        for (int i = 0; i < collectedStars.Count; i++)
        {
            if (collectedStars[i].starId == starId)
                return true;
        }

        return false;
    }

    /// 현재 획득한 별 개수 반환
    public int GetStarCount()
    {
        return collectedStars.Count;
    }

    /// 획득한 별 목록 복사본 반환
    public List<StarData> GetCollectedStars()
    {
        // 외부에서 원본 리스트를 직접 수정하지 못하게 복사본 반환
        return new List<StarData>(collectedStars);
    }
}
