using UnityEngine;

public class SpaceScoreController : MonoBehaviour
{
    private float timer = 0f;
    private float interval = 180f; // 3분

    private bool isActive = true;

    public float CurrentTime => timer;
    public float RemainingTime => interval - timer;

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;

            Episode2ScoreManager.Instance?.ReduceSpaceScore();
            Debug.Log("Space 점수 -1");
        }
    }

    public void StopTimer()
    {
        isActive = false;
    }
}