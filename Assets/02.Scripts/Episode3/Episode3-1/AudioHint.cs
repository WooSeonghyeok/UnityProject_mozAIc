using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioHint : MonoBehaviour
{
    [Header("대상")]
    [SerializeField] private Transform playerTr;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;

    [Header("거리")]
    [SerializeField] private float maxDistance = 18.0f;
    [SerializeField] private float minDistance = 2.0f;

    [Header("볼륨")]
    [SerializeField] private float maxVolume = 0.85f;
    [SerializeField] private float minVolume = 0.0f;
    [SerializeField] private float volChangeSpeed = 1.5f;

    [Header("재생 옵션")]
    [SerializeField] private bool stopWhenFar = false;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool randomStartTime = true;

    [Header("3D 오디오")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField, Range(0f, 360f)] private float spread = 0f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    [SerializeField] private float dopplerLevel = 0f;

    private bool hasInitializedPlaybackPosition = false;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        ApplyAudioSourceSettings();
    }

    private void Start()
    {
        ResolvePlayerIfNeeded();
        if (source == null)
        {
            return;
        }

        source.volume = minVolume;

        if (playOnStart && clip != null)
        {
            StartHintPlaybackIfNeeded();
        }
    }

    private void Update()
    {
        ResolvePlayerIfNeeded();
        if (playerTr == null || source == null || clip == null)
        {
            return;
        }

        float distance = Vector3.Distance(playerTr.position, transform.position);
        float targetVolume = 0f;

        if (distance <= maxDistance)
        {
            float t = 1f - Mathf.InverseLerp(minDistance, maxDistance, distance);
            targetVolume = Mathf.Lerp(minVolume, maxVolume, t);

            if (!source.isPlaying)
            {
                StartHintPlaybackIfNeeded();
            }
        }
        else if (stopWhenFar && source.isPlaying && source.volume <= 0.01f)
        {
            source.Stop();
        }

        source.volume = Mathf.MoveTowards(
            source.volume,
            targetVolume,
            volChangeSpeed * Time.deltaTime
        );
    }

    public void Configure(
        AudioClip hintClip,
        Transform targetPlayer,
        float nearDistance,
        float farDistance,
        float quietVolume,
        float loudVolume,
        float changeSpeed,
        bool shouldStopWhenFar,
        bool shouldLoop,
        bool shouldPlayOnStart,
        bool shouldRandomStartTime,
        float sourceSpatialBlend,
        float sourceSpread,
        AudioRolloffMode sourceRolloffMode,
        float sourceDopplerLevel)
    {
        clip = hintClip;
        playerTr = targetPlayer;
        minDistance = Mathf.Max(0.01f, nearDistance);
        maxDistance = Mathf.Max(minDistance, farDistance);
        minVolume = Mathf.Clamp01(quietVolume);
        maxVolume = Mathf.Clamp01(loudVolume);
        volChangeSpeed = Mathf.Max(0f, changeSpeed);
        stopWhenFar = shouldStopWhenFar;
        loop = shouldLoop;
        playOnStart = shouldPlayOnStart;
        randomStartTime = shouldRandomStartTime;
        spatialBlend = Mathf.Clamp01(sourceSpatialBlend);
        spread = Mathf.Clamp(sourceSpread, 0f, 360f);
        rolloffMode = sourceRolloffMode;
        dopplerLevel = Mathf.Max(0f, sourceDopplerLevel);

        ApplyAudioSourceSettings();
    }

    private void ResolvePlayerIfNeeded()
    {
        if (playerTr != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTr = player.transform;
        }
    }

    private void ApplyAudioSourceSettings()
    {
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }

        if (source == null)
        {
            return;
        }

        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = spatialBlend;
        source.spread = spread;
        source.rolloffMode = rolloffMode;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.dopplerLevel = dopplerLevel;
    }

    private void StartHintPlaybackIfNeeded()
    {
        if (source == null || clip == null)
        {
            return;
        }

        source.clip = clip;

        if (randomStartTime && !hasInitializedPlaybackPosition && clip.length > 0.05f)
        {
            source.time = Random.Range(0f, clip.length);
            hasInitializedPlaybackPosition = true;
        }

        source.Play();
    }
}
