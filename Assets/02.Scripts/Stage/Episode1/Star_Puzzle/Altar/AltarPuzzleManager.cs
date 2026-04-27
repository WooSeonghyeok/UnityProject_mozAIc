using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // Volume 사용

public class AltarPuzzleManager : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject puzzlePanel;            // 퍼즐 패널
    public AltarPuzzleSlot[] slots;           // 8칸 슬롯
    public List<StarData> invalidStars = new List<StarData>();  // 오답용 별 데이터
    public bool showIcon = true;              // true면 아이콘, false면 색 위주 표시

    [Header("플레이어 제어")]
    public PlayerMovement playerMovement;     // 플레이어 이동 스크립트

    [Header("현재 퍼즐 상태")]
    public List<StarData> ownedStars = new List<StarData>();

    [Header("포스트 프로세싱")]
    public Volume puzzleVolume;               // Global Volume 연결
    public float fadeSpeed = 5f;
    private WaitForSeconds clearWs = new WaitForSeconds(1f);

    [Header("퍼즐 성공 시 맵 색상 변경")]
    [SerializeField] private Material sourceSharedMaterial;   // 원본 Color Material
    [SerializeField] private Texture defaultTexture;          // 기본 black&white 텍스처
    [SerializeField] private Texture gradientTexture;         // 퍼즐 성공 시 바꿀 텍스처

    [Header("스카이박스")]
    public Material defaultSkybox;   // 기본 어두운 스카이박스
    public Material clearedSkybox;   // 별이 가득한 스카이박스

    public AudioSource source;
    public AudioClip successClip;

    private int correctPressedCount;
    private bool isPuzzleActive;
    public bool isPuzzleCleared;

    // 외부에서 퍼즐이 열려 있는지 확인할 수 있도록 프로퍼티 제공
    public bool isPuzzleOpen => puzzlePanel != null && puzzlePanel.activeSelf;

    // 런타임 전용 머터리얼
    private Material runtimeMaterialInstance;

    // 바뀐 Renderer 목록 보관
    private readonly List<Renderer> cachedRenderers = new List<Renderer>();

    private void Awake()
    {
        SetupRuntimeSharedMaterial();
        // 게임 시작 시 기본 스카이박스로 초기화
        ApplySkybox(defaultSkybox);
    }

    private void OnDestroy()
    {
        // 생성한 런타임 머터리얼 정리
        if (runtimeMaterialInstance != null)
        {
            Destroy(runtimeMaterialInstance);
        }
    }
#region 게임 시작시 맵의 머터리얼을 흑백으로 초기화
    // 씬 안에서 sourceSharedMaterial을 쓰는 모든 Renderer를 자동으로 찾아
    // 런타임 전용 머터리얼로 교체
    private void SetupRuntimeSharedMaterial()
    {
        if (sourceSharedMaterial == null)
        {
            Debug.LogWarning("[AltarPuzzleManager] sourceSharedMaterial이 연결되지 않았습니다.");
            return;
        }

        if (defaultTexture == null)
        {
            Debug.LogWarning("[AltarPuzzleManager] defaultTexture가 연결되지 않았습니다.");
            return;
        }

        // 기존 복제본 제거
        if (runtimeMaterialInstance != null)
        {
            Destroy(runtimeMaterialInstance);
        }

        cachedRenderers.Clear();

        // 원본 머터리얼을 복제해서 런타임 전용 머터리얼 생성
        runtimeMaterialInstance = new Material(sourceSharedMaterial);

        // 시작 시 항상 기본 텍스처 적용
        runtimeMaterialInstance.SetTexture("_BaseMap", defaultTexture);

        // 현재 씬의 모든 Renderer 검색
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer renderer = allRenderers[i];
            if (renderer == null)
                continue;

            Material[] sharedMats = renderer.sharedMaterials;
            bool changed = false;

            for (int j = 0; j < sharedMats.Length; j++)
            {
                // 원본 Color Material을 쓰는 슬롯만 런타임 머터리얼로 교체
                if (sharedMats[j] == sourceSharedMaterial)
                {
                    sharedMats[j] = runtimeMaterialInstance;
                    changed = true;
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = sharedMats;
                cachedRenderers.Add(renderer);
            }
        }

        Debug.Log($"[AltarPuzzleManager] 런타임 머터리얼 적용 완료 - 대상 Renderer 수: {cachedRenderers.Count}");
    }
#endregion
    // 퍼즐 열기
    public void OpenPuzzle(List<StarData> playerStars)
    {
        if (isPuzzleOpen)
            return;

        ownedStars = new List<StarData>(playerStars);

        if (ownedStars.Count == 0)
        {
            Debug.LogWarning("보유한 별이 없어 퍼즐을 열 수 없습니다.");
            return;
        }

        correctPressedCount = 0;
        isPuzzleActive = true;
        isPuzzleCleared = false;

        GameManager.Instance.openPopupCnt++;
        GameManager.Instance.OnPopupChanged();
        puzzlePanel.SetActive(true);
        SetPlayerControl(false);

        GeneratePuzzle();
    }

    // 퍼즐 닫기
    public void ClosePuzzle()
    {
        puzzlePanel.SetActive(false);
        GameManager.Instance.openPopupCnt--;
        GameManager.Instance.lookLock = (GameManager.Instance.openPopupCnt > 0);
        GameManager.Instance.MouseStateChange();
        isPuzzleActive = false;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    #region 퍼즐 생성 및 입력 처리
    private void GeneratePuzzle()
    {
        List<StarData> puzzleStars = new List<StarData>();

        for (int i = 0; i < ownedStars.Count; i++)
        {
            puzzleStars.Add(ownedStars[i]);
        }

        while (puzzleStars.Count < slots.Length)
        {
            StarData dummy = invalidStars[Random.Range(0, invalidStars.Count)];

            if (ContainsStarId(puzzleStars, dummy.starId))
                continue;

            puzzleStars.Add(dummy);
        }

        Shuffle(puzzleStars);

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Init(this, i, puzzleStars[i], showIcon);
        }
    }

    public void OnSlotClicked(AltarPuzzleSlot clickedSlot)
    {
        if (!isPuzzleActive || isPuzzleCleared)
            return;

        if (HasOwnedStar(clickedSlot.slotStarData.starId))
        {
            correctPressedCount++;
            clickedSlot.SetSuccessVisual();

            if (correctPressedCount >= ownedStars.Count)
            {
                PuzzleSuccess();
            }
        }
        else
        {
            clickedSlot.SetFailVisual();
            PuzzleFail();
        }
    }

    private bool HasOwnedStar(string starId)
    {
        for (int i = 0; i < ownedStars.Count; i++)
        {
            if (ownedStars[i].starId == starId)
                return true;
        }

        return false;
    }

    private bool ContainsStarId(List<StarData> list, string starId)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].starId == starId)
                return true;
        }

        return false;
    }

    public void PuzzleSuccess()
    {
        isPuzzleCleared = true;
        isPuzzleActive = false;
        if (source != null && successClip != null)
        {
            source.PlayOneShot(successClip, 1f);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetInteractable(false);
        }
        Debug.Log("제단 퍼즐 성공");
        StartCoroutine(ClearSequence());
    }

    private void PuzzleFail()
    {
        isPuzzleActive = false;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetInteractable(false);
        }

        Debug.Log("제단 퍼즐 실패");
        // UI 퍼즐 실패 1회 누적
        if (GameManager_Ep1.Instance != null)
        {
            GameManager_Ep1.Instance.AddUiPuzzleFail();
        }
        Invoke(nameof(ResetPuzzle), 1f);
    }

    private void ResetPuzzle()
    {
        correctPressedCount = 0;
        isPuzzleActive = true;
        isPuzzleCleared = false;

        GeneratePuzzle();
    }

    private void SetPlayerControl(bool enable)
    {
        if (playerMovement != null)
            playerMovement.enabled = enable;

        if (enable)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    #endregion

    // 스카이박스 적용 함수
    private void ApplySkybox(Material skyboxMat)
    {
        if (skyboxMat == null)
            return;

        // 현재 씬의 스카이박스를 교체
        RenderSettings.skybox = skyboxMat;

        // 환경광 / 반사 갱신
        DynamicGI.UpdateEnvironment();
    }

    // 런타임 복제 머터리얼의 Base Map을 Gradient로 변경
    private void ApplyGradientTexture()
    {
        if (runtimeMaterialInstance == null)
        {
            Debug.LogWarning("[AltarPuzzleManager] runtimeMaterialInstance가 없습니다.");
            return;
        }

        if (gradientTexture == null)
        {
            Debug.LogWarning("[AltarPuzzleManager] gradientTexture가 연결되지 않았습니다.");
            return;
        }

        runtimeMaterialInstance.SetTexture("_BaseMap", gradientTexture);
        Debug.Log("[AltarPuzzleManager] 런타임 머터리얼의 Base Map을 Gradient로 변경했습니다.");
    }

    private IEnumerator ClearSequence()
    {
        if (puzzleVolume != null)
        {
            while (puzzleVolume.weight > 0f)
            {
                puzzleVolume.weight = Mathf.MoveTowards(
                    puzzleVolume.weight,
                    0f,
                    fadeSpeed * Time.deltaTime
                );
                yield return null;
            }
            puzzleVolume.weight = 0f;
        }
        ApplyGradientTexture();  // 퍼즐 클리어 순간 맵의 머터리얼을 그라디언트 텍스처로 교체
        ApplySkybox(clearedSkybox);  // 퍼즐 클리어 순간 스카이박스를 별 하늘로 교체
        if (GameManager_Ep1.Instance != null)
        {
            GameManager_Ep1.Instance.OnPuzzleCleared();
        }
        yield return clearWs;
        ClosePuzzle();
    }
}
