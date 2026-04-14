using UnityEngine;
using UnityEngine.SceneManagement;

public class EP2CutsceneTriggerManager : MonoBehaviour
{
    private string scene;

    [SerializeField] bool resetForTest = false;

    void Start()
    {
        scene = SceneManager.GetActiveScene().name;

        // ⭐ 테스트용 초기화 (한 번만 실행)
        if (resetForTest && PlayerPrefs.GetInt("Test_Reset_Done", 0) == 0)
        {
            PlayerPrefs.SetInt("Test_Reset_Done", 1);

            PlayerPrefs.DeleteKey("Episode2_Visited");
            PlayerPrefs.DeleteKey("Space_Visited");
            PlayerPrefs.DeleteKey("Paint_Visited");

            PlayerPrefs.DeleteKey("Space_Cleared");
            PlayerPrefs.DeleteKey("Paint_Cleared");

            PlayerPrefs.DeleteKey("Played_Space_Clear");
            PlayerPrefs.DeleteKey("Played_Paint_Clear");

            PlayerPrefs.DeleteKey("Played_Space_Clear_Immediate");
            PlayerPrefs.DeleteKey("Played_Paint_Clear_Immediate");

            PlayerPrefs.Save();

            Debug.Log("컷씬 테스트 초기화 완료");
        }

        // ⭐ Manager 체크
        if (EP2CutsceneManager.Instance == null)
        {
            Debug.LogWarning("EP2CutsceneManager 없음!");
            return;
        }

        // 🎬 Episode2 처음 진입 (⭐ 조건 강화)
        if (scene == "Episode2_Scene" &&
            PlayerPrefs.GetInt("Episode2_Visited", 0) == 0 &&
            PlayerPrefs.GetInt("Space_Cleared", 0) == 0 &&
            PlayerPrefs.GetInt("Paint_Cleared", 0) == 0)
        {
            PlayerPrefs.SetInt("Episode2_Visited", 1);
            PlayerPrefs.Save();

            EP2CutsceneManager.Instance.Play("Episode2_Intro");
        }

        // 🎬 Space 퍼즐 처음
        if (scene == "Space_Puzzle" && PlayerPrefs.GetInt("Space_Visited", 0) == 0)
        {
            PlayerPrefs.SetInt("Space_Visited", 1);
            PlayerPrefs.Save();

            EP2CutsceneManager.Instance.Play("Space_Intro");
        }

        // 🎬 Paint 퍼즐 처음
        if (scene == "Paint_Puzzle" && PlayerPrefs.GetInt("Paint_Visited", 0) == 0)
        {
            PlayerPrefs.SetInt("Paint_Visited", 1);
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
            }

            // Paint 클리어 복귀
            if (PlayerPrefs.GetInt("Paint_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Paint_Clear", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Paint_Clear", 1);
                PlayerPrefs.Save();

                EP2CutsceneManager.Instance.Play("Paint_Clear");
            }
        }
    }

    void Update()
    {
        if (EP2CutsceneManager.Instance == null) return;

        // ⭐ Space 퍼즐 클리어 즉시 컷씬
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

        // ⭐ Paint 퍼즐 클리어 즉시 컷씬
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
}