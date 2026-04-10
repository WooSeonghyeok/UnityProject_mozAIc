using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CutsceneManager : MonoBehaviour
{
    public Image cutsceneImage;

    public List<CutsceneData> cutscenes;

    Dictionary<string, Sprite[]> cutsceneDict;

    void Awake()
    {
        cutsceneDict = new Dictionary<string, Sprite[]>();

        foreach (var c in cutscenes)
        {
            cutsceneDict.Add(c.name, c.images);
        }
    }

    public void Play(string name)
    {
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
            yield return new WaitForSeconds(2f);
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
            t += Time.deltaTime;
            yield return null;
        }

        cutsceneImage.color = new Color(1, 1, 1, end);
    }
}

[System.Serializable]
public class CutsceneData
{
    public string name;
    public Sprite[] images;
}