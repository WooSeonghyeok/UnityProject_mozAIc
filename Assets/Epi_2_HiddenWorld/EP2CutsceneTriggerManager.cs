using UnityEngine;
using UnityEngine.SceneManagement;

public class EP2CutsceneTriggerManager : MonoBehaviour
{
    private string scene;

    void Start()
    {
        scene = SceneManager.GetActiveScene().name;

        if (EP2CutsceneManager.Instance == null)
        {
            Debug.LogWarning("EP2CutsceneManager 없음!");
            return;
        }

        // 🎬 Episode2 Intro (한 번만)
        if (scene == "Episode2_Scene" &&
            PlayerPrefs.GetInt("Played_Episode2_Intro", 0) == 0)
        {
            PlayerPrefs.SetInt("Played_Episode2_Intro", 1);
            PlayerPrefs.Save();

            EP2CutsceneManager.Instance.Play("Episode2_Intro");
            return;
        }

        // 🎬 Space Intro (한 번만)
        if (scene == "Space_Puzzle" &&
            PlayerPrefs.GetInt("Played_Space_Intro", 0) == 0)
        {
            PlayerPrefs.SetInt("Played_Space_Intro", 1);
            PlayerPrefs.Save();

            EP2CutsceneManager.Instance.Play("Space_Intro");
        }

        // 🎬 Paint Intro (한 번만)
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
            // Space 클리어 복귀
            if (PlayerPrefs.GetInt("Space_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Space_Clear", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Space_Clear", 1);
                PlayerPrefs.Save();

                EP2CutsceneManager.Instance.Play("Space_Clear");
                return;
            }

            // Paint 클리어 복귀
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

        // ⭐ F5 누르면 컷씬 초기화
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ResetCutscenes();
        }

        // ⭐ Space 클리어 즉시 컷씬
        if (scene == "Space_Puzzle")
        {
            if (PlayerPrefs.GetInt("Space_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Space_Clear_Immediate", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Space_Clear_Immediate", 1);
                PlayerPrefs.Save();

                EP2CutsceneManager.Instance.Play("Space_Clear_Immediate");
            }
        }

        // ⭐ Paint 클리어 즉시 컷씬
        if (scene == "Paint_Puzzle")
        {
            if (PlayerPrefs.GetInt("Paint_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Paint_Clear_Immediate", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Paint_Clear_Immediate", 1);
                PlayerPrefs.Save();

                EP2CutsceneManager.Instance.Play("Paint_Clear_Immediate");
            }
        }
    }

    // ⭐ 컷씬만 초기화 (핵심)
    void ResetCutscenes()
    {
        PlayerPrefs.DeleteKey("Played_Episode2_Intro");
        PlayerPrefs.DeleteKey("Played_Space_Intro");
        PlayerPrefs.DeleteKey("Played_Paint_Intro");

        PlayerPrefs.DeleteKey("Played_Space_Clear");
        PlayerPrefs.DeleteKey("Played_Paint_Clear");

        PlayerPrefs.DeleteKey("Played_Space_Clear_Immediate");
        PlayerPrefs.DeleteKey("Played_Paint_Clear_Immediate");

        PlayerPrefs.Save();

        Debug.Log("컷씬 초기화 완료 (F5)");
    }
}