using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EP2CutsceneManager : MonoBehaviour
{
    public static EP2CutsceneManager Instance;

    public System.Action OnCutsceneEnd; // ⭐ 추가

    [Header("UI")]
    public Image cutsceneImage;

    [Header("Cutscene Data")]
    public List<CutsceneData> cutscenes;

    private Dictionary<string, Sprite[]> cutsceneDict;

    void Awake()
    {
        Instance = this;

        cutsceneDict = new Dictionary<string, Sprite[]>();

        foreach (var c in cutscenes)
        {
            if (!cutsceneDict.ContainsKey(c.name))
            {
                cutsceneDict.Add(c.name, c.images);
            }
        }

        if (cutsceneImage != null)
            cutsceneImage.gameObject.SetActive(false);
    }

    public void Play(string name, bool cutsceneModeLock=false)
    {
        if (!cutsceneDict.ContainsKey(name))
        {
            Debug.LogWarning($"컷씬 이름 없음: {name}");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PlayRoutine(cutsceneDict[name], cutsceneModeLock));
    }

    IEnumerator PlayRoutine(Sprite[] scenes, bool keepCutsceneMode)
    {
        cutsceneImage.gameObject.SetActive(true);
        GameManager.Instance.CutsceneMode(true);
        foreach (var scene in scenes)
        {
            cutsceneImage.sprite = scene;
            cutsceneImage.color = new Color(1, 1, 1, 0);

            yield return Fade(0, 1);
            yield return new WaitForSecondsRealtime(2f);
            yield return Fade(1, 0);
        }

        cutsceneImage.gameObject.SetActive(false);
        GameManager.Instance.CutsceneMode(keepCutsceneMode);
        // ⭐ 핵심
        OnCutsceneEnd?.Invoke();
    }
    public IEnumerator PlayCutsceneAndWait(string name)
    {
        bool done = false;

        System.Action callback = () => { done = true; };

        OnCutsceneEnd += callback;
        Play(name);

        yield return new WaitUntil(() => done);

        OnCutsceneEnd -= callback;
    }
    IEnumerator Fade(float start, float end)
    {
        float t = 0;
        float duration = 0.5f;

        while (t < duration)
        {
            float a = Mathf.Lerp(start, end, t / duration);
            cutsceneImage.color = new Color(1, 1, 1, a);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        cutsceneImage.color = new Color(1, 1, 1, end);
    }
}