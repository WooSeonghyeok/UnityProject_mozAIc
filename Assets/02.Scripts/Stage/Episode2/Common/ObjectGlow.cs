using UnityEngine;

public class ObjectGlow : MonoBehaviour
{
    private Material mat;

    [Header("Glow Setting")]
    public Color glowColor = Color.white;
    public float maxIntensity = 2f;
    public float speed = 2f;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        float emission = Mathf.PingPong(Time.time * speed, maxIntensity);
        mat.SetColor("_EmissionColor", glowColor * emission);
    }
}