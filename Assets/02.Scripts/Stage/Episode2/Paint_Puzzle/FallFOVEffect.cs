using UnityEngine;
using Cinemachine;

public class FallFOVEffect : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;

    public float normalFOV = 40f;
    public float fallFOV = 85f;

    public float duration = 2f; // 변화 시간

    private float t = 0f;
    private bool isFalling = false;

    void Update()
    {
        if (vcam == null) return;

        if (isFalling)
        {
            t += Time.deltaTime / duration;
        }
        else
        {
            t -= Time.deltaTime / duration;
        }

        t = Mathf.Clamp01(t);

        // ⭐ 핵심: 가속 느낌
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        vcam.m_Lens.FieldOfView = Mathf.Lerp(normalFOV, fallFOV, smoothT);
    }

    public void StartFall()
    {
        isFalling = true;
    }

    public void StopFall()
    {
        isFalling = false;
    }
}