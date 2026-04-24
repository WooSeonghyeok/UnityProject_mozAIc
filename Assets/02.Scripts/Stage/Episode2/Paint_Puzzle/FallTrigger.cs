using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    public FallFOVEffect fovEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (fovEffect != null)
        {
            fovEffect.StartFall();
        }
    }
}