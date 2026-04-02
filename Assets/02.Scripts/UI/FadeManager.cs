using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;
    public Image fadeImage;

    

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    // 🔥 추가 (Fade In)
    public IEnumerator FadeIn(float duration = 1f)
    {
        float t = 1f;

        while (t > 0)
        {
            t -= Time.deltaTime / duration;
            SetAlpha(t);
            yield return null;
        }

        SetAlpha(0);
    }

    // 🔥 추가 (Fade Out)
    public IEnumerator FadeOut(float duration = 1f)
    {
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime / duration;
            SetAlpha(t);
            yield return null;
        }

        SetAlpha(1);
    }
}