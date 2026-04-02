using UnityEngine;

public class AuroraFlow : MonoBehaviour
{
    public float speedX = 0.05f;
    public float speedY = 0.0f;

    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        float offsetX = Time.time * speedX;
        float offsetY = Time.time * speedY;

        mat.SetTextureOffset("_BaseMap", new Vector2(offsetX, offsetY));
    }
}