using UnityEngine;

public class CylinderSwitchController : MonoBehaviour
{
    public GameObject[] cylinders;

    public float fadeDuration = 1f;
    public float delayBetween = 1f;

    [Header("Alpha Limit")]
    public float maxAlpha = 0.8f; // ⭐ 최대 알파값

    private int currentIndex = 0;
    private float timer = 0f;
    private bool isFading = false;

    private Renderer currentRend;
    private Renderer nextRend;

    void Start()
    {
        for (int i = 0; i < cylinders.Length; i++)
        {
            cylinders[i].SetActive(i == 0);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!isFading && timer >= delayBetween)
        {
            StartFade();
        }

        if (isFading)
        {
            float t = timer / fadeDuration;
            t = Mathf.Clamp01(t);

            // ⭐ 핵심: maxAlpha 적용
            SetAlpha(currentRend, maxAlpha * (1f - t));
            SetAlpha(nextRend, maxAlpha * t);

            if (t >= 1f)
            {
                cylinders[currentIndex].SetActive(false);

                currentIndex = (currentIndex + 1) % cylinders.Length;

                timer = 0f;
                isFading = false;
            }
        }
    }

    void StartFade()
    {
        int nextIndex = (currentIndex + 1) % cylinders.Length;

        cylinders[nextIndex].SetActive(true);

        currentRend = cylinders[currentIndex].GetComponent<Renderer>();
        nextRend = cylinders[nextIndex].GetComponent<Renderer>();

        SetAlpha(currentRend, maxAlpha);
        SetAlpha(nextRend, 0f);

        timer = 0f;
        isFading = true;
    }

    void SetAlpha(Renderer rend, float alpha)
    {
        if (rend == null) return;

        Material mat = rend.material;

        if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
        }
    }
}