using UnityEngine;

public class CutsceneTriggerManager : MonoBehaviour
{
    public CutsceneManager cutsceneManager;
    public SaveDataObj CurData;
    private string scene;

    void Start()
    {
        scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        CurData = SaveManager.instance.curData;
        if (scene == "Episode2_Scene" && !CurData.Episode2_Visited)  // Episode2 처음 진입
        {
            CurData.Episode2_Visited = true;
            cutsceneManager.Play("Episode2_Intro");
        }
        if (scene == "Space_Puzzle" && !CurData.Space_Visited)  // Space 퍼즐 처음
        {
            CurData.Space_Visited = true;
            cutsceneManager.Play("Space_Intro");
        }  
        if (scene == "Paint_Puzzle" && !CurData.Paint_Visited)  // Paint 퍼즐 처음
        {
            CurData.Paint_Visited = true;
            cutsceneManager.Play("Paint_Intro");
        }
        if (scene == "Episode2_Scene")  // Episode2 복귀 컷씬
        {
            if (CurData.ep2_spaceClear && !CurData.Played_Space_Clear)
            {
                CurData.Played_Space_Clear = true;
                cutsceneManager.Play("Space_Clear");
            }

            if (CurData.ep2_paintClear && !CurData.Played_Paint_Clear)
            {
                CurData.Played_Paint_Clear = true;
                cutsceneManager.Play("Paint_Clear");
            }
        }
        SaveManager.WriteCurJSON(CurData);
    }

    void Update()
    {
        // ⭐ Space 퍼즐 클리어 즉시 컷씬
        if (scene == "Space_Puzzle")
        {
            if (CurData.ep2_spaceClear && !CurData.Played_Space_Clear_Immediate)
            {
                CurData.Played_Space_Clear_Immediate = true;
                SaveManager.WriteCurJSON(CurData);
                cutsceneManager.Play("Space_Clear_Immediate");
            }
        }

        // ⭐ Paint 퍼즐 클리어 즉시 컷씬
        if (scene == "Paint_Puzzle")
        {
            if (CurData.ep2_paintClear && !CurData.Played_Paint_Sequences)
            {
                CurData.Played_Paint_Sequences = true;
                SaveManager.WriteCurJSON(CurData);
                cutsceneManager.Play("Paint_Clear_Immediate");
            }
        }
    }
}