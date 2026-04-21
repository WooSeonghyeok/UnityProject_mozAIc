using UnityEngine;

public class EP2_InteractObject : MonoBehaviour
{
    private bool isUsed = false;
    readonly string playerTag = "Player";
    private PlayerInput user;
    public SaveDataObj CurData;
    bool isContact = false;
    [Header("Interaction Effect")]
    public GameObject interactionEffectPrefab;
    private void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        CurData = SaveManager.instance.curData;
    }
    private void OnEnable()
    {
        if (user != null) user.Interact += Interact;
    }
    private void OnDisable()
    {
        if (user != null) user.Interact -= Interact;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isContact = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isContact = false;
        }
    }

    public void Interact()
    {
        if (isUsed || !isContact) return;
        isUsed = true;
        Episode2ScoreManager.Instance?.AddInteractionScore(1);
        Debug.Log($"{gameObject.name} 상호작용 +1");
       
        GetComponent<Collider>().enabled = false;
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
         // 🔥 선택: 다시 못 누르게
        GetComponent<Collider>().enabled = false;
        // 또는
        // gameObject.SetActive(false);
    }
}