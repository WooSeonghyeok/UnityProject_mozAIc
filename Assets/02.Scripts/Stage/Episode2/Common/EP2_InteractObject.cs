using UnityEngine;

public class EP2_InteractObject : MonoBehaviour
{
    private bool isUsed = false;
    readonly string playerTag = "Player";
    private PlayerInput user;
    public SaveDataObj CurData;
    bool isContact = false;
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
        if (isUsed) return;

        isUsed = true;

        Episode2ScoreManager.Instance?.AddInteractionScore(1);

        Debug.Log($"{gameObject.name} 상호작용 +1");

        // 🔥 선택: 다시 못 누르게
        GetComponent<Collider>().enabled = false;

        // 또는
        // gameObject.SetActive(false);
    }
}