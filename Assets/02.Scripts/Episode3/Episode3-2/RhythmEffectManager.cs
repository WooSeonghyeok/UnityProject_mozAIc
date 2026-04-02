using System.Collections.Generic;
using UnityEngine;

public class RhythmEffectManager : MonoBehaviour
{
    [System.Serializable]
    private class EffectPool
    {
        [Header("이펙트 프리팹")]
        public GameObject prefab;

        [Header("풀 크기")]
        public int poolSize = 5;

        [Header("회전 오프셋")]
        public Vector3 rotationEulerOffset = new Vector3(180f, 0f, 0f);

        public readonly Queue<GameObject> availableObjects = new Queue<GameObject>();
        public readonly List<GameObject> allObjects = new List<GameObject>();
        public readonly HashSet<GameObject> availableSet = new HashSet<GameObject>();

        public bool isInitialized = false;
    }

    // 하나의 발판에 붙는 정답 표시 이펙트 2개를 한 쌍으로 관리하기 위한 묶음 클래스
    private class TargetIndicatorPair
    {
        public GameObject indicator1;
        public GameObject indicator2;
    }

    [Header("정답 표시 이펙트 1")]
    [SerializeField] private EffectPool targetIndicatorPool1 = new EffectPool();

    [Header("정답 표시 이펙트 2")]
    [SerializeField] private EffectPool targetIndicatorPool2 = new EffectPool();

    [Header("정답 성공 이펙트 1")]
    [SerializeField] private EffectPool successEffectPool1 = new EffectPool();

    [Header("정답 성공 이펙트 2")]
    [SerializeField] private EffectPool successEffectPool2 = new EffectPool();

    // 현재 정답 표시가 붙어 있는 발판별 이펙트 쌍
    // key   : 정답 표시가 붙은 발판 Transform
    // value : 그 발판에 붙은 표시 이펙트 2개
    private readonly Dictionary<Transform, TargetIndicatorPair> activeTargetIndicators
        = new Dictionary<Transform, TargetIndicatorPair>();

    // 성공 이펙트 오브젝트가 어느 풀에서 나왔는지 추적하는 맵
    // 자동 반환 시 원래 풀로 정확히 되돌려보내기 위해 필요하다.
    private readonly Dictionary<GameObject, EffectPool> successEffectOwnerMap
        = new Dictionary<GameObject, EffectPool>();

    private void Awake()
    {
        InitializeEffectPool(targetIndicatorPool1, false);
        InitializeEffectPool(targetIndicatorPool2, false);
        InitializeEffectPool(successEffectPool1, true);
        InitializeEffectPool(successEffectPool2, true);
    }

    private void OnEnable()
    {
        ResetEffectPoolState(targetIndicatorPool1, false);
        ResetEffectPoolState(targetIndicatorPool2, false);
        ResetEffectPoolState(successEffectPool1, true);
        ResetEffectPoolState(successEffectPool2, true);

        activeTargetIndicators.Clear();
    }

    private void OnDisable()
    {
        StopAllEffectsWithoutReparent(targetIndicatorPool1, false);
        StopAllEffectsWithoutReparent(targetIndicatorPool2, false);
        StopAllEffectsWithoutReparent(successEffectPool1, true);
        StopAllEffectsWithoutReparent(successEffectPool2, true);

        activeTargetIndicators.Clear();
    }

    // 특정 발판 하나에 정답 표시 이펙트를 붙인다.
    //
    // 동작 규칙:
    // - targetPlatform이 null이면 아무 작업도 하지 않는다.
    // - 이미 해당 발판에 표시 이펙트가 붙어 있으면 중복 생성하지 않는다.
    // - 표시 이펙트 2개를 풀에서 꺼내 해당 발판 자식으로 부착한다.
    public void ShowTargetIndicatorForPlatform(Transform targetPlatform)
    {
        if (targetPlatform == null)
        {
            Debug.LogWarning("[RhythmEffectManager] ShowTargetIndicatorForPlatform 호출 시 targetPlatform이 null입니다.");
            return;
        }

        if (activeTargetIndicators.ContainsKey(targetPlatform))
        {
            return;
        }

        TargetIndicatorPair pair = new TargetIndicatorPair();
        pair.indicator1 = AttachTargetIndicatorFromPool(targetIndicatorPool1, targetPlatform);
        pair.indicator2 = AttachTargetIndicatorFromPool(targetIndicatorPool2, targetPlatform);

        if (pair.indicator1 == null && pair.indicator2 == null)
        {
            Debug.LogWarning("[RhythmEffectManager] 정답 표시 이펙트를 하나도 활성화하지 못했습니다.");
            return;
        }

        activeTargetIndicators[targetPlatform] = pair;
    }

    // 전달받은 발판 목록 전체에 정답 표시 이펙트를 붙인다.
    //
    // 사용 목적:
    // - 현재 화면에 살아 있는 정답 발판이 여러 개일 때
    //   각 발판마다 정답 표시용 파티클을 동시에 붙이기 위함이다.
    public void ShowTargetIndicatorsForPlatforms(IEnumerable<Transform> targetPlatforms)
    {
        if (targetPlatforms == null)
        {
            return;
        }

        foreach (Transform targetPlatform in targetPlatforms)
        {
            ShowTargetIndicatorForPlatform(targetPlatform);
        }
    }

    // 특정 발판에 붙어 있는 정답 표시 이펙트만 숨기고 풀로 반환한다.
    public void HideTargetIndicatorForPlatform(Transform targetPlatform)
    {
        if (targetPlatform == null)
        {
            return;
        }

        if (!activeTargetIndicators.TryGetValue(targetPlatform, out TargetIndicatorPair pair))
        {
            return;
        }

        if (pair != null)
        {
            ReturnTargetIndicator(pair.indicator1, targetIndicatorPool1);
            ReturnTargetIndicator(pair.indicator2, targetIndicatorPool2);
        }

        activeTargetIndicators.Remove(targetPlatform);
    }

    // 현재 붙어 있는 정답 표시 이펙트를 모두 숨기고 풀로 반환한다.
    //
    // 퍼즐 종료, 전체 초기화, 상태 리셋 시 사용한다.
    public void HideAllTargetIndicators()
    {
        if (activeTargetIndicators.Count == 0)
        {
            return;
        }

        List<Transform> keys = new List<Transform>(activeTargetIndicators.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            HideTargetIndicatorForPlatform(keys[i]);
        }

        activeTargetIndicators.Clear();
    }

    // 기존 코드와의 호환용 메서드
    // 이전에 ShowTargetIndicator(targetPlatform) 하나만 호출하던 구조를
    // 깨지 않게 유지하려고 내부에서 새 다중 타겟 메서드로 연결한다.
    public void ShowTargetIndicator(Transform targetPlatform)
    {
        ShowTargetIndicatorForPlatform(targetPlatform);
    }

    // 기존 코드와의 호환용 메서드
    // 이전에는 단일 타겟 표시를 숨겼지만,
    // 현재 구조에서는 전체 정답 표시 이펙트를 숨기는 동작으로 연결한다.
    public void HideTargetIndicator()
    {
        HideAllTargetIndicators();
    }

    // 정답 성공 시 성공 이펙트 2개를 같은 위치에서 동시에 재생한다.
    //
    // 성공 연출을 풍부하게 보이게 하기 위해 두 종류의 풀을 동시에 사용한다.
    public void PlaySuccessEffect(Vector3 worldPosition)
    {
        PlaySuccessEffectFromPool(successEffectPool1, worldPosition);
        PlaySuccessEffectFromPool(successEffectPool2, worldPosition);
    }

    // 성공 이펙트 자동 반환 스크립트가 재생 종료 후 호출하는 반환 메서드
    //
    // ownerMap에 등록된 원래 풀을 찾아 정확히 되돌려보낸다.
    // owner를 찾지 못하면 최소한 비활성화/부모 복귀까지만 수행한다.
    public void ReturnSuccessEffect(GameObject effectObject)
    {
        if (effectObject == null)
        {
            return;
        }

        if (!successEffectOwnerMap.TryGetValue(effectObject, out EffectPool ownerPool))
        {
            StopAllParticlesInHierarchy(effectObject);
            effectObject.SetActive(false);
            effectObject.transform.SetParent(transform, false);
            return;
        }

        // 이미 반환된 오브젝트를 다시 넣는 실수를 막는다.
        if (ownerPool.availableSet.Contains(effectObject))
        {
            return;
        }

        SuccessEffectAutoReturn autoReturn = effectObject.GetComponent<SuccessEffectAutoReturn>();
        if (autoReturn != null)
        {
            autoReturn.StopAndClear();
        }
        else
        {
            StopAllParticlesInHierarchy(effectObject);
        }

        effectObject.SetActive(false);
        effectObject.transform.SetParent(transform, false);
        ownerPool.availableObjects.Enqueue(effectObject);
        ownerPool.availableSet.Add(effectObject);
    }

    // 정답 표시 이펙트를 풀에서 꺼내 대상 발판의 자식으로 붙인다.
    //
    // 위치/회전/스케일을 발판 기준으로 리셋한 뒤 파티클을 재생한다.
    private GameObject AttachTargetIndicatorFromPool(EffectPool pool, Transform targetPlatform)
    {
        if (pool == null || pool.prefab == null)
        {
            return null;
        }

        InitializeEffectPool(pool, false);

        GameObject effectObject = GetEffectFromPool(pool, false);
        if (effectObject == null)
        {
            Debug.LogWarning($"[RhythmEffectManager] 정답 표시 이펙트를 가져오지 못했습니다. prefab={pool.prefab.name}");
            return null;
        }

        effectObject.transform.SetParent(targetPlatform, false);
        effectObject.transform.localPosition = Vector3.zero;
        effectObject.transform.localRotation = Quaternion.Euler(pool.rotationEulerOffset);
        effectObject.transform.localScale = Vector3.one;
        effectObject.SetActive(true);

        PlayAllParticlesInHierarchy(effectObject);
        return effectObject;
    }

    // 정답 표시 이펙트를 풀로 반환한다.
    //
    // 현재 구조에서는 발판별 이펙트를 Dictionary로 관리하므로
    // ref 대신 반환 대상 오브젝트를 직접 받아 처리한다.
    private void ReturnTargetIndicator(GameObject effectObject, EffectPool pool)
    {
        if (effectObject == null || pool == null)
        {
            return;
        }

        if (!pool.availableSet.Contains(effectObject))
        {
            StopAllParticlesInHierarchy(effectObject);
            effectObject.SetActive(false);
            effectObject.transform.SetParent(transform, false);
            pool.availableObjects.Enqueue(effectObject);
            pool.availableSet.Add(effectObject);
        }
    }

    // 성공 이펙트를 풀에서 꺼내 월드 좌표에서 재생한다.
    //
    // 성공 이펙트는 표시 이펙트와 달리 발판 자식으로 붙지 않고
    // 월드 공간 기준으로 한 번 재생된 뒤 자동 반환된다.
    private void PlaySuccessEffectFromPool(EffectPool pool, Vector3 worldPosition)
    {
        if (pool == null || pool.prefab == null)
        {
            return;
        }

        InitializeEffectPool(pool, true);

        GameObject effectObject = GetEffectFromPool(pool, true);
        if (effectObject == null)
        {
            Debug.LogWarning($"[RhythmEffectManager] 성공 이펙트를 가져오지 못했습니다. prefab={pool.prefab.name}");
            return;
        }

        effectObject.transform.SetParent(transform, false);
        effectObject.transform.position = worldPosition;
        effectObject.transform.rotation = pool.prefab.transform.rotation * Quaternion.Euler(pool.rotationEulerOffset);
        effectObject.SetActive(true);

        SuccessEffectAutoReturn autoReturn = effectObject.GetComponent<SuccessEffectAutoReturn>();
        if (autoReturn == null)
        {
            autoReturn = effectObject.AddComponent<SuccessEffectAutoReturn>();
        }

        autoReturn.Initialize(this);
        autoReturn.Play();
    }

    // 특정 풀을 초기화한다.
    //
    // 최초 한 번만 poolSize만큼 미리 생성하고,
    // 이후에는 재사용만 하도록 만든다.
    private void InitializeEffectPool(EffectPool pool, bool isSuccessEffect)
    {
        if (pool == null || pool.isInitialized)
        {
            return;
        }

        if (pool.prefab == null)
        {
            return;
        }

        int count = Mathf.Max(1, pool.poolSize);

        for (int i = 0; i < count; i++)
        {
            CreateEffectInstance(pool, isSuccessEffect);
        }

        pool.isInitialized = true;
        Debug.Log($"[RhythmEffectManager] 풀 초기화 완료 - prefab={pool.prefab.name}, poolSize={count}");
    }

    // 풀용 오브젝트를 하나 생성한다.
    //
    // 성공 이펙트라면 자동 반환 스크립트도 붙이고 ownerMap도 같이 등록한다.
    private GameObject CreateEffectInstance(EffectPool pool, bool isSuccessEffect)
    {
        GameObject effectObject = Instantiate(pool.prefab, transform);
        effectObject.name = $"{pool.prefab.name}_{pool.allObjects.Count + 1}";
        effectObject.SetActive(false);

        if (isSuccessEffect)
        {
            SuccessEffectAutoReturn autoReturn = effectObject.GetComponent<SuccessEffectAutoReturn>();
            if (autoReturn == null)
            {
                autoReturn = effectObject.AddComponent<SuccessEffectAutoReturn>();
            }

            autoReturn.Initialize(this);
            successEffectOwnerMap[effectObject] = pool;
        }

        pool.allObjects.Add(effectObject);
        pool.availableObjects.Enqueue(effectObject);
        pool.availableSet.Add(effectObject);

        return effectObject;
    }

    // 풀에서 사용할 오브젝트를 하나 꺼낸다.
    //
    // 비어 있으면 런타임에 추가 생성해서라도 반환하려고 시도한다.
    // 이렇게 하면 예상보다 이펙트 동시 재생이 많아도 완전히 끊기지는 않는다.
    private GameObject GetEffectFromPool(EffectPool pool, bool isSuccessEffect)
    {
        if (pool.availableObjects.Count == 0)
        {
            CreateEffectInstance(pool, isSuccessEffect);
        }

        if (pool.availableObjects.Count == 0)
        {
            return null;
        }

        GameObject effectObject = pool.availableObjects.Dequeue();
        pool.availableSet.Remove(effectObject);
        return effectObject;
    }

    // 매니저 재활성화 시 풀 상태를 다시 정돈한다.
    //
    // 모든 오브젝트를 비활성화하고 루트로 되돌린 뒤,
    // available 큐와 set을 다시 채워 "대기 상태"로 맞춘다.
    private void ResetEffectPoolState(EffectPool pool, bool isSuccessEffect)
    {
        if (pool == null || pool.allObjects.Count == 0)
        {
            return;
        }

        pool.availableObjects.Clear();
        pool.availableSet.Clear();

        for (int i = 0; i < pool.allObjects.Count; i++)
        {
            GameObject effectObject = pool.allObjects[i];
            if (effectObject == null)
            {
                continue;
            }

            if (isSuccessEffect)
            {
                SuccessEffectAutoReturn autoReturn = effectObject.GetComponent<SuccessEffectAutoReturn>();
                if (autoReturn != null)
                {
                    autoReturn.StopAndClear();
                }
                else
                {
                    StopAllParticlesInHierarchy(effectObject);
                }
            }
            else
            {
                StopAllParticlesInHierarchy(effectObject);
            }

            effectObject.SetActive(false);
            effectObject.transform.SetParent(transform, false);
            pool.availableObjects.Enqueue(effectObject);
            pool.availableSet.Add(effectObject);
        }
    }

    // 비활성화 도중 재부모화 없이 모든 이펙트를 정지시키는 루틴
    //
    // OnDisable 시점에는 안전성을 우선해 "정지 + 비활성화"만 수행한다.
    private void StopAllEffectsWithoutReparent(EffectPool pool, bool isSuccessEffect)
    {
        if (pool == null || pool.allObjects.Count == 0)
        {
            return;
        }

        for (int i = 0; i < pool.allObjects.Count; i++)
        {
            GameObject effectObject = pool.allObjects[i];
            if (effectObject == null)
            {
                continue;
            }

            if (isSuccessEffect)
            {
                SuccessEffectAutoReturn autoReturn = effectObject.GetComponent<SuccessEffectAutoReturn>();
                if (autoReturn != null)
                {
                    autoReturn.StopAndClear();
                }
                else
                {
                    StopAllParticlesInHierarchy(effectObject);
                }
            }
            else
            {
                StopAllParticlesInHierarchy(effectObject);
            }

            effectObject.SetActive(false);
        }
    }

    // 오브젝트 하위의 모든 파티클 시스템을 재생한다.
    //
    // 여러 파티클이 중첩된 프리팹도 한 번에 다룰 수 있도록 계층 전체를 순회한다.
    private void PlayAllParticlesInHierarchy(GameObject rootObject)
    {
        if (rootObject == null)
        {
            return;
        }

        ParticleSystem[] particles = rootObject.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem particle = particles[i];
            if (particle == null)
            {
                continue;
            }

            particle.gameObject.SetActive(true);
            particle.Clear(true);
            particle.Play(true);
        }
    }

    // 오브젝트 하위의 모든 파티클 시스템을 정지하고 비운다.
    //
    // 반환 시 이전 재생 흔적이 남지 않도록 StopEmittingAndClear를 사용한다.
    private void StopAllParticlesInHierarchy(GameObject rootObject)
    {
        if (rootObject == null)
        {
            return;
        }

        ParticleSystem[] particles = rootObject.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem particle = particles[i];
            if (particle == null)
            {
                continue;
            }

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}