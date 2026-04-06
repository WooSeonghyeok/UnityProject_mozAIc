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
    // 🔥 스폰 위치 구분 (추가 ⭐⭐⭐)
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
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)  // 🔥 씬 들어올 때 실행
    {
        if (scene.name == "Episode2")
        {
            GameObject spaceObj = GameObject.Find("SpaceQuad");
            GameObject paintObj = GameObject.Find("PictureQuad");
            if (spaceObj != null)  spacePortalRenderer = spaceObj.GetComponent<MeshRenderer>();
            if (paintObj != null)  paintPortalRenderer = paintObj.GetComponent<MeshRenderer>();
            ApplyPortalMaterials();
        }
    }
    public void SolveSpacePuzzle()  // 🔵 Space 퍼즐 완료
    {
        if (spaceClear) return;
        spaceClear = true;
        Debug.Log("Space 퍼즐 완료");
        ApplyPortalMaterials();
    }
    public void SolvePaintPuzzle()  // 🎨 Paint 퍼즐 완료
    {
        if (paintClear) return;
        paintClear = true;
        Debug.Log("Paint 퍼즐 완료");
        if (SaveManager.instance != null)  SaveManager.instance.curData.ep2_paintClear = true;
        ApplyPortalMaterials();
    }
    void ApplyPortalMaterials()  // 🔥 머터리얼 적용
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
    public bool AllClear()  // ⭐ 전체 클리어 체크
    {
        return spaceClear && paintClear;
    }
}