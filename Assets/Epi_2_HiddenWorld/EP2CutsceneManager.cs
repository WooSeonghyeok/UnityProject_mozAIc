using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EP2CutsceneManager : MonoBehaviour
{
    public static EP2CutsceneManager Instance;

    [Header("UI")]
    public Image cutsceneImage;

    [Header("Cutscene Data")]
    public List<CutsceneData> cutscenes; // ⭐ 기존 CutsceneData 사용

    private Dictionary<string, Sprite[]> cutsceneDict;

    void Awake()
    {
        // ⭐ 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ⭐ 딕셔너리 초기화
        cutsceneDict = new Dictionary<string, Sprite[]>();

        foreach (var c in cutscenes)
        {
            if (!cutsceneDict.ContainsKey(c.name))
            {
                cutsceneDict.Add(c.name, c.images);
            }
        }

        // ⭐ 처음엔 꺼두기
        if (cutsceneImage != null)
            cutsceneImage.gameObject.SetActive(false);
    }

    // 🎬 컷씬 실행
    public void Play(string name)
    {
        if (!cutsceneDict.ContainsKey(name))
        {
            Debug.LogWarning($"컷씬 이름 없음: {name}");
            return;
        }

        StopAllCoroutines(); // 중복 실행 방지
        StartCoroutine(PlayRoutine(cutsceneDict[name]));
    }

    IEnumerator PlayRoutine(Sprite[] scenes)
    {
        cutsceneImage.gameObject.SetActive(true);

        foreach (var scene in scenes)
        {
            cutsceneImage.sprite = scene;
            cutsceneImage.color = new Color(1, 1, 1, 0);

            yield return Fade(0, 1);
            yield return new WaitForSecondsRealtime(2f); // ⭐ TimeScale 영향 없음
            yield return Fade(1, 0);
        }

        cutsceneImage.gameObject.SetActive(false);
    }

    IEnumerator Fade(float start, float end)
    {
        float t = 0;
        float duration = 1f;

        while (t < duration)
        {
            float a = Mathf.Lerp(start, end, t / duration);
            cutsceneImage.color = new Color(1, 1, 1, a);
            t += Time.unscaledDeltaTime; // ⭐ TimeScale 무시
            yield return null;
        }

        cutsceneImage.color = new Color(1, 1, 1, end);
    }
}