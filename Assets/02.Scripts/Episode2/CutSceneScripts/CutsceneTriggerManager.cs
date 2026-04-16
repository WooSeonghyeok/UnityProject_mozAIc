using UnityEngine;

public class CutsceneTriggerManager : MonoBehaviour
{
    public CutsceneManager cutsceneManager;

    private string scene;

    void Start()
    {
        scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Episode2 처음 진입
        if (scene == "Episode2_Scene" && PlayerPrefs.GetInt("Episode2_Visited", 0) == 0)
        {
            PlayerPrefs.SetInt("Episode2_Visited", 1);
            cutsceneManager.Play("Episode2_Intro");
        }

        // Space 퍼즐 처음
        if (scene == "Space_Puzzle" && PlayerPrefs.GetInt("Space_Visited", 0) == 0)
        {
            PlayerPrefs.SetInt("Space_Visited", 1);
            cutsceneManager.Play("Space_Intro");
        }

        // Paint 퍼즐 처음
        if (scene == "Paint_Puzzle" && PlayerPrefs.GetInt("Paint_Visited", 0) == 0)
        {
            PlayerPrefs.SetInt("Paint_Visited", 1);
            cutsceneManager.Play("Paint_Intro");
        }

        // Episode2 복귀 컷씬
        if (scene == "Episode2_Scene")
        {
            if (PlayerPrefs.GetInt("Space_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Space_Clear", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Space_Clear", 1);
                cutsceneManager.Play("Space_Clear");
            }

            if (PlayerPrefs.GetInt("Paint_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Paint_Clear", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Paint_Clear", 1);
                cutsceneManager.Play("Paint_Clear");
            }
        }
    }

    void Update()
    {
        // ⭐ Space 퍼즐 클리어 즉시 컷씬
        if (scene == "Space_Puzzle")
        {
            if (PlayerPrefs.GetInt("Space_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Space_Clear_Immediate", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Space_Clear_Immediate", 1);
                cutsceneManager.Play("Space_Clear_Immediate");
            }
        }

        // ⭐ Paint 퍼즐 클리어 즉시 컷씬
        if (scene == "Paint_Puzzle")
        {
            if (PlayerPrefs.GetInt("Paint_Cleared", 0) == 1 &&
                PlayerPrefs.GetInt("Played_Paint_Clear_Immediate", 0) == 0)
            {
                PlayerPrefs.SetInt("Played_Paint_Clear_Immediate", 1);
                cutsceneManager.Play("Paint_Clear_Immediate");
            }
        }
    }
}