using UnityEngine;
public class SceneStartFade : MonoBehaviour
{
    void Start()
    {
        Debug.Log("씬 시작 - Fade 실행됨");
        if (FadeManager.Instance == null)
        {
            Debug.LogError("FadeManager 없음!");
            return;
        }
        StartCoroutine(FadeManager.Instance.FadeIn(1.5f));
    }
}