using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPortal_Ep1 : MonoBehaviour
{
    [Header("씬 전환")]
    [SerializeField] private string nextSceneName;
    [Header("포탈 상태")]
    [SerializeField] private bool isActivated = false;
    [Header("참조")]
    [SerializeField] private Collider portalTrigger;
    [SerializeField] private ParticleSystem[] portalParticles;
    private void Awake()
    {
        // 시작 시 포탈은 비활성화
        SetPortalActive(false);
    }
    public void SetPortalActive(bool active)
    {
        isActivated = active;
        if (portalTrigger != null)  // 트리거 콜라이더 활성/비활성
        {
            portalTrigger.enabled = active;
        }
        if (portalParticles != null)  // 파티클 재생/정지
        {
            for (int i = 0; i < portalParticles.Length; i++)
            {
                if (portalParticles[i] == null) continue;
                if (active)
                {
                    portalParticles[i].gameObject.SetActive(true);
                    portalParticles[i].Play();
                }
                else
                {
                    portalParticles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    portalParticles[i].gameObject.SetActive(false);
                }
            }
        }
        // 포탈 오브젝트 전체를 꺼버리면 스크립트도 같이 꺼질 수 있으므로
        // 필요 시 비주얼 자식만 끄는 식으로 운영하는 게 안전함
        Debug.Log($"[EpisodePortal] 포탈 활성화 상태: {active}");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated) return;  // 활성화되지 않은 포탈은 무시
        if (!other.CompareTag("Player")) return;  // 플레이어만 포탈 진입 가능
        SaveManager.instance.curData.ep2_open = true;  //포탈 이용 시 다음 스테이지 오픈 처리를 먼저 실행
        // 다음 씬으로 전환
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[EpisodePortal] nextSceneName이 비어 있습니다.");
        }
    }
}