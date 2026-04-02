namespace Episode3.Common
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 벽에 붙이는 심볼(상호작용 오브젝트).
    /// - 입력 처리 책임을 플레이어 인풋 스크립트에 두지 않고,
    ///   콜라이더 범위 내에 있는 `Player` 태그 오브젝트가 지정된 키를 누르면 상호작용 수행.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableSymbol : MonoBehaviour
    {
        [Header("상호작용 설정")]
        [Tooltip("체크하면 sceneName을 사용하여 SceneManager.LoadScene 호출")]
        [SerializeField] private bool useSceneName = true;
        [SerializeField] private string sceneNameValue;

        [Header("키 설정")]
        [Tooltip("범위 내 플레이어가 누르면 상호작용이 발생하는 키")]
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        [Header("디자이너 훅 (인스펙터에서 연결)")]
        [SerializeField] private UnityEvent onInteract;

        private bool _playerInRange = false;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
        }

        private void Update()
        {
            if (!_playerInRange) return;

            // 플레이어가 범위 내에 있고 interactionKey를 누르면 상호작용 실행
            if (Input.GetKeyDown(interactionKey))
            {
                PerformInteraction();
            }
        }

        // 외부에서 직접 호출할 필요가 있으면 public으로 유지
        public void Interact()
        {
            if (!_playerInRange) return;
            PerformInteraction();
        }

        private void PerformInteraction()
        {
            if (useSceneName)
            {
                if (!string.IsNullOrEmpty(sceneNameValue))
                {
                    SceneManager.LoadScene(sceneNameValue);
                }
                else
                {
                    Debug.LogWarning($"[InteractableSymbol] sceneName이 비어있습니다. 오브젝트: {name}");
                }
            }
            else
            {
                onInteract?.Invoke();
            }
        }
    }
}