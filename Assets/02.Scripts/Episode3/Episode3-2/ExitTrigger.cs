using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    [SerializeField] private string nextSceneName;

    [Header("문 열림 확인용")]
    [SerializeField] private DoorOpen doorOpen;

    [Header("중복 이동 방지")]
    [SerializeField] private bool onlyOnce = true;

    private bool _moved = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_moved && onlyOnce) return;
        if (!other.CompareTag("Player")) return;

        // 문이 아직 안 열렸으면 통과 불가
        if (doorOpen != null && !doorOpen.IsOpen)
            return;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[ExitTrigger] nextSceneName이 비어 있습니다.");
            return;
        }

        _moved = true;
        SceneManager.LoadScene(nextSceneName);
    }
}