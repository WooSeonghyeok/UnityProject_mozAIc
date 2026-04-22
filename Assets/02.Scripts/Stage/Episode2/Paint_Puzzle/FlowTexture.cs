using UnityEngine;

public class FlowTexture : MonoBehaviour
{
    [Header("Flow Speed")]
    public float speedY = 0.1f;

    [Header("Optional Sway (좌우 흔들림)")]
    public float swayAmount = 0.02f;
    public float swaySpeed = 0.5f;

    private Material mat;
    private Vector2 offset;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        offset = Vector2.zero;
    }

    void Update()
    {
        // ⭐ 세로 흐름 (핵심)
        offset.y += Time.deltaTime * speedY;

        // ⭐ 좌우 흔들림 (Seam 안 생기게 sin 사용)
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        offset.x = sway;

        mat.mainTextureOffset = offset;
    }
}