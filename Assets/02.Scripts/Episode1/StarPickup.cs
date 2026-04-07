using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SoundManager;

public class StarPickup : MonoBehaviour
{
    [Header("별 데이터")]
    public StarData starData;   // 이 오브젝트가 어떤 별인지 연결

    [Header("획득 설정")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 아니면 무시
        if (!other.CompareTag(playerTag))
            return;

        // 플레이어의 수집 컴포넌트 가져오기
        PlayerStarCollector collector = other.GetComponent<PlayerStarCollector>();
        if (collector == null)
            return;

        // 별 데이터가 없으면 무시
        if (starData == null)
        {
            Debug.LogWarning($"{gameObject.name} 에 StarData가 연결되지 않았습니다.");
            return;
        }

        // 플레이어에게 별 지급
        collector.AddStar(starData);
        if(Instance != null)
            SoundManager.Instance.PlaySFX(SFXType.Ep1_Village_StarPickup);
        // 획득 후 오브젝트 제거
        Destroy(gameObject);
    }
}