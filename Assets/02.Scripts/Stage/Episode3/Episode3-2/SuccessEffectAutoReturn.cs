using System.Collections;
using UnityEngine;

/// <summary>
/// 성공 이펙트가 재생을 마친 뒤 자동으로 RhythmEffectManager에 반환되도록 돕는 보조 스크립트.
/// 
/// 역할:
/// 1. 하위 ParticleSystem들을 캐싱한다.
/// 2. 재생 시작 시 파티클을 모두 다시 재생한다.
/// 3. 모든 파티클이 끝날 때까지 기다렸다가 ownerManager로 반환 요청을 보낸다.
/// 
/// 즉, "성공 이펙트 전용 자동 반환 트리거" 역할이다.
/// </summary>
public class SuccessEffectAutoReturn : MonoBehaviour
{
    /// <summary>
    /// 이 이펙트를 소유하고 반환받을 RhythmEffectManager.
    /// 재생 완료 후 ReturnSuccessEffect 호출에 사용된다.
    /// </summary>
    private RhythmEffectManager ownerManager;

    /// <summary>
    /// 하위에서 찾아둔 ParticleSystem 캐시.
    /// 매번 GetComponentsInChildren를 반복 호출하지 않기 위해 저장한다.
    /// </summary>
    private ParticleSystem[] cachedParticles;

    /// <summary>
    /// 현재 재생 종료 대기 코루틴 참조.
    /// 중복 코루틴 실행을 막고 안전하게 중단하기 위해 보관한다.
    /// </summary>
    private Coroutine waitForReturnCoroutine;

    /// <summary>
    /// ownerManager를 연결하고 파티클 캐시를 준비한다.
    /// 풀에서 오브젝트를 다시 꺼낼 때도 재호출될 수 있다.
    /// </summary>
    public void Initialize(RhythmEffectManager manager)
    {
        ownerManager = manager;
        CacheParticles();
    }

    /// <summary>
    /// 성공 이펙트 재생 시작.
    /// 
    /// 처리 순서:
    /// 1. 파티클 캐시 확인
    /// 2. 기존 대기 코루틴 정리
    /// 3. 모든 파티클 Clear + Play
    /// 4. 재생 종료 감시 코루틴 시작
    /// </summary>
    public void Play()
    {
        CacheParticles();
        StopWaitingCoroutine();

        for (int i = 0; i < cachedParticles.Length; i++)
        {
            ParticleSystem particle = cachedParticles[i];
            if (particle == null)
            {
                continue;
            }

            particle.gameObject.SetActive(true);
            particle.Clear(true);
            particle.Play(true);
        }

        waitForReturnCoroutine = StartCoroutine(CoWaitForParticlesFinished());
    }

    /// <summary>
    /// 현재 이펙트를 즉시 중단하고 파티클 상태를 정리한다.
    /// 
    /// 풀 반환 직전이나 매니저 비활성화 시 흔적이 남지 않게 하기 위한 안전 정리 루틴이다.
    /// </summary>
    public void StopAndClear()
    {
        StopWaitingCoroutine();
        CacheParticles();

        for (int i = 0; i < cachedParticles.Length; i++)
        {
            ParticleSystem particle = cachedParticles[i];
            if (particle == null)
            {
                continue;
            }

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    /// <summary>
    /// 오브젝트가 비활성화될 때 대기 코루틴을 중단한다.
    /// 
    /// 비활성화 이후에도 코루틴이 남아 있으면 불필요한 반환 호출이나 상태 꼬임이 생길 수 있다.
    /// </summary>
    private void OnDisable()
    {
        StopWaitingCoroutine();
    }

    /// <summary>
    /// 모든 파티클이 완전히 끝날 때까지 기다린 뒤 ownerManager에 반환 요청을 보낸다.
    /// 
    /// 첫 프레임은 한 번 넘겨서 Play 직후 상태가 안정적으로 반영되도록 한다.
    /// </summary>
    private IEnumerator CoWaitForParticlesFinished()
    {
        yield return null;

        while (IsAnyParticleAlive())
        {
            yield return null;
        }

        if (ownerManager != null)
        {
            ownerManager.ReturnSuccessEffect(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 하위 파티클 중 하나라도 아직 살아 있는지 확인한다.
    /// 
    /// 하나라도 살아 있으면 이펙트는 아직 반환하면 안 된다.
    /// </summary>
    private bool IsAnyParticleAlive()
    {
        CacheParticles();

        for (int i = 0; i < cachedParticles.Length; i++)
        {
            ParticleSystem particle = cachedParticles[i];
            if (particle == null)
            {
                continue;
            }

            if (particle.IsAlive(true))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 하위 ParticleSystem들을 캐싱한다.
    /// 이미 캐시가 있으면 다시 찾지 않는다.
    /// </summary>
    private void CacheParticles()
    {
        if (cachedParticles != null && cachedParticles.Length > 0)
        {
            return;
        }

        cachedParticles = GetComponentsInChildren<ParticleSystem>(true);
    }

    /// <summary>
    /// 현재 실행 중인 반환 대기 코루틴이 있으면 중단하고 참조를 비운다.
    /// </summary>
    private void StopWaitingCoroutine()
    {
        if (waitForReturnCoroutine == null)
        {
            return;
        }

        StopCoroutine(waitForReturnCoroutine);
        waitForReturnCoroutine = null;
    }
}