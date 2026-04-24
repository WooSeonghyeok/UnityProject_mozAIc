using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EP1CutsceneTriggerManager : MonoBehaviour
{
    public static EP1CutsceneTriggerManager Instance;

    private string scene;
    public SaveDataObj CurData;

    private bool sequencePlaying = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
        scene = SceneManager.GetActiveScene().name;
        Debug.Log("현재 씬 이름: " + scene);
        CurData = SaveManager.instance.curData;

        if (EP1CutsceneManager.Instance == null)
        {
            Debug.LogWarning("EP1CutsceneManager 없음!");
            return;
        }

        // 🎬 1️⃣ Episode1 Scene 진입 컷씬 (이미지 + 텍스트)
        if (scene == "Episode1_Scene" && !CurData.Played_Episode1_Intro)
        {
            CurData.Played_Episode1_Intro = true;
            SaveManager.WriteCurJSON(CurData);

            // ⭐ 핵심: 코루틴으로 실행
            StartCoroutine(PlayIntroSequence());
        }
    }

    void Update()
    {
        if (EP1CutsceneManager.Instance == null) return;

        // 🔥 테스트 초기화
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ResetAll();
        }
    }

    
    IEnumerator PlayIntroSequence()
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep1>();

        // ⭐ 전체 컷씬 시작
        GameManager.Instance.CutsceneMode(true);

        // 1️⃣ 이미지
        yield return StartCoroutine(PlayCutsceneAndWait("EP1_Intro"));

        // 2️⃣ 텍스트
        yield return StartCoroutine(ctrl.Episode1Intro());

        // ⭐ 전체 컷씬 종료
        GameManager.Instance.CutsceneMode(false);
    }
    // =================================================
    // ⭐ 별 획득 시 호출
    // =================================================
    public void OnStarCollected()
    {
        if (sequencePlaying || CurData.Played_StarGet) return;
        CurData.Played_StarGet = true;
        SaveManager.WriteCurJSON(CurData);

        StartCoroutine(StarSequence());
    }

    IEnumerator StarSequence()
    {
        sequencePlaying = true;

        GameManager.Instance.CutsceneMode(true);

        // 1️⃣ 컷씬 먼저
        yield return StartCoroutine(PlayCutsceneAndWait("StarGet"));

        // 2️⃣ 컷씬 끝난 뒤 텍스트 실행
        if (GameManager_Ep1.Instance != null)
        {
            GameManager_Ep1.Instance.OnFirstStarCollected();
        }

        GameManager.Instance.CutsceneMode(false);

        sequencePlaying = false;
    }

    // =================================================
    // 🎬 컷씬 기다리기
    // =================================================
    IEnumerator PlayCutsceneAndWait(string name)
    {
        bool done = false;

        System.Action callback = () => { done = true; };

        EP1CutsceneManager.Instance.OnCutsceneEnd += callback;
        EP1CutsceneManager.Instance.Play(name);

        yield return new WaitUntil(() => done);

        EP1CutsceneManager.Instance.OnCutsceneEnd -= callback;
    }

    // =================================================
    // 🔥 초기화
    // =================================================
    void ResetAll()
    {
        var data = SaveManager.instance.curData;

        data.Played_Episode1_Intro = false;

        SaveManager.WriteCurJSON(data);

        Debug.Log("🔥 EP1 초기화 완료");
    }
}