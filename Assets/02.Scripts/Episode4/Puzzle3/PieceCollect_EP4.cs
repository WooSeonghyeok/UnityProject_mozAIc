using UnityEngine;
[RequireComponent(typeof(Collider))]
public class PieceCollect_EP4 : MonoBehaviour
{
    [Header("수집 이펙트/사운드 (선택)")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float destroyDelay = 0.1f;
    [Header("수집 연출")]
    [SerializeField] private bool useCollectFlyEffect = true;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private float flyDuration = 0.8f;
    private bool _collected = false;
    private void Reset()
    {
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
        Vector3 effectSpawnPos = GetVisualWorldPosition();
        if (useCollectFlyEffect) CreateCollectVisual();
        if (collectEffect != null)
        {
            ParticleSystem ps = Instantiate(collectEffect, effectSpawnPos, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + 0.5f);
        }
        if (collectSound != null) AudioSource.PlayClipAtPoint(collectSound, effectSpawnPos);
        Ep4_Puzzle3Manager manager = FindObjectOfType<Ep4_Puzzle3Manager>();
        if (manager != null) manager.AddPiece();
        else Debug.LogWarning("[PieceCollect] Ep4_3Manager를 찾을 수 없습니다. AddPiece 호출 실패.");
        HideAndDestroy();
    }
    private Vector3 GetVisualWorldPosition()
    {
        if (visualRoot != null) return visualRoot.transform.position;
        return transform.position;
    }
    private void CreateCollectVisual()
    {
        GameObject targetVisual = visualRoot != null ? visualRoot : gameObject;
        Vector3 spawnPos = targetVisual.transform.position;
        Quaternion spawnRot = targetVisual.transform.rotation;
        GameObject clone = Instantiate(targetVisual, spawnPos, spawnRot);  // visualRoot만 복제해서 연출
        clone.transform.SetParent(null, true);  // 월드 좌표 유지
        Collider[] cloneCols = clone.GetComponentsInChildren<Collider>();  // 복제본 콜라이더 비활성화
        foreach (Collider c in cloneCols)
        {
            c.enabled = false;
        }
        PieceCollect pc = clone.GetComponent<PieceCollect>();  // 혹시 수집 스크립트가 따라왔으면 제거
        if (pc != null) Destroy(pc);
        CollectFlyEffect fly = clone.AddComponent<CollectFlyEffect>();  // 연출 스크립트 추가
        fly.Initialize(flyDuration);
    }
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