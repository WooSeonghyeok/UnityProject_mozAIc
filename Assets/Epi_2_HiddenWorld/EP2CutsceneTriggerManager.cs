using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EP2CutsceneTriggerManager : MonoBehaviour
{
    private string scene;

    private bool paintSequencePlaying = false;

    void Start()
    {
        scene = SceneManager.GetActiveScene().name;

        if (EP2CutsceneManager.Instance == null)
        {
            Debug.LogWarning("EP2CutsceneManager 없음!");
            return;
        }

        // 🎬 Episode2 Intro
        if (scene == "Episode2_Scene" &&
            PlayerPrefs.GetInt("Played_Episode2_Intro", 0) == 0)
        {
            PlayerPrefs.SetInt("Played_Episode2_Intro", 1);
            PlayerPrefs.Save();

            EP2CutsceneManager.Instance.Play("Episode2_Intro");
            return;
        }

        // 🎬 Space Intro
        if (scene == "Space_Puzzle" &&
            PlayerPrefs.GetInt("Played_Space_Intro", 0) == 0)
        {
            PlayerPrefs.SetInt("Played_Space_Intro", 1);
            PlayerPrefs.Save();

            EP2CutsceneManager.Instance.Play("Space_Intro");
        }

        // 🎬 Paint Intro
        if (scene == "Paint_Puzzle" &&
            PlayerPrefs.GetInt("Played_Paint_Intro", 0) == 0)
        {
            PlayerPrefs.SetInt("Played_Paint_Intro", 1);
            PlayerPrefs.Save();

            EP2CutsceneManager.Instance.Play("Paint_Intro");
        }

        // 🎬 Episode2 복귀 컷씬
        if (scene == "Episode2_Scene")
        {
            if (PlayerPrefs.GetInt("Space_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Space_Clear", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Space_Clear", 1);
                PlayerPrefs.Save();

                EP2CutsceneManager.Instance.Play("Space_Clear");
                return;
            }

            if (PlayerPrefs.GetInt("Paint_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Paint_Clear", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Paint_Clear", 1);
                PlayerPrefs.Save();

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
            if (PlayerPrefs.GetInt("Space_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Paint_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_EP2_Ending", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_EP2_Ending", 1);
                PlayerPrefs.Save();

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
            if (PlayerPrefs.GetInt("Space_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Space_Clear_Immediate", 0) == 0 &&
                PlayerPrefs.GetInt("Played_Space_Intro", 0) == 1)
            {
                PlayerPrefs.SetInt("Played_Space_Clear_Immediate", 1);
                PlayerPrefs.Save();

                EP2CutsceneManager.Instance.Play("Space_Clear_Immediate");
            }
        }

        // 🔥 Paint 전체 연출 (핵심🔥🔥🔥)
        if (scene == "Paint_Puzzle")
        {
            if (PlayerPrefs.GetInt("Paint_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Paint_Sequence", 0) == 0 &&
                PlayerPrefs.GetInt("Played_Paint_Intro", 0) == 1 &&
                !paintSequencePlaying)
            {
                PlayerPrefs.SetInt("Played_Paint_Sequence", 1);
                PlayerPrefs.Save();

                StartCoroutine(PaintSequence());
            }
        }

        if (scene == "Space_Puzzle")
        {
            if (PlayerPrefs.GetInt("Played_Space_Intro", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Space_Text", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Space_Text", 1);
                PlayerPrefs.Save();

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
        PlayerPrefs.DeleteKey("Played_Episode2_Intro");
        PlayerPrefs.DeleteKey("Played_Space_Intro");
        PlayerPrefs.DeleteKey("Played_Paint_Intro");
        PlayerPrefs.DeleteKey("Played_EP2_Text_Intro");

        PlayerPrefs.DeleteKey("Played_Space_Clear");
        PlayerPrefs.DeleteKey("Played_Paint_Clear");

        PlayerPrefs.DeleteKey("Played_Space_Clear_Immediate");
        PlayerPrefs.DeleteKey("Played_EP2_Ending");
        // ⭐ 핵심
        PlayerPrefs.DeleteKey("Played_Paint_Sequence");
        PlayerPrefs.DeleteKey("Played_Space_Text");

        PlayerPrefs.DeleteKey("Space_Cleared");
        PlayerPrefs.DeleteKey("Paint_Cleared");

        PlayerPrefs.Save();

        Debug.Log("🔥 완전 초기화 완료 (F5)");
    }
}