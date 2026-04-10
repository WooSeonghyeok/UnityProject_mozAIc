using UnityEngine;

public class SpacePuzzleController : MonoBehaviour
{
    public GameObject obj1;
    public GameObject obj2;
    public GameObject obj3;

    [Header("Activation Objects")]
    public GameObject portalPrefab; // 포탈
    public GameObject[] activateObjects; // ⭐ 추가 활성화 오브젝트들

    private bool isActivated = false;

    private SpaceScoreController scoreController;

    void Start()
    {
        scoreController = GetComponent<SpaceScoreController>();
    }

    void Update()
    {
        if (isActivated) return;

        if (obj1.activeSelf && obj2.activeSelf && obj3.activeSelf)
        {
            ActivatePortal();
            isActivated = true;
        }
    }

    void ActivatePortal()
    {
        Debug.Log("3개 완료 → 포탈 및 오브젝트 활성화");

        if (SaveManager.instance != null)
            SaveManager.instance.curData.ep2_spaceClear = true;

        // ⭐ 포탈 활성화
        if (portalPrefab != null)
            portalPrefab.SetActive(true);

        // ⭐ 추가 오브젝트 활성화
        if (activateObjects != null)
        {
            foreach (GameObject obj in activateObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        // ⭐ 타이머 멈추기
        scoreController?.StopTimer();

        // ⭐ 점수 추가
        Episode2ScoreManager.Instance?.AddClearScore(5);

        // ⭐ 상태 처리
        EP2_PuzzleManager.Instance?.SolveSpacePuzzle();
    }
}