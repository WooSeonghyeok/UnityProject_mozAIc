using UnityEngine;

public class PerspectivePuzzle : MonoBehaviour
{
    [Header("Player & View")]
    public Transform player;
    public Transform viewPoint;
    public Transform lookTarget;

    [Header("Condition")]
    public float positionThreshold = 1.0f;
    public float angleThreshold = 15f;
    public float requiredTime = 5f;
    public float effectDelay = 4f;

    [Header("Objects")]
    public GameObject completeObject;
    public GameObject[] pieces;

    [Header("Effects")]
    public ParticleSystem centerParticle;

    [Header("Sound")]
    public AudioClip holdSound;

    private float timer = 0f;
    private bool isSolved = false;
    private bool isActivating = false;
    private bool hasPlayedEffect = false;

    private Renderer rend;
    private Vector3[] originalPositions;
    private AudioSource audioSource;

    // ⭐ Solve 중복 방지 핵심
    private bool hasTriggeredSolve = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        originalPositions = new Vector3[pieces.Length];
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] != null)  originalPositions[i] = pieces[i].transform.localPosition;
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
                isActivating = true;

            // 사운드
            if (holdSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = holdSound;
                audioSource.Play();
            }

            timer += Time.deltaTime;
            float strength = GetStrength(playerDist, angle);
            ApplyShake(strength);
            UpdateParticle(timer);

            // 중간 이펙트
            if (!hasPlayedEffect && timer >= effectDelay)
            {
                hasPlayedEffect = true;

                if (centerParticle != null)
                    centerParticle.Play();
            }

            // ⭐ 퍼즐 성공 (한 번만 실행되게)
            if (timer >= requiredTime && !hasTriggeredSolve)
            {
                hasTriggeredSolve = true;
                SolvePuzzle();
            }
        }
        else
        {
            timer = 0f;
            hasPlayedEffect = false;

            if (audioSource.isPlaying)
                audioSource.Stop();

            if (isActivating)
            {
                ResetPositions();
                ResetParticle();
                isActivating = false;
            }
        }
    }

    void SolvePuzzle()
    {
        Debug.Log("퍼즐 성공!");

        isSolved = true; // ⭐ 먼저 막아버림

        // ⭐ FlowManager 호출 (딱 1번)
        EP2_SpacePuzzleFlowManager.Instance?.OnPuzzleSolved();

        if (rend != null)
            rend.material.color = Color.green;

        foreach (GameObject piece in pieces)
        {
            if (piece != null)
                piece.SetActive(false);
        }

        if (completeObject != null)
        {
            completeObject.SetActive(true);
        }

        if (EP2_PuzzleManager.Instance != null)
        {
            EP2_PuzzleManager.Instance.spaceClear = true;
            EP2_PuzzleManager.Instance.SolveSpacePuzzle();
        }

        if (audioSource.isPlaying)
            audioSource.Stop();
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
        if (dist > positionThreshold) rend.material.color = Color.red;
        else if (angle > angleThreshold) rend.material.color = Color.yellow;
        else  rend.material.color = Color.blue;
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