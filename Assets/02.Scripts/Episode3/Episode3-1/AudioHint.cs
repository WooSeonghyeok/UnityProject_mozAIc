using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 플레이어와 악보 조각의 거리가 가까워지면 점점 소리가 크게 나게 하는 스크립트
public class AudioHint : MonoBehaviour
{
    [Header("대상")]
    public Transform playerTr;
    private AudioSource Source;
    public AudioClip clip;
    [Header("거리")]
    public float maxDistance = 20.0f;
    public float minDistance = 1.0f;
    [Header("볼륨")]
    public float maxVolume = 1.0f;
    public float minVolume = 0.0f;
    public float volChangeSpeed = 1.5f;
    [Header("재생옵션")]
    private bool isStoping = false;
    void Awake()
    {
        Source = GetComponent<AudioSource>();
    }
    private void Start()
    {
        if (playerTr == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTr = player.transform;
            }
        }
        Source.volume = minVolume;
        if (!Source.isPlaying)
        {
            Source.Play();
        }
    }
    private void Update()
    {
        if (playerTr == null) return;

        float distance = Vector3.Distance(playerTr.position, transform.position);

        float targetVolume = 0f;

        // 최대 청취 거리 안에 있을 때만 볼륨 계산
        if (distance <= maxDistance)
        {
            // distance가 minDistance ~ maxDistance 사이에 있을 때 0 ~ 1로 정규화
            float t = 1f - Mathf.InverseLerp(minDistance, maxDistance, distance);

            // 최소 볼륨 ~ 최대 볼륨 사이로 보간
            targetVolume = Mathf.Lerp(minVolume, maxVolume, t);

            // 혹시 멀어서 정지된 상태였다면 다시 재생
            if (!Source.isPlaying)
            {
                Source.Play();
            }
        }
        else
        {
            // 범위 밖이면 0으로
            targetVolume = 0f;

            // 아예 멈추고 싶으면 정지
            if (isStoping && Source.isPlaying && Source.volume <= 0.01f)
            {
                Source.Stop();
            }
        }

        // 부드럽게 볼륨 변경
        Source.volume = Mathf.MoveTowards(
            Source.volume,
            targetVolume,
            volChangeSpeed * Time.deltaTime
        );
    }
}
