using UnityEngine;
using System.Collections;

public class PaintManager : MonoBehaviour
{
    public PuzzleTile[] tiles;
    public GameObject activateObject;

    private bool isActivated = false;

    private int clearedCount = 0;

    // ⭐ 중복 방지
    private bool step1Played = false;
    private bool step5Played = false;
    private bool step9Played = false;

    void Update()
    {
        if (isActivated) return;

        int currentCleared = 0;

        foreach (PuzzleTile tile in tiles)
        {
            if (tile.IsCleared())
                currentCleared++;
        }

        // ⭐ 개수 변화 감지
        if (currentCleared != clearedCount)
        {
            clearedCount = currentCleared;
            CheckStepDialogue(clearedCount);
        }

        // ⭐ 전체 클리어
        if (currentCleared == tiles.Length)
        {
            Activate();
        }
    }

    void CheckStepDialogue(int count)
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();

        if (ctrl == null) return;

        // 🔥 1개
        if (count >= 1 && !step1Played)
        {
            step1Played = true;
            StartCoroutine(ctrl.PaintStep1());
        }

        // 🔥 5개
        if (count >= 5 && !step5Played)
        {
            step5Played = true;
            StartCoroutine(ctrl.PaintStep2());
        }

        // 🔥 9개 (마지막)
        if (count >= tiles.Length && !step9Played)
        {
            step9Played = true;
            StartCoroutine(ctrl.PaintStep3());
        }
    }

    void Activate()
    {
        isActivated = true;

        // ⭐ 클리어 상태 저장
        PlayerPrefs.SetInt("Paint_Cleared", 1);
        PlayerPrefs.Save();

        // ⭐ 점수
        Episode2ScoreManager.Instance?.AddClearScore(5);

        // ⭐ 컷씬 끝나면 텍스트 실행 연결 (핵심🔥)
        if (EP2CutsceneManager.Instance != null)
        {
            EP2CutsceneManager.Instance.OnCutsceneEnd += OnClearCutsceneEnd;

            // ⭐ 이미지 컷씬 실행
            EP2CutsceneManager.Instance.Play("Paint_Clear_Immediate");
        }

        // ⭐ 퍼즐 매니저
        EP2_PuzzleManager.Instance?.SolvePaintPuzzle();

        // ⭐ 포탈/오브젝트 활성화
        if (activateObject != null)
        {
            activateObject.SetActive(true);
        }

        Debug.Log("Paint 퍼즐 클리어!");
    }

    // ⭐ 이미지 컷씬 끝나면 텍스트 실행
    void OnClearCutsceneEnd()
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();

        if (ctrl != null)
        {
            StartCoroutine(ctrl.PaintPuzzleComplete());
        }

        // ⭐ 반드시 해제 (중요🔥)
        if (EP2CutsceneManager.Instance != null)
        {
            EP2CutsceneManager.Instance.OnCutsceneEnd -= OnClearCutsceneEnd;
        }
    }
}