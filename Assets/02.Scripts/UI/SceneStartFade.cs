using UnityEngine;

public class SceneStartFade : MonoBehaviour
{
    void Start()
    {
        Debug.Log("æ¿ Ω√¿€ - Fade Ω««‡µ ");

        if (FadeManager.Instance == null)
        {
            Debug.LogError("FadeManager æ¯¿Ω!");
            return;
        }

        StartCoroutine(FadeManager.Instance.FadeIn(1.5f));
    }
}