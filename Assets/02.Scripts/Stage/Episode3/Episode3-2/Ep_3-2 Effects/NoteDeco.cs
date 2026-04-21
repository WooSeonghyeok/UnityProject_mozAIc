using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteDeco : MonoBehaviour
{
    [Header("떠다니기 설정")]
    [SerializeField] private bool useFloatMotion = true;
    [SerializeField] private float floatHeight = 0.25f;   // 위아래로 움직이는 높이
    [SerializeField] private float floatSpeed = 1.2f;     // 떠다니는 속도

    [Header("회전 설정")]
    [SerializeField] private bool useRotation = true;
    [SerializeField] private Vector3 rotationAxis = new Vector3(0f, 1f, 0f); // 회전 축
    [SerializeField] private float rotationSpeed = 20f;   // 초당 회전 속도

    [Header("랜덤 오프셋")]
    [SerializeField] private bool randomizeOnStart = true;
    [SerializeField] private float randomHeightMin = 0.15f;
    [SerializeField] private float randomHeightMax = 0.35f;
    [SerializeField] private float randomSpeedMin = 0.8f;
    [SerializeField] private float randomSpeedMax = 1.5f;
    [SerializeField] private float randomRotationMin = 10f;
    [SerializeField] private float randomRotationMax = 25f;

    [Header("색상 랜덤")]
    [SerializeField] private bool useRandomColor = true;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color minColor = new Color(0.6f, 0.3f, 1f);   // 보라
    [SerializeField] private Color maxColor = new Color(0.3f, 0.7f, 1f);   // 푸른색
    [SerializeField] private bool useEmission = true;
    [SerializeField] private float emissionIntensity = 1.5f;

    private Vector3 startPos;      // 시작 위치 저장
    private float randomOffset;    // 각 오브젝트마다 다른 흔들림 타이밍
    private Material runtimeMaterial;

    private void Start()
    {
        startPos = transform.position;

        // 음표마다 움직임 시작 타이밍이 다르게 보이게 하는 랜덤값
        randomOffset = Random.Range(0f, 100f);

        if (randomizeOnStart)
        {
            floatHeight = Random.Range(randomHeightMin, randomHeightMax);
            floatSpeed = Random.Range(randomSpeedMin, randomSpeedMax);
            rotationSpeed = Random.Range(randomRotationMin, randomRotationMax);

            // 회전 방향도 랜덤하게 뒤집기
            if (Random.value > 0.5f)
            {
                rotationSpeed *= -1f;
            }
        }

        if (useRandomColor)
        {
            ApplyRandomColor();
        }
    }

    private void Update()
    {
        FloatMotion();
        RotateMotion();
    }

    private void FloatMotion()
    {
        if (!useFloatMotion) return;

        float y = Mathf.Sin((Time.time + randomOffset) * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, startPos.y + y, startPos.z);
    }

    private void RotateMotion()
    {
        if (!useRotation) return;

        transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void ApplyRandomColor()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null)
        {
            Debug.LogWarning(name + " : Renderer가 없어서 색상을 적용할 수 없습니다.");
            return;
        }

        // 오브젝트마다 개별 머티리얼 생성
        runtimeMaterial = targetRenderer.material;

        Color randomColor = Color.Lerp(minColor, maxColor, Random.Range(0f, 1f));

        // Standard 셰이더용
        if (runtimeMaterial.HasProperty("_Color"))
        {
            runtimeMaterial.color = randomColor;
        }

        // URP Lit 셰이더용
        if (runtimeMaterial.HasProperty("_BaseColor"))
        {
            runtimeMaterial.SetColor("_BaseColor", randomColor);
        }

        // Emission 색상도 같이 적용
        if (useEmission && runtimeMaterial.HasProperty("_EmissionColor"))
        {
            runtimeMaterial.EnableKeyword("_EMISSION");
            runtimeMaterial.SetColor("_EmissionColor", randomColor * emissionIntensity);
        }
    }
}