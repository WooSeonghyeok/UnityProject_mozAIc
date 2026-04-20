using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Episode3.Common
{
    /// <summary>
    /// 씬 전환 시 플레이어가 씬에 없으면 인스펙터에 연결된 프리팹을 스폰포인트에 인스턴스화합니다.
    /// 인스펙터에서 `spawnPoint`와 `playerPrefab`을 연결한 상태에서 동작합니다.
    /// </summary>
    public class EP3_PlayerSpawn : MonoBehaviour
    {
        [Header("스폰 설정")]
        [Tooltip("플레이어가 없을 때 인스펙터에서 연결한 Transform 위치에 프리팹을 스폰합니다.")]
        public Transform spawnPoint;
        [Header("플레이어 프리팹")]
        [Tooltip("씬에 플레이어가 없으면 이 프리팹을 스폰합니다.")]
        public GameObject playerPrefab;
        [Header("옵션")]
        [Tooltip("플레이어가 없을 때 자동으로 인스턴스화할지 여부")]
        public bool instantiateIfMissing = true;
        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TrySpawnIfMissing();
        }
        /// <summary>
        /// 씬에 Player 태그의 오브젝트가 없으면 inspector에 연결된 prefab을 spawnPoint에 스폰합니다.
        /// </summary>
        public void TrySpawnIfMissing()
        {
            // 이미 플레이어가 존재하면 아무 동작도 하지 않음
            var existing = GameObject.FindGameObjectWithTag("Player");
            if (existing != null) return;
            if (!instantiateIfMissing)
            {
                Debug.Log("[PlayerSpawn] 플레이어가 없지만 자동 인스턴스화 옵션이 꺼져 있습니다.");
                return;
            }
            if (playerPrefab == null)
            {
                Debug.LogWarning("[PlayerSpawn] playerPrefab이 지정되어 있지 않습니다. 인스펙터에서 할당하세요.");
                return;
            }
            if (spawnPoint == null)
            {
                Debug.LogWarning("[PlayerSpawn] spawnPoint가 지정되어 있지 않습니다. 인스펙터에서 할당하세요.");
                return;
            }
            var inst = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            if (inst != null && inst.tag != "Player")
            {
                inst.tag = "Player";
            }
            // CharacterController가 있다면 위치 보정
            var cc = inst != null ? inst.GetComponent<CharacterController>() : null;
            if (cc != null)
            {
                cc.enabled = false;
                inst.transform.position = spawnPoint.position;
                cc.enabled = true;
            }
            Debug.Log($"[PlayerSpawn] playerPrefab을 spawnPoint에 인스턴스화했습니다. ({spawnPoint.name})");
        }
    }
}