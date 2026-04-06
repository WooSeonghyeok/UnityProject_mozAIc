using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Puzzle4_3_DoorCtrl : MonoBehaviour
{
    private bool _activated = false;
    private WaitForSeconds ws;
    private void Awake()
    {
        var ep = FindObjectOfType<Ep4_Puzzle3Manager>();
        if (ep != null)
        {
            ep.onAllPiecesCollected.AddListener(OnAllPiecesCollected);
        }
        gameObject.SetActive(true);
    }
    public void OnAllPiecesCollected()
    {
        if (_activated) return;
        StartCoroutine(DoCompletionSequence());
    }
    private IEnumerator DoCompletionSequence()
    {
        _activated = true;
        ws = new WaitForSeconds(1.0f);
        yield return ws;
        gameObject.SetActive(false);
    }
}
