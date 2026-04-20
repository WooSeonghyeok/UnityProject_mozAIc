using UnityEngine;

public class RandomNPCAnimation : MonoBehaviour
{
    private Animator animator;

    [Header("Animation Settings")]
    public string[] animationTriggers; // 트리거 이름들

    public float minDelay = 3f; // 최소 대기 시간
    public float maxDelay = 7f; // 최대 대기 시간

    private float timer;
    private float nextTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetNextTime();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextTime)
        {
            PlayRandomAnimation();
            timer = 0f;
            SetNextTime();
        }
    }

    void PlayRandomAnimation()
    {
        if (animationTriggers.Length == 0 || animator == null) return;

        int rand = Random.Range(0, animationTriggers.Length);
        string triggerName = animationTriggers[rand];

        animator.SetTrigger(triggerName);
    }

    void SetNextTime()
    {
        nextTime = Random.Range(minDelay, maxDelay);
    }
}