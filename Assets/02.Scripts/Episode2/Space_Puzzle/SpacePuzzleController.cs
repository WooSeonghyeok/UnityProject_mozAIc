using UnityEngine;

public class SpacePuzzleController : MonoBehaviour
{
    public GameObject obj1;
    public GameObject obj2;
    public GameObject obj3;

    public GameObject portalPrefab; // 🔥 포탈 (Inspector에서 넣기)

    private bool isActivated = false;

    // ⭐ ScoreController 참조
    private SpaceScoreController scoreController;

    void Start()
    {
        // ⭐ 같은 오브젝트에 붙어있다고 가정
        scoreController = GetComponent<SpaceScoreController>();
    }

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

        if (SaveManager.instance != null)
            SaveManager.instance.curData.ep2_spaceClear = true;

        if (portalPrefab != null)
            portalPrefab.SetActive(true);

        // ⭐ 타이머 멈추기 (핵심)
        scoreController?.StopTimer();

        // ⭐ 퍼즐 클리어 처리 (+5 점수 포함)
        EP2_PuzzleManager.Instance.SolveSpacePuzzle();
    }
}