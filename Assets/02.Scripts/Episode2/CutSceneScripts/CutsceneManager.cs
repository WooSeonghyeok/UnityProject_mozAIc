using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CutsceneManager : MonoBehaviour
{
    private const float FadeDuration = 1f;
    private const float SceneHoldDuration = 2f;

    [Header("Cutscene UI")]
    public Image cutsceneImage;
    [SerializeField] private bool useRuntimeHintOverlay = false;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject nextButtonRoot;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject skipButtonRoot;

    public List<CutsceneData> cutscenes;

    Dictionary<string, Sprite[]> cutsceneDict;
    private Coroutine playRoutine;
    private bool isPlaying;
    private bool advanceRequested;
    private bool skipRequested;
    private CutsceneHintOverlay hintOverlay;

    void Awake()
    {
        cutsceneDict = new Dictionary<string, Sprite[]>();

        foreach (var c in cutscenes)
        {
            cutsceneDict.Add(c.name, c.images);
        }

        InitializeCutsceneButtons();
        SetCutsceneButtonsVisible(false);

        if (useRuntimeHintOverlay)
        {
            Transform overlayParent = cutsceneImage != null ? cutsceneImage.transform : transform;
            hintOverlay = CutsceneHintOverlay.GetOrCreate(overlayParent);
            hintOverlay?.Hide();
        }
    }

    public void Play(string name)
    {
        if (!cutsceneDict.TryGetValue(name, out Sprite[] scenes) || scenes == null || scenes.Length == 0)
        {
            Debug.LogWarning($"[CutsceneManager] 컷씬 '{name}' 을(를) 찾지 못했습니다.");
            return;
        }

        if (isPlaying)
        {
            return;
        }

        playRoutine = StartCoroutine(PlayRoutine(scenes));
    }

    public void RequestAdvance()
    {
        if (isPlaying)
        {
            advanceRequested = true;
        }
    }

    public void RequestSkip()
    {
        if (isPlaying)
        {
            skipRequested = true;
        }
    }

    public void OnNextButton()
    {
        RequestAdvance();
    }

    public void OnSkipButton()
    {
        RequestSkip();
    }

    IEnumerator PlayRoutine(Sprite[] scenes)
    {
        isPlaying = true;
        advanceRequested = false;
        skipRequested = false;
        cutsceneImage.gameObject.SetActive(true);
        SetCutsceneButtonsVisible(true);
        if (useRuntimeHintOverlay)
        {
            hintOverlay?.Show();
        }

        foreach (var scene in scenes)
        {
            advanceRequested = false;
            cutsceneImage.sprite = scene;
            cutsceneImage.color = new Color(1, 1, 1, 0);

            yield return Fade(0, 1);
            if (skipRequested)
            {
                break;
            }

            if (!advanceRequested)
            {
                yield return WaitInterruptible(SceneHoldDuration);
            }

            if (skipRequested)
            {
                break;
            }

            yield return Fade(1, 0);
            if (skipRequested)
            {
                break;
            }
        }

        cutsceneImage.color = new Color(1, 1, 1, 0);
        cutsceneImage.gameObject.SetActive(false);
        SetCutsceneButtonsVisible(false);
        if (useRuntimeHintOverlay)
        {
            hintOverlay?.Hide();
        }
        isPlaying = false;
        advanceRequested = false;
        skipRequested = false;
        playRoutine = null;
    }

    IEnumerator Fade(float start, float end)
    {
        float t = 0;

        while (t < FadeDuration)
        {
            PollCutsceneInput();
            if (skipRequested || advanceRequested)
            {
                cutsceneImage.color = new Color(1, 1, 1, end);
                yield break;
            }

            float a = Mathf.Lerp(start, end, t / FadeDuration);
            cutsceneImage.color = new Color(1, 1, 1, a);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        cutsceneImage.color = new Color(1, 1, 1, end);
    }

    IEnumerator WaitInterruptible(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            PollCutsceneInput();
            if (skipRequested || advanceRequested)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void PollCutsceneInput()
    {
        if (!skipRequested && CutsceneInputHelper.IsSkipPressedThisFrame())
        {
            skipRequested = true;
            return;
        }

        if (!advanceRequested && CutsceneInputHelper.IsAdvancePressedThisFrame())
        {
            advanceRequested = true;
        }
    }

    private void InitializeCutsceneButtons()
    {
        CutsceneControlButtonHelper.TryAutoResolve(ref nextButton, ref nextButtonRoot, ref skipButton, ref skipButtonRoot);
        CutsceneControlButtonHelper.Register(nextButton, OnNextButton);
        CutsceneControlButtonHelper.Register(skipButton, OnSkipButton);
    }

    private void SetCutsceneButtonsVisible(bool visible)
    {
        CutsceneControlButtonHelper.SetVisible(nextButton, nextButtonRoot, visible);
        CutsceneControlButtonHelper.SetVisible(skipButton, skipButtonRoot, visible);
    }
}

[System.Serializable]
public class CutsceneData
{
    public string name;
    public Sprite[] images;
}
