using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    public ColorType tileColor;
    public Transform teleportPoint;
    public Transform playerTransform;

    private bool isCleared = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerColor player = other.GetComponentInParent<PlayerColor>();

            if (player != null)
            {
                Debug.Log("타일 충돌됨");

                ColorType playerColor = player.currentColor;

                // ❌ 1. 이미 클리어된 타일 다시 밟음
                if (isCleared)
                {
                    Debug.Log("이미 클리어된 타일 → -1");
                    Episode2ScoreManager.Instance?.ReducePaintScore();
                }
                // ❌ 2. 색이 틀림
                else if (playerColor != tileColor)
                {
                    Debug.Log("색이 틀림 → -1");
                    Episode2ScoreManager.Instance?.ReducePaintScore();
                }
                // ✅ 3. 정답
                else
                {
                    MeshRenderer mesh = GetComponent<MeshRenderer>();
                    if (mesh != null)
                        mesh.enabled = false;

                    isCleared = true;
                }

                // ⭐ 낙하 종료 추가 (핵심🔥)
                FallFOVEffect fov = FindObjectOfType<FallFOVEffect>();
                if (fov != null)
                {
                    fov.StopFall();
                }

                // 공통 처리
                player.ResetColor();
                Teleport();
                Episode2ScoreManager.Instance.Ep2_PuzzleScore();
            }
        }
    }

    public bool IsCleared()
    {
        return isCleared;
    }

    void Teleport()
    {
        if (teleportPoint == null || playerTransform == null)
        {
            Debug.LogError("텔레포트 설정 안됨!");
            return;
        }

        CharacterController cc = playerTransform.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        playerTransform.position = teleportPoint.position;

        if (cc != null)
            cc.enabled = true;

        Debug.Log("텔레포트 완료");
    }
}