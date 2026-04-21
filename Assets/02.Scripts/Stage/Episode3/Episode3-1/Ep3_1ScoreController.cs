using System.Collections;
using UnityEngine;

public class Ep3_1ScoreController : MonoBehaviour
{
    private float initialDelay = 180f; // 3분 대기
    private float interval = 60f; // 3분 경과 후 1분마다 감점  
    private bool isActive = true;
    public void StartTiming()
    {
        StartCoroutine(ReduceScore());
    }
    IEnumerator ReduceScore()
    {
        yield return new WaitForSeconds(initialDelay);
        while (isActive)
        {
            yield return new WaitForSeconds(interval);
            Ep_3Manager.Instance.Ep3_1puzzleLoss++;  // 3-1 퍼즐 점수 감점 누적값 증가
            Debug.Log("Space 점수 -1");
        }
    }
    public void TimerSwitch(bool b)
    {
        isActive = b;
    }
}