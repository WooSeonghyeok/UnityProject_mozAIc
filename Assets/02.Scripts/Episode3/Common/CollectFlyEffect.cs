using UnityEngine;

/// <summary>
/// 수집된 오브젝트가 위로 떠오르며 회전하고 사라지는 연출 전용 스크립트
/// </summary>
public class CollectFlyEffect : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float riseHeight = 1.5f;
    [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("회전")]
    [SerializeField] private Vector3 rotateSpeed = new Vector3(0f, 360f, 0f);

    [Header("스케일 (증가만 사용)")]
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useScaleUp = true;
    [SerializeField] private float maxScale = 2f; // 시작 스케일 대비 최대 배수

    [Header("알파")]
    [SerializeField] private bool useFadeOut = false;

    private float duration = 0.8f;
    private float timer = 0f;

    private Vector3 startPos;
    private Vector3 startScale;

    private MaterialPropertyBlock mpb;
    private Renderer[] renderers;

    public void Initialize(float effectDuration)
    {
        duration = Mathf.Max(0.01f, effectDuration);
    }

    private void Awake()
    {
        startPos = transform.position;
        startScale = transform.localScale;

        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        // 1. 위로 상승
        float yOffset = riseCurve.Evaluate(t) * riseHeight;
        transform.position = startPos + Vector3.up * yOffset;

        // 2. 회전
        transform.Rotate(rotateSpeed * Time.deltaTime, Space.Self);

        // 3. 스케일 처리: 증가만 사용
        if (useScaleUp)
        {
            // scaleCurve은 0..1 범위 진행값을 반환하도록 설정하세요.
            float v = Mathf.Clamp01(scaleCurve.Evaluate(t));
            float scaleMul = Mathf.Lerp(1f, maxScale, v);
            transform.localScale = startScale * scaleMul;
        }
        else
        {
            transform.localScale = startScale;
        }

        // 4. 알파 감소 (머티리얼이 투명 지원해야 함)
        if (useFadeOut)
        {
            ApplyFade(1f - t);
        }

        // 5. 종료
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyFade(float alpha)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(mpb);

            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_BaseColor"))
            {
                Color c = r.sharedMaterial.color;
                c.a = alpha;
                mpb.SetColor("_BaseColor", c);
            }
            else if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_Color"))
            {
                Color c = r.sharedMaterial.color;
                c.a = alpha;
                mpb.SetColor("_Color", c);
            }

            r.SetPropertyBlock(mpb);
        }
    }
}