using UnityEngine;
using System.Collections;

public class FurnitureEmissionFade : MonoBehaviour
{
    public float duration = 2f;
    public float endIntensity = 0f;

    private Material[] mats;
    private Color[] startEmissions;

    private Renderer rend; // 🔥 추가

    void Awake()
    {
        rend = GetComponent<Renderer>(); // 🔥 저장

        mats = rend.materials;
        startEmissions = new Color[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i].HasProperty("_EmissionColor"))
            {
                startEmissions[i] = mats[i].GetColor("_EmissionColor");
                mats[i].EnableKeyword("_EMISSION");
            }
        }
    }

    public void PlayFade()
    {
        if (!gameObject.activeInHierarchy) return;

        StopAllCoroutines();
        StartCoroutine(FadeEmission());
    }

    // 🔥 수정된 부분
    public void SetEmissionOff()
    {
        if (rend == null) return;

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i].HasProperty("_EmissionColor"))
            {
                mats[i].SetColor("_EmissionColor", Color.black);
            }
        }
    }

    IEnumerator FadeEmission()
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            float intensity = Mathf.Lerp(1f, endIntensity, t);

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_EmissionColor"))
                {
                    mats[i].SetColor("_EmissionColor", startEmissions[i] * intensity);
                }
            }

            yield return null;
        }

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i].HasProperty("_EmissionColor"))
            {
                mats[i].SetColor("_EmissionColor", startEmissions[i] * endIntensity);
            }
        }
    }
}