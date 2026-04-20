using System.Collections;
using Episode3.Common;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles the EP3-1 completion flow after all sheet pieces are collected.
/// </summary>
public class PuzzleComplete : MonoBehaviour
{
    [Header("Completion Targets")]
    [SerializeField] private GameObject completedSheetObject;
    [SerializeField] private GameObject[] spotlightObjects;
    [SerializeField] private InteractableSymbol completedSheetInteractable;
    [SerializeField] private CutsceneImagePlayer completionImageCutscene;
    [SerializeField] private Ep3LobbyIntroCutsceneController completionCutsceneController;

    [Header("Door")]
    [SerializeField] private GameObject doorObject;

    [Header("Audio")]
    [SerializeField] private AudioClip completeClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Flow Options")]
    [SerializeField] private bool autoFinalizeStage = false;
    [SerializeField] private float postAudioDelay = 0.5f;

    [Header("Designer Hook")]
    public UnityEvent onCompletion;

    private bool _activated;
    private bool _completionCutsceneTriggered;
    private bool _completionResolved;
    private WaitForSeconds _waitCache;

    private void Awake()
    {
        ResolveRuntimeReferences();

        Ep3_1Manager ep = FindObjectOfType<Ep3_1Manager>();
        if (ep != null)
            ep.onAllPiecesCollected.AddListener(OnAllPiecesCollected);

        if (completedSheetObject != null)
            completedSheetObject.SetActive(false);

        if (completedSheetInteractable != null)
            completedSheetInteractable.SetInteractionEnabled(false);

        if (spotlightObjects != null)
        {
            foreach (GameObject spotlight in spotlightObjects)
            {
                if (spotlight != null)
                    spotlight.SetActive(false);
            }
        }

        if (doorObject != null)
            doorObject.SetActive(false);

        if (completionImageCutscene != null)
            completionImageCutscene.AddFinishedListener(OnCompletionImageCutsceneFinished);
    }

    private void OnDestroy()
    {
        if (completionImageCutscene != null)
            completionImageCutscene.RemoveFinishedListener(OnCompletionImageCutsceneFinished);
    }

    public void OnAllPiecesCollected()
    {
        if (_activated)
            return;

        StartCoroutine(DoCompletionSequence());
    }

    private IEnumerator DoCompletionSequence()
    {
        _activated = true;

        if (completedSheetObject != null)
            completedSheetObject.SetActive(true);

        if (spotlightObjects != null)
        {
            foreach (GameObject spotlight in spotlightObjects)
            {
                if (spotlight != null)
                    spotlight.SetActive(true);
            }
        }

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

        float waitTime = Mathf.Max(3f, Mathf.Max(0f, clipLength) + postAudioDelay);
        if (waitTime > 0f)
        {
            _waitCache = new WaitForSeconds(waitTime);
            yield return _waitCache;
        }

        if (completedSheetInteractable != null)
        {
            completedSheetInteractable.SetInteractionEnabled(true);
            yield break;
        }

        OnCompletionCutsceneFinished();
    }

    public void FinalizeStage()
    {
        Ep3_1Manager ep = FindObjectOfType<Ep3_1Manager>();
        if (ep != null)
        {
            ep.CompleteStage();
        }
        else
        {
            Debug.LogWarning("[PuzzleComplete] Could not find Ep3_1Manager to finalize the stage.");
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

    public void PlayCompletionCutscene()
    {
        if (_completionCutsceneTriggered)
            return;

        _completionCutsceneTriggered = true;

        if (completedSheetInteractable != null)
            completedSheetInteractable.SetInteractionEnabled(false);

        if (completionImageCutscene != null && completionImageCutscene.HasConfiguredImages)
        {
            completionImageCutscene.PlayCutscene();
            return;
        }

        OnCompletionCutsceneFinished();
    }

    public void OnCompletionCutsceneFinished()
    {
        if (_completionResolved)
            return;

        _completionResolved = true;
        RevealDoor();
        onCompletion?.Invoke();

        if (autoFinalizeStage)
            FinalizeStage();
    }

    public void RevealDoor()
    {
        if (doorObject != null)
            doorObject.SetActive(true);
    }

    private void OnCompletionImageCutsceneFinished()
    {
        if (_completionResolved)
            return;

        OnCompletionCutsceneFinished();
    }

    private void ResolveRuntimeReferences()
    {
        if (completedSheetInteractable == null && completedSheetObject != null)
            completedSheetInteractable = completedSheetObject.GetComponentInChildren<InteractableSymbol>(true);

        if (completionImageCutscene == null)
            completionImageCutscene = FindObjectOfType<CutsceneImagePlayer>(true);

    }
}
