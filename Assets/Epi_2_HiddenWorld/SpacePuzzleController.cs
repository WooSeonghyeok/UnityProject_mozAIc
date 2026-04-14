using UnityEngine;

public class SpacePuzzleController : MonoBehaviour
{
    public GameObject obj1;
    public GameObject obj2;
    public GameObject obj3;

    [Header("Activation Objects")]
    public GameObject portalPrefab;
    public GameObject[] activateObjects;

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

        // ⭐ 클리어 상태 저장 (추가🔥)
        PlayerPrefs.SetInt("Space_Cleared", 1);

        if (SaveManager.instance != null)
            SaveManager.instance.curData.ep2_spaceClear = true;

        if (portalPrefab != null)
            portalPrefab.SetActive(true);

        if (activateObjects != null)
        {
            foreach (GameObject obj in activateObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        // ⭐ 컷씬
        EP2CutsceneManager.Instance.Play("Space_Clear_Immediate");

        // ⭐ 타이머 정지
        scoreController?.StopTimer();

        // ⭐ 점수
        Episode2ScoreManager.Instance?.AddClearScore(5);

        // ⭐ 상태 처리
        EP2_PuzzleManager.Instance?.SolveSpacePuzzle();
    }
}