using UnityEngine;
/// <summary>
/// 플레이어가 닿으면 악보 조각을 수집하고 Ep3_1Manager에 수집을 보고합니다.
/// 루트 오브젝트에 붙여서 사용하세요.
/// visualRoot에는 실제로 보이는 자식 오브젝트를 연결하세요.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PieceCollect_EP4 : MonoBehaviour
{
    [Header("수집 이펙트/사운드 (선택)")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float destroyDelay = 0.1f;
    [Header("수집 연출")]
    [SerializeField] private bool useCollectFlyEffect = true;
    [SerializeField] private GameObject visualRoot;   // 실제 보이는 자식 오브젝트
    [SerializeField] private float flyDuration = 0.8f;
    private bool _collected = false;
    private void Reset()
    {
        // 루트 콜라이더는 기본적으로 Trigger로 사용
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;
        Collect();
    }
    private void Collect()
    {
        _collected = true;
        // 기준 위치는 visualRoot가 있으면 그 위치, 없으면 루트 위치 사용
        Vector3 effectSpawnPos = GetVisualWorldPosition();
        // 1. 연출용 복제본 생성
        if (useCollectFlyEffect)
        {
            CreateCollectVisual();
        }
        // 2. 파티클 이펙트 재생
        if (collectEffect != null)
        {
            ParticleSystem ps = Instantiate(collectEffect, effectSpawnPos, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + 0.5f);
        }
        // 3. 사운드 재생
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, effectSpawnPos);
        }
        // 4. 매니저에 수집 보고
        Ep4_3Manager manager = FindObjectOfType<Ep4_3Manager>();
        if (manager != null)
        {
            manager.AddPiece();
        }
        else
        {
            Debug.LogWarning("[PieceCollect] Ep3_1Manager를 찾을 수 없습니다. AddPiece 호출 실패.");
        }
        // 5. 원본 숨기고 삭제
        HideAndDestroy();
    }
    /// <summary>
    /// visualRoot가 있으면 그 월드 위치를, 없으면 루트 위치를 반환
    /// </summary>
    private Vector3 GetVisualWorldPosition()
    {
        if (visualRoot != null) return visualRoot.transform.position;
        return transform.position;
    }
    /// <summary>
    /// 수집 연출용 비주얼 복제본 생성
    /// </summary>
    private void CreateCollectVisual()
    {
        GameObject targetVisual = visualRoot != null ? visualRoot : gameObject;
        Vector3 spawnPos = targetVisual.transform.position;
        Quaternion spawnRot = targetVisual.transform.rotation;
        // visualRoot만 복제해서 연출
        GameObject clone = Instantiate(targetVisual, spawnPos, spawnRot);
        // 월드 좌표 유지
        clone.transform.SetParent(null, true);
        // 복제본 콜라이더 비활성화
        Collider[] cloneCols = clone.GetComponentsInChildren<Collider>();
        foreach (Collider c in cloneCols)
        {
            c.enabled = false;
        }
        // 혹시 수집 스크립트가 따라왔으면 제거
        PieceCollect pc = clone.GetComponent<PieceCollect>();
        if (pc != null)
        {
            Destroy(pc);
        }
        // 연출 스크립트 추가
        CollectFlyEffect fly = clone.AddComponent<CollectFlyEffect>();
        fly.Initialize(flyDuration);
    }
    /// <summary>
    /// 원본 오브젝트 숨기고 삭제
    /// </summary>
    private void HideAndDestroy()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (Collider c in cols)
        {
            c.enabled = false;
        }
        Destroy(gameObject, destroyDelay);
    }
}