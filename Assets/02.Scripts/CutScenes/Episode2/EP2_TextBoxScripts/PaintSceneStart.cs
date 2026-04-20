using UnityEngine;

public class PaintSceneStart : MonoBehaviour
{
    void Start()
    {
        if (EP2CutsceneManager.Instance == null)
        {
            Debug.LogWarning("CutsceneManager 없음!");
            return;
        }

        // ⭐ 컷씬 끝났을 때 텍스트 실행
        EP2CutsceneManager.Instance.OnCutsceneEnd += OnIntroCutsceneEnd;
    }

    void OnIntroCutsceneEnd()
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();

        if (ctrl != null)
        {
            StartCoroutine(ctrl.PaintPuzzleStart());
        }

        // ⭐ 반드시 해제 (중요🔥)
        EP2CutsceneManager.Instance.OnCutsceneEnd -= OnIntroCutsceneEnd;
    }
}