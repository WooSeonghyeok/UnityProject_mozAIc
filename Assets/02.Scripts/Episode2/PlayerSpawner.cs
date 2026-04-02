using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    public Transform player;

    public Transform spawnPointSpace;
    public Transform spawnPointPaint;
    public Transform spawnPointDefault;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.3f);

        if (player == null)
        {
            Debug.LogError("Player 없음!");
            yield break;
        }

        string spawnType = PuzzleManager.Instance.spawnType;

        Transform targetPoint = spawnPointDefault;

        if (spawnType == "Space")
            targetPoint = spawnPointSpace;

        else if (spawnType == "Paint")
            targetPoint = spawnPointPaint;

        // 이동
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        player.position = targetPoint.position;

        if (cc != null)
            cc.enabled = true;

        Debug.Log("스폰 위치: " + targetPoint.name);

        // 🔥 반드시 초기화
        PuzzleManager.Instance.spawnType = "Default";
    }
}