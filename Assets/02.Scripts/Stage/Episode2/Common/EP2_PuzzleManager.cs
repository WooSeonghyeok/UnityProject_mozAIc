using UnityEngine;
using UnityEngine.SceneManagement;

public class EP2_PuzzleManager : MonoBehaviour
{
    public static EP2_PuzzleManager Instance;

    // 🔥 퍼즐 클리어 상태
    public bool spaceClear = false;
    public bool paintClear = false;

    // 🔥 가구 연출 여부
    public bool spaceFurnitureSpawned = false;
    public bool paintFurnitureSpawned = false;

    // 🔥 스폰 위치 구분
    public string spawnType = "Default";  // "Default", "Space", "Paint"

    [Header("Portal Materials")]
    public Material spaceIncompleteMat;
    public Material spaceCompleteMat;
    public Material paintIncompleteMat;
    public Material paintCompleteMat;

    private MeshRenderer spacePortalRenderer;
    private MeshRenderer paintPortalRenderer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Episode2_Scene")
        {
            GameObject spaceObj = GameObject.Find("SpaceQuad");
            GameObject paintObj = GameObject.Find("PictureQuad");

            if (spaceObj != null)
                spacePortalRenderer = spaceObj.GetComponent<MeshRenderer>();

            if (paintObj != null)
                paintPortalRenderer = paintObj.GetComponent<MeshRenderer>();

            ApplyPortalMaterials();
        }
    }

    // 🔵 Space 퍼즐 완료
    public void SolveSpacePuzzle()
    {
        if (spaceClear) return;

        spaceClear = true;
        Debug.Log("Space 퍼즐 완료");

        // ⭐ 클리어 점수 +5
        //Episode2ScoreManager.Instance?.AddClearScore(5);

        ApplyPortalMaterials();
    }

    // 🎨 Paint 퍼즐 완료
    public void SolvePaintPuzzle()
    {
        if (paintClear) return;

        paintClear = true;
        Debug.Log("Paint 퍼즐 완료");

        //// ⭐ 클리어 점수 +5
        //Episode2ScoreManager.Instance?.AddClearScore(5);

        if (SaveManager.instance != null)
            SaveManager.instance.curData.ep2_paintClear = true;

        ApplyPortalMaterials();
    }

    // 🔥 머터리얼 적용
    void ApplyPortalMaterials()
    {
        if (spacePortalRenderer != null)
        {
            spacePortalRenderer.material =
                spaceClear ? spaceCompleteMat : spaceIncompleteMat;
        }
        else
        {
            Debug.LogWarning("SpacePortalRenderer 못 찾음!");
        }

        if (paintPortalRenderer != null)
        {
            paintPortalRenderer.material =
                paintClear ? paintCompleteMat : paintIncompleteMat;
        }
        else
        {
            Debug.LogWarning("PaintPortalRenderer 못 찾음!");
        }
    }

    // ⭐ 상태 확인용 (점수 X)
    public bool AllClear()
    {
        return spaceClear && paintClear;
    }
}