using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Ep3_2StartPuzzle : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Ep3_2Manager ep3_2Manager;
    [SerializeField] private AudioSource musicSource;
    [Header("카운트다운 UI")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [Header("추가 연출/트리거")]
    [SerializeField] private GameObject eventTriggerToEnable;
    [SerializeField] private GameObject interactableToDisable;
    private bool isStarting = false;
    private bool isStarted = false;
    public void BeginStartSequence()
    {
        if (isStarting) return;
        if (isStarted) return;
        StartCoroutine(CoBeginStartSequence());
    }
    private IEnumerator CoBeginStartSequence()
    {
        isStarting = true;
        if (interactableToDisable != null) interactableToDisable.SetActive(false);  // 다시 상호작용 못 하게 비활성화
        if (countdownPanel != null)  countdownPanel.SetActive(true);  // 카운트다운 UI 켜기
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)  countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        if (countdownText != null)  countdownText.text = "START!";
        if (eventTriggerToEnable != null)   eventTriggerToEnable.SetActive(true);  // 필요하면 이벤트 트리거 활성화
        if (musicSource != null) musicSource.Play();  // 음악 시작
        if (ep3_2Manager != null)  ep3_2Manager.StartRhythmStage();  // 퍼즐 시작
        yield return new WaitForSeconds(0.7f);
        if (countdownPanel != null)    countdownPanel.SetActive(false);
        isStarting = false;
        isStarted = true;
    }
}