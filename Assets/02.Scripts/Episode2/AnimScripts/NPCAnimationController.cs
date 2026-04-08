using UnityEngine;

public class NPCAnimationController : MonoBehaviour
{
    private Animator anim;
    private ChatNPC chatNpc;
    private Transform playerTr;

    private float timer;
    private float lookWeight = 0f;

    [Header("Random Animation")]
    public float minTime = 3f;
    public float maxTime = 7f;

    [Header("IK Settings")]
    public float lookSpeed = 5f;
    public float maxLookAngle = 90f;
    public float eyeHeight = 1.6f;

    void Start()
    {
        anim = GetComponent<Animator>();
        chatNpc = GetComponent<ChatNPC>();

        // 플레이어 자동 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTr = player.transform;
        }

        ResetTimer();
    }

    void Update()
    {
        bool isTalking = ChatNPCManager.instance != null && ChatNPCManager.instance.isTalking;

        // Talking 상태 적용
        anim.SetBool("isTalking", isTalking);

        // Talking 중이면 랜덤 애니메이션 중단
        if (isTalking) return;

        // 랜덤 애니메이션
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            anim.SetTrigger("doPoint");
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(minTime, maxTime);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (anim == null || playerTr == null) return;

        bool isTalking = ChatNPCManager.instance != null && ChatNPCManager.instance.isTalking;

        float targetWeight = isTalking ? 1f : 0f;
        lookWeight = Mathf.Lerp(lookWeight, targetWeight, Time.deltaTime * lookSpeed);

        if (lookWeight > 0.01f)
        {
            Vector3 dir = playerTr.position - transform.position;

            // 👉 좌/우 판별 (-180 ~ 180)
            float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);

            // 👉 각도 제한 (좌우 90도)
            angle = Mathf.Clamp(angle, -maxLookAngle, maxLookAngle);

            // 🔥 핵심: 오른쪽일 때 IK 약하게
            float weightMultiplier = (angle > 0) ? 0.5f : 1f;

            float finalWeight = lookWeight * weightMultiplier;

            // 👉 IK 강도 적용 (부드럽게)
            anim.SetLookAtWeight(finalWeight, 0.2f, 0.6f, 1.0f, 0.6f);

            // 👉 제한된 방향으로 바라보기
            Vector3 clampedDir = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 lookPos = transform.position + clampedDir * 5f + Vector3.up * eyeHeight;

            anim.SetLookAtPosition(lookPos);
        }
        else
        {
            anim.SetLookAtWeight(0f);
        }
    }
}