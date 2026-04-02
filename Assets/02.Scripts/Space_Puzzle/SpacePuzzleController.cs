using UnityEngine;

public class SpacePuzzleController : MonoBehaviour
{
    public GameObject obj1;
    public GameObject obj2;
    public GameObject obj3;

    public GameObject portalPrefab; // 🔥 포탈 (Inspector에서 넣기)

    bool isActivated = false;

    void Update()
    {
        if (isActivated) return;

        // 🔥 3개 다 활성화됐는지 체크
        if (obj1.activeSelf && obj2.activeSelf && obj3.activeSelf)
        {
            ActivatePortal();
            isActivated = true;
        }
    }

    void ActivatePortal()
    {
        Debug.Log("3개 완료 → 포탈 생성");

        // 🔥 포탈 활성화
        if (portalPrefab != null)
            portalPrefab.SetActive(true);

        // 🔥 상태 저장 (핵심!)
        PuzzleManager.Instance.SolveSpacePuzzle();
    }
}