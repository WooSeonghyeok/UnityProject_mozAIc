using UnityEngine;

public class PerspectivePuzzle_EP4 : MonoBehaviour
{
    public Transform player;
    public Transform viewPoint;
    public Transform lookTarget;
    public float positionThreshold = 1.0f;
    public float angleThreshold = 15f;
    public float requiredTime = 5f;
    public float effectDelay = 4f; // 🔥 이펙트 실행 시간
    public GameObject completeObject;
    public GameObject[] pieces;

    float timer = 0f;
    bool isSolved = false;
    bool isActivating = false;
    bool hasPlayedEffect = false;

    Renderer rend;
    Vector3[] originalPositions;

    public ParticleSystem centerParticle;

    // 🔥 사운드 추가
    [Header("Sound")]
    public AudioClip holdSound;
    private AudioSource audioSource;

    void Start()
    {
        rend = GetComponent<Renderer>();

        // 🔥 AudioSource 자동 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D 사운드 (원하면 0으로 바꾸면 2D)

        originalPositions = new Vector3[pieces.Length];
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] != null)
                originalPositions[i] = pieces[i].transform.localPosition;
        }

        if (centerParticle != null)
        {
            var emission = centerParticle.emission;
            emission.rateOverTime = 0;
            centerParticle.Stop();
        }
    }

    void Update()
    {
        if (isSolved) return;

        Transform cam = Camera.main.transform;

        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
        Vector3 viewFlat = new Vector3(viewPoint.position.x, 0, viewPoint.position.z);

        float playerDist = Vector3.Distance(playerFlat, viewFlat);

        Vector3 camForward = cam.forward;
        camForward.y = 0;

        Vector3 targetForward = lookTarget.forward;
        targetForward.y = 0;

        float angle = Vector3.Angle(camForward, targetForward);

        UpdateColor(playerDist, angle);

        if (playerDist < positionThreshold && angle < angleThreshold)
        {
            if (!isActivating)
            {
                isActivating = true;
            }

            // 🔥 사운드 재생 (조건 만족 동안만)
            if (holdSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = holdSound;
                audioSource.Play();
            }

            timer += Time.deltaTime;

            float strength = GetStrength(playerDist, angle);

            ApplyShake(strength);
            UpdateParticle(timer);

            // 🔥 일정 시간 후 이펙트
            if (!hasPlayedEffect && timer >= effectDelay)
            {
                hasPlayedEffect = true;

                if (centerParticle != null)
                {
                    centerParticle.Play();
                }
            }

            if (timer >= requiredTime)
            {
                SolvePuzzle();
                isSolved = true;

                // 🔥 퍼즐 완료 시 사운드 정지
                if (audioSource.isPlaying)
                    audioSource.Stop();
            }
        }
        else
        {
            timer = 0f;
            hasPlayedEffect = false;

            // 🔥 조건 벗어나면 사운드 정지
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (isActivating)
            {
                ResetPositions();
                ResetParticle();
                isActivating = false;
            }
        }
    }

    void UpdateParticle(float currentTime)
    {
        if (centerParticle == null) return;

        float progress = Mathf.Clamp01(currentTime / requiredTime);

        var emission = centerParticle.emission;
        emission.rateOverTime = Mathf.Lerp(10f, 80f, progress);
    }

    void UpdateColor(float dist, float angle)
    {
        if (rend == null) return;

        if (dist > positionThreshold)
            rend.material.color = Color.red;
        else if (angle > angleThreshold)
            rend.material.color = Color.yellow;
        else
            rend.material.color = Color.blue;
    }

    float GetStrength(float dist, float angle)
    {
        float distFactor = Mathf.Clamp01(1 - (dist / positionThreshold));
        float angleFactor = Mathf.Clamp01(1 - (angle / angleThreshold));
        return distFactor * angleFactor;
    }

    void ApplyShake(float strength)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;

            float shake = strength * 0.015f;

            pieces[i].transform.localPosition = originalPositions[i] +
                new Vector3(
                    Mathf.Sin(Time.time * 10f + i) * shake,
                    Mathf.Cos(Time.time * 8f + i) * shake,
                    Mathf.Sin(Time.time * 6f + i) * shake * 0.3f
                );
        }
    }

    void ResetPositions()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;
            pieces[i].transform.localPosition = originalPositions[i];
        }
    }

    void ResetParticle()
    {
        if (centerParticle == null) return;

        var emission = centerParticle.emission;
        emission.rateOverTime = 0;

        centerParticle.Stop();
    }

    void SolvePuzzle()
    {
        Debug.Log("퍼즐 성공!");

        if (rend != null)
            rend.material.color = Color.green;

        foreach (GameObject piece in pieces)
        {
            piece.SetActive(false);
        }

        if (completeObject != null)
        {
            completeObject.SetActive(true);
        }
    }

    void OnDrawGizmos()
    {
        if (viewPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(viewPoint.position, positionThreshold);
        }

        if (lookTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(lookTarget.position, lookTarget.forward * 2f);
        }
    }
}