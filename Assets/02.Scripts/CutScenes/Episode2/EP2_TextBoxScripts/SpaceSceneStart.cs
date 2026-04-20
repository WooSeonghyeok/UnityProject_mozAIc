using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceSceneStart : MonoBehaviour
{
    public float delay = 4f; // ÀÌ¹ÌÁö ÄÆ¾À ±æÀÌ ¸ÂÃç¼­

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        yield return new WaitForSecondsRealtime(delay);

        var ctrl = FindObjectOfType<TextboxCtrl_Ep2>();

        if (ctrl != null)
        {
            yield return StartCoroutine(ctrl.SpacePuzzleStart());
        }
    }
}
