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
        if (scene == "Episode2_Scene" && !CurData.Episode2_Visited)
        {
            CurData.Episode2_Visited = true;
            SaveManager.WriteCurJSON(CurData);
            EP2CutsceneManager.Instance.Play("Episode2_Intro");
            return;
        }
        // 🎬 Space Intro
        if (scene == "Space_Puzzle" && !CurData.Space_Visited)  // Space 퍼즐 처음
        {
            CurData.Space_Visited = true;
            SaveManager.WriteCurJSON(CurData);
            EP2CutsceneManager.Instance.Play("Space_Intro");
        }
        // 🎬 Paint Intro
        if (scene == "Paint_Puzzle" && !CurData.Paint_Visited)  // Paint 퍼즐 처음
        {
            CurData.Paint_Visited = true;
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

        // ⭐ Space Clear
        if (scene == "Space_Puzzle")
        {
            if (CurData.ep2_spaceClear &&
                !CurData.Played_Space_Clear_Immediate &&
                CurData.Space_Visited)
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
                CurData.Paint_Visited &&
                !paintSequencePlaying)
            {
                CurData.Played_Paint_Sequences = true;
                SaveManager.WriteCurJSON(CurData);
                StartCoroutine(PaintSequence());
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
        data.Episode2_Visited = false;
        data.Space_Visited = false;
        data.Paint_Visited = false;
        data.Played_Space_Clear = false;
        data.Played_Paint_Clear = false;
        data.Played_Space_Clear_Immediate = false;
        data.Played_Paint_Sequences = false;
        PlayerPrefs.DeleteKey("Played_EP2_Text_Intro");
        // ⭐ 핵심
        PlayerPrefs.DeleteKey("Space_Cleared");
        PlayerPrefs.DeleteKey("Paint_Cleared");
        SaveManager.WriteCurJSON(CurData);
        Debug.Log("🔥 완전 초기화 완료 (F5)");
    }
}