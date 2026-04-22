using UnityEngine;
using System.Collections;

public class PaintManager : MonoBehaviour
{
    public PuzzleTile[] tiles;

    // ⭐ 여러 개 할당 가능하도록 변경
    public GameObject[] activateObjects;

    private bool isActivated = false;
    private int clearedCount = 0;

    private bool step1Played = false;
    private bool step5Played = false;
    void Update()
    {
        if (isActivated) return;

        int currentCleared = 0;

        foreach (PuzzleTile tile in tiles)
        {
            if (tile.IsCleared())
                currentCleared++;
        }

        if (currentCleared != clearedCount)
        {
            clearedCount = currentCleared;
            CheckStepDialogue(clearedCount);
        }

        if (currentCleared == tiles.Length)
        {
            Activate();
        }
    }

    void CheckStepDialogue(int count)
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();
        if (ctrl == null) return;

        if (count >= 1 && !step1Played)
        {
            step1Played = true;
            StartCoroutine(ctrl.PaintStep1());
        }

        if (count >= 5 && !step5Played)
        {
            step5Played = true;
            StartCoroutine(ctrl.PaintStep2());
        }
    }

    void Activate()
    {
        isActivated = true;

        // ⭐ 클리어 상태 저장
        PlayerPrefs.SetInt("Paint_Cleared", 1);
        PlayerPrefs.Save();

        //Episode2ScoreManager.Instance?.AddClearScore(5);

        EP2_PuzzleManager.Instance?.SolvePaintPuzzle();

        // ⭐ 여러 오브젝트 활성화
        if (activateObjects != null)
        {
            foreach (GameObject obj in activateObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        Debug.Log("Paint 퍼즐 클리어!");
    }
}