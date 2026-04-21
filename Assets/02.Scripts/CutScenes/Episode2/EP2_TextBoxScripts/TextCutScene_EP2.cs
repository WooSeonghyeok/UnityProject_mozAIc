using UnityEngine;

public class TextCutScene_EP2 : MonoBehaviour
{
    void Start()
    {
        if (EP2CutsceneManager.Instance == null)
        {
            Debug.LogWarning("CutsceneManager 없음!");
            return;
        }

        // ⭐ 컷씬 끝났을 때 실행
        EP2CutsceneManager.Instance.OnCutsceneEnd += PlayText;
    }

    void PlayText()
    {
        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();

        if (ctrl != null)
        {
            ctrl.Episode2Start();
        }
        else
        {
            Debug.LogWarning("TextboxCtrl_Ep2 없음!");
        }

        // ⭐ 한 번 실행 후 제거 (중요🔥)
        EP2CutsceneManager.Instance.OnCutsceneEnd -= PlayText;
    }
}