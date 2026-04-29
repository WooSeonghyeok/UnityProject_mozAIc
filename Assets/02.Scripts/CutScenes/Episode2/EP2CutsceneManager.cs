using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EP2CutsceneManager : CutsceneManager
{
    public static EP2CutsceneManager Instance;
    private Dictionary<string, Sprite[]> cutsceneDict;
    private void Reset()
    {
        fadeDuration = 0.5f;
        holdDuration = 2f;
        useCutsceneMode = true;
        keepCutsceneMode = false;
    }
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
    public override IEnumerator PlayCutsceneAndWait(string name)
    {
        return base.PlayCutsceneAndWait(name);
    }
    protected override IEnumerator Fade(float start, float end)
    {
        return base.Fade(start, end);
    }
}