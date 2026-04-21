using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 에피소드 3 로비 매니저.
/// 
/// 역할:
/// 1. 에피소드 3 시작 진입점 역할
/// 2. 로비에서 발생하는 AI 상호작용 기록
/// 3. 3-1 씬으로 이동
/// 
/// 현재는 구조가 단순하지만,
/// 이후 로비 대사/선택지/진입 조건 분기 등을 붙일 수 있는 시작 지점이다.
/// </summary>
public class LobbyManager : MonoBehaviour
{
    [Header("다음 씬 이름")]
    [SerializeField] private string stage3_1SceneName = "Episode3_Stage3_1";

    /// <summary>
    /// 에피소드 3 시작 버튼/이벤트 진입점.
    /// 
    /// 현재는 Ep_3Manager 존재 여부만 확인하고 로그를 남기지만,
    /// 이후 초기화 루틴이나 연출 시작 로직을 붙일 수 있다.
    /// </summary>
    public void StartEpisode3()
    {
        if (Ep_3Manager.Instance == null)
        {
            Debug.LogWarning("[LobbyManager] Ep_3Manager가 없습니다.");
            return;
        }

        Debug.Log("[LobbyManager] 에피소드 3 시작");
    }

    /// <summary>
    /// 로비에서 AI와 상호작용했을 때 호출된다.
    /// 
    /// 현재는 공통 매니저에 상호작용 횟수를 기록하는 역할만 수행한다.
    /// </summary>
    public void OnTalkToAI()
    {
        if (Ep_3Manager.Instance != null)
        {
            Ep_3Manager.Instance.AddAIInteraction(1);
        }

        Debug.Log("[LobbyManager] AI와 대화");
    }

    /// <summary>
    /// 3-1 씬으로 이동한다.
    /// 로비에서 실제 스테이지 시작 버튼 역할을 한다.
    /// </summary>
    public void GoToStage3_1()
    {
        SceneManager.LoadScene(stage3_1SceneName);
    }
}