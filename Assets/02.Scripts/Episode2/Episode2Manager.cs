using UnityEngine;
using System.Collections;

public class Episode2Manager : MonoBehaviour
{
    [Header("Space Clear 가구")]
    public GameObject[] spaceFurniture;

    [Header("Paint Clear 가구")]
    public GameObject[] paintFurniture;

    [Header("둘 다 클리어 시")]
    public GameObject finalObject;

    public float delay = 1.5f;

    void Start()
    {
        ApplyImmediateState(); // 🔥 먼저 바로 적용
        StartCoroutine(ApplyDelayedState()); // 🔥 나중 연출
    }

    // 🔥 이미 존재하는 가구 → 즉시 표시
    void ApplyImmediateState()
    {
        if (PuzzleManager.Instance.spaceClear && PuzzleManager.Instance.spaceFurnitureSpawned)
        {
            ActivateFurniture(spaceFurniture, false);
        }

        if (PuzzleManager.Instance.paintClear && PuzzleManager.Instance.paintFurnitureSpawned)
        {
            ActivateFurniture(paintFurniture, false);
        }
    }

    // 🔥 새로 등장하는 가구 → delay + 연출
    IEnumerator ApplyDelayedState()
    {
        yield return new WaitForSeconds(delay);

        // Space
        if (PuzzleManager.Instance.spaceClear && !PuzzleManager.Instance.spaceFurnitureSpawned)
        {
            ActivateFurniture(spaceFurniture, true);
            PuzzleManager.Instance.spaceFurnitureSpawned = true;
        }

        // Paint
        if (PuzzleManager.Instance.paintClear && !PuzzleManager.Instance.paintFurnitureSpawned)
        {
            ActivateFurniture(paintFurniture, true);
            PuzzleManager.Instance.paintFurnitureSpawned = true;
        }

        // 둘 다 클리어
        if (PuzzleManager.Instance.AllClear())
        {
            if (finalObject != null)
            {
                finalObject.SetActive(true);
            }
        }
    }

    void ActivateFurniture(GameObject[] furnitureList, bool playEffect)
    {
        foreach (var obj in furnitureList)
        {
            obj.SetActive(true);

            var fade = obj.GetComponent<FurnitureEmissionFade>();

            if (fade != null)
            {
                if (playEffect)
                    fade.PlayFade();
                else
                    fade.SetEmissionOff();
            }
        }
    }
}