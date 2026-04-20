using UnityEngine;
using System.Collections;

public class FinalObjectEffect : MonoBehaviour
{
    public Transform target;
    public GameObject finalObject;

    public float duration = 1.2f;

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;

    void OnEnable()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        startScale = transform.localScale;

        StartCoroutine(MoveEffect());
    }

    IEnumerator MoveEffect()
    {
        float time = 0f;

        Vector3 endPos = target.position;

        // 🔥 원하는 최종 회전값
        Quaternion endRot = Quaternion.Euler(0f, 90f, 0f);

        while (time < duration)
        {
            float t = time / duration;

            t = t * t;

            transform.position = Vector3.Lerp(startPos, endPos, t);

            // 🔥 회전 추가
            transform.rotation = Quaternion.Lerp(startRot, endRot, t);

            transform.localScale = Vector3.Lerp(startScale, startScale * 0.2f, t);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        OnArrive();
    }

    void OnArrive()
    {
        Debug.Log("도착!");

        gameObject.SetActive(false);

        if (finalObject != null)
        {
            finalObject.SetActive(true);
        }
    }
}