using UnityEngine;

public class CutsceneTriggerManager : MonoBehaviour
{
    public CutsceneManager cutsceneManager;

    void Start()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Episode2 √≥¿Ω ¡¯¿‘
        if (scene == "Episode2Scene" && PlayerPrefs.GetInt("Episode2_Visited", 0) == 0)
        {
            PlayerPrefs.SetInt("Episode2_Visited", 1);
            cutsceneManager.Play("Episode2_Intro");
        }

        // Space ∆€¡Ò √≥¿Ω
        if (scene == "SpacePuzzleScene" && PlayerPrefs.GetInt("Space_Visited", 0) == 0)
        {
            PlayerPrefs.SetInt("Space_Visited", 1);
            cutsceneManager.Play("Space_Intro");
        }

        // Paint ∆€¡Ò √≥¿Ω
        if (scene == "PaintPuzzleScene" && PlayerPrefs.GetInt("Paint_Visited", 0) == 0)
        {
            PlayerPrefs.SetInt("Paint_Visited", 1);
            cutsceneManager.Play("Paint_Intro");
        }

        // ∆€¡Ò ≈¨∏ÆæÓ »ƒ ∫π±Õ
        if (scene == "Episode2Scene")
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
}