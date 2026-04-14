using UnityEngine;

public class PaintManager : MonoBehaviour
{
    public PuzzleTile[] tiles;
    public GameObject activateObject;

    private bool isActivated = false;

    void Update()
    {
        if (isActivated) return;

        bool allCleared = true;

        foreach (PuzzleTile tile in tiles)
        {
            if (!tile.IsCleared())
            {
                allCleared = false;
                break;
            }
        }

        if (allCleared)
        {
            Activate();
        }
    }

    void Activate()
    {
        isActivated = true;

        // ⭐ 클리어 상태 저장
        PlayerPrefs.SetInt("Paint_Cleared", 1);
        PlayerPrefs.Save(); // 🔥 필수

        // ⭐ 점수
        Episode2ScoreManager.Instance?.AddClearScore(5);

        // ⭐ 컷씬
        EP2CutsceneManager.Instance?.Play("Paint_Clear_Immediate");

        // ⭐ 퍼즐 매니저
        EP2_PuzzleManager.Instance?.SolvePaintPuzzle();

        if (activateObject != null)
        {
            activateObject.SetActive(true);
        }

        Debug.Log("Paint 퍼즐 클리어!");
    }
}