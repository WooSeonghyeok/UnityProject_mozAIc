using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EP2CutsceneTriggerManager : MonoBehaviour
{
    private string scene;
    public SaveDataObj CurData;
    private bool paintSequencePlaying = false;
    void Start()
    {
        scene = SceneManager.GetActiveScene().name;
        CurData = SaveManager.instance.curData;
        if (EP2CutsceneManager.Instance == null)
        {
            Debug.LogWarning("EP2CutsceneManager 없음!");
            return;
        }
        // 🎬 Episode2 Intro
        if (scene == "Episode2_Scene" && !CurData.Played_Episode2_Intro)
        {
            CurData.Played_Episode2_Intro = true;
            SaveManager.WriteCurJSON(CurData);
            EP2CutsceneManager.Instance.Play("Episode2_Intro");
            return;
        }
        // 🎬 Space Intro
        if (scene == "Space_Puzzle" && !CurData.Played_Space_Intro)  // Space 퍼즐 처음
        {
            CurData.Played_Space_Intro = true;
            SaveManager.WriteCurJSON(CurData);
            EP2CutsceneManager.Instance.Play("Space_Intro");
        }
        // 🎬 Paint Intro
        if (scene == "Paint_Puzzle" && !CurData.Played_Paint_Intro)  // Paint 퍼즐 처음
        {
            CurData.Played_Paint_Intro = true;
            SaveManager.WriteCurJSON(CurData);
            EP2CutsceneManager.Instance.Play("Paint_Intro");
        }

        // 🎬 Episode2 복귀 컷씬
        if (scene == "Episode2_Scene")
        {
            if (CurData.ep2_spaceClear && !CurData.Played_Space_Clear)
            {
                CurData.Played_Space_Clear = true;
                SaveManager.WriteCurJSON(CurData);
                EP2CutsceneManager.Instance.Play("Space_Clear");
                return;
            }
            if (CurData.ep2_paintClear && !CurData.Played_Paint_Clear)
            {
                CurData.Played_Paint_Clear = true;
                SaveManager.WriteCurJSON(CurData);
                EP2CutsceneManager.Instance.Play("Paint_Clear");
                return;
            }
        }
    }

    void Update()
    {
        if (EP2CutsceneManager.Instance == null) return;
        // ⭐ F5 = 완전 초기화
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ResetAll();
        }
        // ⭐ 엔딩 조건 (추가🔥)
        if (scene == "Episode2_Scene")
        {
            if (CurData.ep2_spaceClear &&
                CurData.ep2_paintClear &&
                !CurData.Played_EP2_Ending)
            {
                CurData.Played_EP2_Ending = true;
                SaveManager.WriteCurJSON(CurData);

                var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();
                if (ctrl != null)
                {
                    ctrl.Episode2Ending();
                }
            }
        }
        // ⭐ Space Clear
        if (scene == "Space_Puzzle")
        {
            if (CurData.ep2_spaceClear &&
                !CurData.Played_Space_Clear_Immediate &&
                CurData.Played_Space_Intro)
            {
                CurData.Played_Space_Clear_Immediate = true;
                SaveManager.WriteCurJSON(CurData);
                EP2CutsceneManager.Instance.Play("Space_Clear_Immediate");
            }
        }
        // 🔥 Paint 전체 연출 (핵심🔥🔥🔥)
        if (scene == "Paint_Puzzle")
        {
            if (CurData.ep2_paintClear &&
                !CurData.Played_Paint_Sequences &&
                CurData.Played_Paint_Intro &&
                !paintSequencePlaying)
            {
                CurData.Played_Paint_Sequences = true;
                SaveManager.WriteCurJSON(CurData);
                StartCoroutine(PaintSequence());
            }
        }
        if (scene == "Space_Puzzle")
        {
            if (CurData.Played_Space_Intro && !CurData.Played_Space_Text)
            {
                CurData.Played_Space_Text = true;
                SaveManager.WriteCurJSON(CurData);
                StartCoroutine(SpaceIntroSequence());
            }
        }
    }

    // ===============================
    // 🎬 Paint 연출 전체
    // ===============================
    IEnumerator PaintSequence()
    {
        paintSequencePlaying = true;

        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();
        if (ctrl == null) yield break;

        // 1️⃣ 이미지1
        yield return StartCoroutine(PlayCutsceneAndWait("Paint_Clear_Immediate_1"));

        // 2️⃣ 텍스트1
        yield return StartCoroutine(ctrl.PaintStep3());

        // 3️⃣ 이미지2
        yield return StartCoroutine(PlayCutsceneAndWait("Paint_Clear_Immediate_2"));

        // 4️⃣ 텍스트2
        yield return StartCoroutine(ctrl.PaintPuzzleComplete());

        paintSequencePlaying = false;
    }

    IEnumerator SpaceIntroSequence()
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();
        if (ctrl == null) yield break;

        // ⭐ 이미지 끝날 때까지 기다림
        yield return StartCoroutine(PlayCutsceneAndWait("Space_Intro"));

        // ⭐ 텍스트 실행
        yield return StartCoroutine(ctrl.SpacePuzzleStart());
    }

    IEnumerator PlayCutsceneAndWait(string name)
    {
        bool done = false;

        System.Action callback = () => { done = true; };

        EP2CutsceneManager.Instance.OnCutsceneEnd += callback;
        EP2CutsceneManager.Instance.Play(name);

        yield return new WaitUntil(() => done);

        EP2CutsceneManager.Instance.OnCutsceneEnd -= callback;
    }

    // ===============================
    // 🔥 초기화
    // ===============================
    void ResetAll()
    {
        var data = SaveManager.instance.curData;
        data.Played_Episode2_Intro = false;
        data.Played_EP2_Text_Intro = false;
        data.Played_Space_Intro = false;
        data.Played_Paint_Intro = false;
        data.Played_Space_Clear = false;
        data.Played_Paint_Clear = false;
        data.Played_Space_Clear_Immediate = false;
        data.Played_Paint_Sequences = false;
        SaveManager.WriteCurJSON(CurData);
        Debug.Log("🔥 완전 초기화 완료 (F5)");
    }
}