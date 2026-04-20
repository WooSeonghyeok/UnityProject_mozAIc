using UnityEngine;
using System.Collections;

public class EP2_SpacePuzzleFlowManager : MonoBehaviour
{
    public static EP2_SpacePuzzleFlowManager Instance;

    private int solveCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void OnPuzzleSolved()
    {
        solveCount++;

        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();
        if (ctrl == null) return;

        // 🔵 퍼즐 1개
        if (solveCount == 1)
        {
            StartCoroutine(ctrl.SpacePuzzleStep1());
        }
        // 🔵 퍼즐 2개
        else if (solveCount == 2)
        {
            StartCoroutine(ctrl.SpacePuzzleStep2());
        }

        // ❌ solveCount == 3 제거 (중복 방지 핵심🔥)
    }
}