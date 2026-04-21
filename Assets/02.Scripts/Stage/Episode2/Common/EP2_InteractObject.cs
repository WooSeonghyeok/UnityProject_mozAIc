using UnityEngine;

public class EP2_InteractObject : MonoBehaviour
{
    private bool isUsed = false;

    [Header("Interaction Effect")]
    public GameObject interactionEffectPrefab;

    public void Interact()
    {
        if (isUsed) return;

        isUsed = true;

        Episode2ScoreManager.Instance?.AddInteractionScore(1);

        // ⭐ 플레이어 위치에서 이펙트 실행
        if (interactionEffectPrefab != null && Episode2ScoreManager.Instance != null)
        {
            Transform player = Episode2ScoreManager.Instance.playerTransform;

            if (player != null)
            {
                GameObject effect = Instantiate(
                    interactionEffectPrefab,
                    player.position + Vector3.up * 1.2f,
                    Quaternion.identity
                );

                Destroy(effect, 2f);
            }
        }

        Debug.Log($"{gameObject.name} 상호작용 +1");

        GetComponent<Collider>().enabled = false;
    }
}