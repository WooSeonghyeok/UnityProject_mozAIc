using UnityEngine;

public class EP2_InteractObject : MonoBehaviour
{
    private bool isUsed = false;

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