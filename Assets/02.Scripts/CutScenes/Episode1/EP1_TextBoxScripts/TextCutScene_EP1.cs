using UnityEngine;

public class TextCutScene_EP1 : MonoBehaviour
{
    private void Start()
    {
        if (EP1CutsceneManager.Instance == null)
        {
            Debug.LogWarning("EP1CutsceneManager 없음!");
            return;
        }

        // 컷씬 끝나면 텍스트 실행
        EP1CutsceneManager.Instance.OnCutsceneEnd += PlayText;
    }

    void PlayText()
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep1>();

        if (ctrl != null)
        {
            // 👉 원하는 텍스트 함수로 바꿔
            ctrl.StartCoroutine(ctrl.Episode1Intro());
        }
        else
        {
            Debug.LogWarning("TextboxCtrl_Ep1 없음!");
        }

        // ⭐ 한 번 실행 후 제거 (중요)
        EP1CutsceneManager.Instance.OnCutsceneEnd -= PlayText;
    }
}