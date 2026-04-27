using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;

public class PaintingPortal : MonoBehaviour
{
    public enum PortalReturnType { None, Space, Paint }
    public enum PortalUsageType { Entry, Exit } // ⭐ 추가

    public PortalReturnType returnType;
    public PortalUsageType usageType; // ⭐ 추가

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
        mat = rend.material;

        mat.SetFloat("_WaveStrength", idleStrength);

        // ⭐ Entry 포탈만 막기
        if (usageType == PortalUsageType.Entry && !CanEnterPortal())
        {
            DisablePortal();
        }
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

        // ⭐ 클리어 데이터 저장 (Entry 포탈일 때만)
        if (usageType == PortalUsageType.Entry &&
            SaveManager.instance != null &&
            SaveManager.instance.curData != null)
        {
            if (returnType == PortalReturnType.Space)
            {
                SaveManager.instance.curData.ep2_spaceClear = true;
                Debug.Log("SpacePuzzle 클리어 저장됨");
            }
            else if (returnType == PortalReturnType.Paint)
            {
                SaveManager.instance.curData.ep2_paintClear = true;
                Debug.Log("PaintPuzzle 클리어 저장됨");
            }

            SaveManager.instance.WriteCurJSON();
        }

        // ⭐ 스폰 위치 설정 (기존 유지)
        if (EP2_PuzzleManager.Instance != null)
        {
            if (returnType == PortalReturnType.Space)
                EP2_PuzzleManager.Instance.spawnType = "Space";
            else if (returnType == PortalReturnType.Paint)
                EP2_PuzzleManager.Instance.spawnType = "Paint";
            else
                EP2_PuzzleManager.Instance.spawnType = "Default";
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

    // ⭐ 입장 가능 여부
    private bool CanEnterPortal()
    {
        // Exit 포탈은 항상 허용
        if (usageType == PortalUsageType.Exit)
            return true;

        if (SaveManager.instance == null || SaveManager.instance.curData == null)
            return true;

        if (returnType == PortalReturnType.Space &&
            SaveManager.instance.curData.ep2_spaceClear)
            return false;

        if (returnType == PortalReturnType.Paint &&
            SaveManager.instance.curData.ep2_paintClear)
            return false;

        return true;
    }

    // ⭐ 포탈 비활성화
    private void DisablePortal()
    {
        GetComponent<Collider>().enabled = false;
        mat.SetFloat("_WaveStrength", 0f);

        Debug.Log("이미 클리어된 Entry 포탈 → 비활성화");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!CanEnterPortal())
            {
                Debug.Log("이미 클리어해서 입장 불가");
                return;
            }

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
            if (!CanEnterPortal()) return;

            Vector3 hitPos = other.transform.position;

            Vector3 localPos = transform.InverseTransformPoint(hitPos);
            Vector3 scale = transform.localScale;

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