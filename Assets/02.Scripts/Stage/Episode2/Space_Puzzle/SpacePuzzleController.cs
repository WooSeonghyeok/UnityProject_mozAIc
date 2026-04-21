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

        // ⭐ 클리어 상태 저장
        PlayerPrefs.SetInt("Space_Cleared", 1);
        PlayerPrefs.Save();

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

        // ⭐ 컷씬 끝났을 때 텍스트 연결 (핵심🔥)
        if (EP2CutsceneManager.Instance != null)
        {
            EP2CutsceneManager.Instance.OnCutsceneEnd += OnClearCutsceneEnd;

            // ⭐ 이미지 컷씬 실행
            EP2CutsceneManager.Instance.Play("Space_Clear_Immediate");
        }

        // ⭐ 타이머 정지
        scoreController?.StopTimer();

        // ⭐ 점수
        //Episode2ScoreManager.Instance?.AddClearScore(5);

        // ⭐ 퍼즐 상태 저장
        EP2_PuzzleManager.Instance?.SolveSpacePuzzle();
    }

    // ⭐ 이미지 컷씬 끝나면 텍스트 실행
    void OnClearCutsceneEnd()
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();

        if (ctrl != null)
        {
            StartCoroutine(ctrl.SpacePuzzleComplete());
        }

        // ⭐ 반드시 해제 (중요🔥)
        if (EP2CutsceneManager.Instance != null)
        {
            EP2CutsceneManager.Instance.OnCutsceneEnd -= OnClearCutsceneEnd;
        }
    }
}