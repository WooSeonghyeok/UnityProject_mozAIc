using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteVisuallizer : MonoBehaviour
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

    private void Awake()
    {
        // 이 오브젝트에 붙어 있는 MeshRenderer를 미리 가져온다.
        // Start보다 Awake에서 가져오면 더 안정적이다.
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        // 혹시 Awake에서 못 가져왔을 경우를 대비한 안전장치
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        // 렌더러가 있으면 그 렌더러의 머터리얼을 가져온다.
        // material을 쓰면 이 오브젝트 전용 머터리얼 인스턴스를 사용하게 된다.
        if (meshRenderer != null)
        {
            noteMat = meshRenderer.material;
        }

        // 타이머 초기화
        timer = 0f;

        // 랜덤 크기 적용
        float randomScale = Random.Range(scaleMin, scaleMax);
        transform.localScale = Vector3.one * randomScale;

        // 랜덤 이동 속도 적용
        floatSpeed = Random.Range(floatSpeedMin, floatSpeedMax);

        // 랜덤 회전 속도 적용
        rotSpeed = Random.Range(rotSpeedMin, rotSpeedMax);

        // 랜덤 색상 적용
        SetRandomColor();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 위로 떠오르기
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);

        // 회전하기
        transform.Rotate(Vector3.forward * rotSpeed * Time.deltaTime, Space.World);

        // 시간이 지날수록 알파값을 1 -> 0 으로 줄여서 서서히 사라지게 만든다.
        FadeOut();

        // 수명이 다하면 비활성화
        if (timer >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    private void SetRandomColor()
    {
        if (noteMat == null) return;

        // colors 배열에 색이 하나라도 있으면 그중 랜덤 선택
        if (colors != null && colors.Length > 0)
        {
            startColor = colors[Random.Range(0, colors.Length)];
        }
        else
        {
            // 배열이 비어 있으면 완전 랜덤 색 생성
            startColor = new Color(Random.value, Random.value, Random.value, 1f);
        }

        // 혹시 인스펙터에서 알파가 0으로 들어가 있으면 안 보일 수 있으니
        // 시작 알파는 무조건 1로 고정한다.
        startColor.a = 1f;

        ApplyColor(startColor);

        Debug.Log($"{gameObject.name} 선택된 색상: {startColor}");
    }

    private void FadeOut()
    {
        if (noteMat == null) return;

        // 현재 시간이 lifeTime에서 얼마나 진행됐는지 0~1 비율 계산
        float t = Mathf.Clamp01(timer / lifeTime);

        // 알파값만 1에서 0으로 감소
        float alpha = Mathf.Lerp(1f, 0f, t);

        Color fadeColor = new Color(startColor.r, startColor.g, startColor.b, alpha);

        ApplyColor(fadeColor);
    }

    private void ApplyColor(Color color)
    {
        if (noteMat == null) return;

        // URP 머터리얼은 보통 _BaseColor를 사용한다.
        if (noteMat.HasProperty("_BaseColor"))
        {
            noteMat.SetColor("_BaseColor", color);
        }
        // 혹시 다른 셰이더라면 _Color를 사용할 수도 있다.
        else if (noteMat.HasProperty("_Color"))
        {
            noteMat.SetColor("_Color", color);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}의 머터리얼에서 색상 프로퍼티를 찾지 못했습니다.");
        }
    }
}