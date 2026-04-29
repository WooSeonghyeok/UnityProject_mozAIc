using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EP1CutsceneManager : CutsceneManager
{
    public static EP1CutsceneManager Instance;
    private Dictionary<string, Sprite[]> cutsceneDict;
    private void Reset()
    {
        fadeDuration = 0.5f;
        holdDuration = 2f;
        useCutsceneMode = false;
    }
    void Awake()
    {
        // 싱글톤
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 데이터 초기화
        cutsceneDict = new Dictionary<string, Sprite[]>();

        foreach (var c in cutscenes)
        {
            if (!cutsceneDict.ContainsKey(c.name))
            {
                cutsceneDict.Add(c.name, c.images);
            }
        }

        // 시작 시 비활성화
        if (cutsceneImage != null)
            cutsceneImage.gameObject.SetActive(false);
    }

    // 컷씬 실행
    public new void Play(string name)
    {
        if (!cutsceneDict.ContainsKey(name))
        {
            Debug.LogWarning($"컷씬 이름 없음: {name}");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PlayRoutine(cutsceneDict[name]));
    }

    IEnumerator PlayRoutine(Sprite[] scenes)
    {
        cutsceneImage.gameObject.SetActive(true);

        //// ⭐ 컷씬 시작 → 게임 멈춤
        //GameManager.Instance.CutsceneMode(true);

        foreach (var scene in scenes)
        {
            cutsceneImage.sprite = scene;
            cutsceneImage.color = new Color(1, 1, 1, 0);

            yield return Fade(0, 1);
            yield return new WaitForSecondsRealtime(2f);
            yield return Fade(1, 0);
        }

        cutsceneImage.gameObject.SetActive(false);

        //// ⭐ 컷씬 끝 → 다시 게임 가능
        //GameManager.Instance.CutsceneMode(false);

        OnCutsceneEnd?.Invoke();
    }

    // 컷씬 끝날 때까지 기다리기 (코루틴용)
    public override IEnumerator PlayCutsceneAndWait(string name)
    {
        return base.PlayCutsceneAndWait(name);
    }

    // 페이드
    protected override IEnumerator Fade(float start, float end)
    {
        return base.Fade(start, end);
    }
}