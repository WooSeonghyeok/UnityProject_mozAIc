using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    public ColorType tileColor;
    public Transform teleportPoint;
    public Transform playerTransform;

    private bool isCleared = false; // 타일 클리어 여부

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerColor player = other.GetComponentInParent<PlayerColor>();

            if (player != null)
            {
                Debug.Log("타일 충돌됨");

                // 🎯 현재 색
                ColorType playerColor = player.currentColor;

                // 🎨 색이 맞으면 타일 숨김 (한 번만)
                if (playerColor == tileColor && !isCleared)
                {
                    MeshRenderer mesh = GetComponent<MeshRenderer>();
                    if (mesh != null)
                        mesh.enabled = false;

                    isCleared = true;
                }

                // 🧼 플레이어 색 초기화 (항상)
                player.ResetColor();

                // 🚀 텔레포트 (항상)
                Teleport();
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