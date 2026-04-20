using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Episode3_Scene 로드 시 로비 인트로 컷씬 컨트롤러를 자동으로 생성한다.
/// 씬 YAML을 크게 수정하지 않고 런타임에서 컷씬 시스템을 붙이기 위한 부트스트랩이다.
/// </summary>
public static class Ep3LobbyIntroCutsceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != Ep3LobbyIntroCutsceneController.SceneName)
        {
            return;
        }

        if (Object.FindObjectOfType<Ep3LobbyIntroCutsceneController>() != null)
        {
            return;
        }

        GameObject host = new GameObject("EP3 Lobby Intro Cutscene Controller");
        Ep3LobbyIntroCutsceneController controller = host.AddComponent<Ep3LobbyIntroCutsceneController>();
        controller.InitializeAsRuntimeFallback();
    }
}
