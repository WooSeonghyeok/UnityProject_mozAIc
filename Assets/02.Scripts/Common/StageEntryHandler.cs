using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StageEntryHandler : MonoBehaviour
{
    public static StageEntryHandler instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int selectedStage = StageSelectionData.SelectedStage;
        int selectedCP = StageSelectionData.SelectedCP;
        string selectedName = $"Stage{selectedStage + 1}";
        if (scene.name != selectedName) return;
        StartCoroutine(StartSpawn(selectedStage, selectedCP));
    }
    private IEnumerator StartSpawn(int selectedStage, int selectedCP)
    {
        yield return null;
        /* Checkpoint_Plane 찾기 */
        var allPlanes = FindObjectsOfType<Checkpoint_Plane>();
        if (allPlanes == null || allPlanes.Length == 0) yield break;
        var target = allPlanes.FirstOrDefault(p => p.cpNum == selectedCP);
        if (target == null) yield break;
        /* 플레이어 찾기 (Player 태그 필요) */
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null) yield break;
        /* 위치, 회전 설정 (spawnPos가 Null인지 체크) */
        if (target.spawnPos != null)
        {
            var rb = playerGO.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;          // 기존 속도 초기화
                rb.angularVelocity = Vector3.zero;   // 회전 관성 초기화
                rb.position = target.spawnPos.position;  // transform 대신 rb.position 사용
            }
            else
            {
                playerGO.transform.position = target.spawnPos.position;
            }
            var movement = playerGO.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SetLookRotation(target.spawnPos.rotation);
            }
            playerGO.transform.rotation = target.spawnPos.rotation;
        }
        else
        {
            Debug.LogWarning("StageEntryHandler: target.spawnPos is null.");
        }
    }
}