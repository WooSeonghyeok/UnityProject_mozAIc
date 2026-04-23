using UnityEngine;

public class InteractPointAlarm : MonoBehaviour
{
    private readonly string playerTag = "Player";
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            SaveUIManager.instance.InteractUIOpen(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            SaveUIManager.instance.InteractUIOpen(false);
        }
    }
}
