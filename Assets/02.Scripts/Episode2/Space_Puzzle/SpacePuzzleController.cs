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
        if (obj1.activeSelf && obj2.activeSelf && obj3.activeSelf)  // 🔥 3개 다 활성화됐는지 체크
        {
            ActivatePortal();
            isActivated = true;
        }
    }
    void ActivatePortal()
    {
        Debug.Log("3개 완료 → 포탈 생성");
        if (SaveManager.instance != null) SaveManager.instance.curData.ep2_spaceClear = true;
        if (portalPrefab != null)  portalPrefab.SetActive(true);  // 🔥 포탈 활성화
        EP2_PuzzleManager.Instance.SolveSpacePuzzle();  // 🔥 상태 저장 (핵심!)
    }
}