using UnityEngine;
using UnityEngine.UI;

public class SelectEffectPulse : MonoBehaviour
{
    public Image img;
    public float speed = 3f;

    public float minAlpha = 0.2f;
    public float maxAlpha = 0.6f;

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}