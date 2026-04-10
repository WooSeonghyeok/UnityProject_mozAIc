using UnityEngine;

public class PaintManager : MonoBehaviour
{
    public PuzzleTile[] tiles;        // 9개 타일 넣기
    public GameObject activateObject; // 클리어 시 활성화할 오브젝트

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
        Episode2ScoreManager.Instance?.AddClearScore(5);
        // 🔥 퍼즐 매니저에 클리어 전달 (핵심)
        EP2_PuzzleManager.Instance.SolvePaintPuzzle();

        if (activateObject != null)
        {
            activateObject.SetActive(true);
        }

        Debug.Log("Paint 퍼즐 클리어!");
    }
}