using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteVisualizer : MonoBehaviour
{
    [Header("수명")]
    [SerializeField] private float lifeTime = 2f;

    [Header("이동")]
    [SerializeField] private float floatSpeedMin = 0.3f;
    [SerializeField] private float floatSpeedMax = 0.8f;

    [Header("크기")]
    [SerializeField] private float scaleMin = 0.7f;
    [SerializeField] private float scaleMax = 1.3f;

    [Header("회전")]
    [SerializeField] private float rotSpeedMin = -20f;
    [SerializeField] private float rotSpeedMax = 20f;

    [Header("랜덤 색상 후보")]
    [SerializeField] private Color[] colors;

    private float timer;
    private float floatSpeed;
    private float rotSpeed;

    private MeshRenderer meshRenderer;
    private Material noteMat;
    private Color startColor;
    private Vector3 startScale;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            noteMat = meshRenderer.material;
        }
    }

    private void OnEnable()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (noteMat == null && meshRenderer != null)
        {
            noteMat = meshRenderer.material;
        }

        timer = 0f;

        float randomScale = Random.Range(scaleMin, scaleMax);
        startScale = Vector3.one * randomScale;
        transform.localScale = startScale;

        floatSpeed = Random.Range(floatSpeedMin, floatSpeedMax);
        rotSpeed = Random.Range(rotSpeedMin, rotSpeedMax);

        if (noteMat != null)
        {
            if (colors != null && colors.Length > 0)
            {
                startColor = colors[Random.Range(0, colors.Length)];
            }
            else
            {
                startColor = new Color(Random.value, Random.value, Random.value, 1f);
            }

            noteMat.color = startColor;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward * rotSpeed * Time.deltaTime, Space.World);

        if (noteMat != null)
        {
            float t = Mathf.Clamp01(timer / lifeTime);
            float alpha = Mathf.Lerp(startColor.a, 0f, t);

            Color fadeColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            noteMat.color = fadeColor;
        }

        if (timer >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }
}