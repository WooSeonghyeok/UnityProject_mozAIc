using UnityEngine;

public class PaintBucket : MonoBehaviour
{
    public ColorType colorType;

    [Header("Sound")]
    public AudioClip audioClip;
    private AudioSource audioSource;

    [Header("Start Time")]
    public float startTime = 0f;

    [Header("Play Duration")]
    public float playDuration = 1f; // 🔥 몇 초 재생할지

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerColor player = other.GetComponent<PlayerColor>();

            if (player != null)
            {
                player.AddColor(colorType);

                if (audioClip != null)
                {
                    audioSource.clip = audioClip;

                    audioSource.time = Mathf.Clamp(startTime, 0f, audioClip.length);

                    audioSource.Play();

                    // 🔥 일정 시간 후 정지
                    CancelInvoke(); // 중복 방지
                    Invoke(nameof(StopSound), playDuration);
                }
            }
        }
    }

    void StopSound()
    {
        audioSource.Stop();
    }
}