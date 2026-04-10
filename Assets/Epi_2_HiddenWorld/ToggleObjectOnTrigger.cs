using UnityEngine;

public class ToggleObjectOnTrigger : MonoBehaviour
{
    [Header("Toggle Target")]
    public GameObject targetObject;

    [Header("Settings")]
    public string playerTag = "Player"; // 플레이어 태그

    private bool isActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        ToggleObject();
    }

    void ToggleObject()
    {
        if (targetObject == null) return;

        isActive = !isActive;
        targetObject.SetActive(isActive);

        Debug.Log("토글 상태: " + isActive);
    }
}