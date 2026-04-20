using System.Collections;
using UnityEngine;
public class Puzzle4_3_DoorCtrl : MonoBehaviour
{
    private bool _activated = false;
    private WaitForSeconds ws;
    private void Awake()
    {
        var ep = FindObjectOfType<Ep4_Puzzle3Manager>();
        if (ep != null) ep.onAllCollected.AddListener(OnAllCollected);
        gameObject.SetActive(true);
    }
    public void OnAllCollected()
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
