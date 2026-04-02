using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneImagePlayer : MonoBehaviour
{
    [Header("컷씬 UI")]
    [SerializeField] private GameObject cutscenePanel;   // 컷씬 전체 패널
    [SerializeField] private Image cutsceneImage;        // 화면에 이미지를 표시할 UI Image

    [Header("컷씬 이미지")]
    [SerializeField] private Sprite[] cutsceneSprites;   // 순서대로 보여줄 컷씬 이미지들
    [SerializeField] private float imageShowTime = 3f;   // 각 이미지 표시 시간
    [SerializeField] private float fadeDuration = 1f;    // 페이드 시간

    [Header("플레이어 제어")]
    [SerializeField] private PlayerMovement playerMovement; // 플레이어 이동 잠금용

    private AspectRatioFitter aspectFitter;
    private bool isPlaying = false;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (cutsceneImage != null)
        {
            // 이미지 원본 비율 유지
            cutsceneImage.preserveAspect = true;
            aspectFitter = cutsceneImage.GetComponent<AspectRatioFitter>();
        }

        if (cutscenePanel != null)
        {
            // 시작 시 컷씬 패널은 꺼둠
            cutscenePanel.SetActive(false);
        }
    }

    // 외부에서 호출하는 컷씬 시작 함수
    public void PlayCutscene()
    {
        // 이미 재생 중이면 중복 실행 방지
        if (isPlaying)
            return;

        StartCoroutine(PlayCutsceneRoutine());
    }

    // 이미지 컷씬 순차 재생 코루틴
    private IEnumerator PlayCutsceneRoutine()
    {
        isPlaying = true;

        // 컷씬 패널 켜기
        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        // 플레이어 이동 잠금
        if (playerMovement != null)
        {
            // PlayerMovement에 있는 이동 잠금 함수
            playerMovement.SetMoveLock(true);
        }

        // 마우스 커서를 보여주고 고정 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        for (int i = 0; i < cutsceneSprites.Length; i++)
        {
            if (cutsceneImage != null && cutsceneSprites[i] != null)
            {
                cutsceneImage.sprite = cutsceneSprites[i];

                //// 이미지가 바뀔 때마다 원본 비율 기준으로 Aspect Ratio 갱신
                //if (aspectFitter != null)
                //{
                //    float width = cutsceneSprites[i].rect.width;
                //    float height = cutsceneSprites[i].rect.height;
                //    aspectFitter.aspectRatio = width / height;
                //}
            }

            // 첫 이미지도 자연스럽게 보이도록 페이드 인
            yield return StartCoroutine(Fade(0f, 1f));

            // 이미지 유지 시간
            yield return new WaitForSeconds(imageShowTime);

            // 마지막 이미지가 아니면 다음 이미지 전환 전 페이드 아웃
            if (i < cutsceneSprites.Length - 1)
            {
                yield return StartCoroutine(Fade(1f, 0f));
            }
        }

        // 마지막 이미지 종료 후 페이드 아웃
        yield return StartCoroutine(Fade(1f, 0f));

        // 컷씬 종료
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        // 플레이어 이동 다시 허용
        if (playerMovement != null)
        {
            playerMovement.SetMoveLock(false);
        }

        // 다시 게임 커서 상태로 복귀
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isPlaying = false;
    }

    // 전체 컷씬 패널을 서서히 투명/불투명하게 만드는 코루틴
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (cutsceneImage == null)
            yield break;

        float elapsed = 0f;
        SetImageAlpha(startAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            SetImageAlpha(alpha);

            yield return null;
        }

        SetImageAlpha(endAlpha);
    }

    // 이미지 컬러의 알파만 변경
    private void SetImageAlpha(float alpha)
    {
        if (cutsceneImage == null)
            return;

        Color color = cutsceneImage.color;
        color.a = alpha;
        cutsceneImage.color = color;
    }
}