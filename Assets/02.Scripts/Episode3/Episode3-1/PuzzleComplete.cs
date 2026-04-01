using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 모든 조각 수집 시 활성화/연출 담당.
/// - completedSheetObject와 spotlightObjects를 시작 시 비활성화합니다.
/// - Ep3_1Manager.onAllPiecesCollected를 구독하여 연출을 실행합니다.
/// </summary>
public class PuzzleComplete : MonoBehaviour
{
    [Header("연결 대상")]
    [SerializeField] private GameObject completedSheetObject;
    [SerializeField] private GameObject[] spotlightObjects;

    [Header("출구 문")]
    [SerializeField] private GameObject doorObject;

    [Header("오디오")]
    [SerializeField] private AudioClip completeClip;
    [SerializeField] private AudioSource audioSource;

    [Header("동작 옵션")]
    [SerializeField] private bool autoFinalizeStage = false;
    [SerializeField] private float postAudioDelay = 0.5f;

    [Header("디자이너 훅")]
    public UnityEvent onCompletion;

    private bool _activated = false;

    private WaitForSeconds ws;

    private void Awake()
    {
        // 이벤트 구독은 먼저 수행
        var ep = FindObjectOfType<Ep3_1Manager>();
        if (ep != null)
        {
            ep.onAllPiecesCollected.AddListener(OnAllPiecesCollected);
        }

        // 시작 시 보이는 것을 방지하기 위해 비활성화
        if (completedSheetObject != null)
            completedSheetObject.SetActive(false);

        if (spotlightObjects != null)
        {
            foreach (var s in spotlightObjects)
            {
                if (s != null) s.SetActive(false);
            }
        }

        // 문도 시작 시 비활성화
        if (doorObject != null)
            doorObject.SetActive(false);
    }

    public void OnAllPiecesCollected()
    {
        if (_activated) return;
        StartCoroutine(DoCompletionSequence());
    }

    private IEnumerator DoCompletionSequence()
    {
        _activated = true;

        // 악보 등장
        if (completedSheetObject != null)
            completedSheetObject.SetActive(true);

        // 스포트라이트 등장
        if (spotlightObjects != null)
        {
            foreach (var s in spotlightObjects)
            {
                if (s != null) s.SetActive(true);
            }
        }

        // 완료 사운드 재생
        float clipLength = 0f;
        if (completeClip != null)
        {
            if (audioSource != null)
            {
                audioSource.clip = completeClip;
                audioSource.Play();
            }
            else
            {
                AudioSource.PlayClipAtPoint(completeClip, transform.position);
            }
            clipLength = completeClip.length;
        }

        ws = new WaitForSeconds(3.0f);

        yield return ws; // 사운드 길이와 별개로 최소 3초 대기 (사운드가 짧을 경우에도 연출이 충분히 보이도록)

        // 문 등장
        if (doorObject != null)
            doorObject.SetActive(true);

        // 컷신/사운드 종료까지 대기
        float waitTime = Mathf.Max(0f, clipLength) + postAudioDelay;
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        // 추가 이벤트 실행
        onCompletion?.Invoke();

        // 필요 시 스테이지 마무리
        if (autoFinalizeStage)
            FinalizeStage();
    }

    public void FinalizeStage()
    {
        var ep = FindObjectOfType<Ep3_1Manager>();
        if (ep != null)
        {
            ep.CompleteStage();
        }
        else
        {
            Debug.LogWarning("[PuzzleComplete] Ep3_1Manager를 찾을 수 없어 CompleteStage를 호출하지 못했습니다.");
        }
    }

    public void FinalizeStageAfterDelay(float delaySeconds)
    {
        StartCoroutine(FinalizeAfterDelayCoroutine(delaySeconds));
    }

    private IEnumerator FinalizeAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delay));
        FinalizeStage();
    }
}