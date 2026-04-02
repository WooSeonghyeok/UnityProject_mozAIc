using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;

public class PaintingPortal : MonoBehaviour
{
    public enum PortalReturnType { None, Space, Paint }
    public PortalReturnType returnType;

    public string nextScene;
    public float stayTime = 1f;

    public Transform cameraTarget;

    private float timer = 0f;
    private bool isInside = false;
    private bool isTriggered = false;

    public CinemachineVirtualCamera portalCam;
    public CinemachineVirtualCamera playerCamVCam;
    public MonoBehaviour playerController;

    Renderer rend;
    Material mat;

    float idleStrength = 0.01f;
    float hitStrength = 0.05f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material; // 🔥 인스턴스 material

        mat.SetFloat("_WaveStrength", idleStrength);
    }

    void Update()
    {
        if (isInside && !isTriggered)
        {
            timer += Time.deltaTime;

            if (timer >= stayTime)
            {
                Debug.Log("포탈 시작");
                StartCoroutine(PortalSequence());
                isTriggered = true;
            }
        }
    }

    IEnumerator PortalSequence()
    {
        if (portalCam == null || playerCamVCam == null)
        {
            Debug.LogError("카메라 연결 안됨!");
            yield break;
        }

        if (playerController != null)
            playerController.enabled = false;

        portalCam.transform.position = cameraTarget.position;
        portalCam.transform.rotation = cameraTarget.rotation;

        portalCam.Priority = 20;
        playerCamVCam.Priority = 10;

        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeToWhite());

        if (PuzzleManager.Instance != null)
        {
            if (returnType == PortalReturnType.Space)
                PuzzleManager.Instance.spawnType = "Space";
            else if (returnType == PortalReturnType.Paint)
                PuzzleManager.Instance.spawnType = "Paint";
            else
                PuzzleManager.Instance.spawnType = "Default";
        }

        SceneManager.LoadScene(nextScene);
    }

    IEnumerator FadeToWhite()
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime;
            FadeManager.Instance.SetAlpha(t);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInside = true;
            timer = 0f;

            mat.SetFloat("_WaveStrength", hitStrength);
            mat.SetFloat("_ImpactTime", Time.time);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 hitPos = other.transform.position;

            Vector3 localPos = transform.InverseTransformPoint(hitPos);
            Vector3 scale = transform.localScale;

            // 🔥 핵심 수정
            float u = (localPos.x / scale.x) + 0.5f;
            float v = (localPos.z / scale.z) + 0.5f;

            Vector2 uv = new Vector2(u, v);

            mat.SetVector("_WaveCenter", uv);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInside = false;
            timer = 0f;

            mat.SetFloat("_WaveStrength", idleStrength);
        }
    }
}