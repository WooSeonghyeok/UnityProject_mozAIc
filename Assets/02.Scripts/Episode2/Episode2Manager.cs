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
        ApplyImmediateState();
        StartCoroutine(ApplyDelayedState());
    }

    void ApplyImmediateState()
    {
        if (EP2_PuzzleManager.Instance != null)
        {
            if (EP2_PuzzleManager.Instance.spaceClear &&
                EP2_PuzzleManager.Instance.spaceFurnitureSpawned)
            {
                ActivateFurniture(spaceFurniture, false);
            }

            if (EP2_PuzzleManager.Instance.paintClear &&
                EP2_PuzzleManager.Instance.paintFurnitureSpawned)
            {
                ActivateFurniture(paintFurniture, false);
            }
        }
    }

    IEnumerator ApplyDelayedState()
    {
        yield return new WaitForSeconds(delay);

        // ⭐ Instance 안전 처리
        bool spaceClear = false;
        bool paintClear = false;
        bool spaceSpawned = false;
        bool paintSpawned = false;

        if (EP2_PuzzleManager.Instance != null)
        {
            spaceClear = EP2_PuzzleManager.Instance.spaceClear;
            paintClear = EP2_PuzzleManager.Instance.paintClear;
            spaceSpawned = EP2_PuzzleManager.Instance.spaceFurnitureSpawned;
            paintSpawned = EP2_PuzzleManager.Instance.paintFurnitureSpawned;
        }
        else
        {
            // ⭐ fallback (혹시 Instance 없을 때 대비)
            spaceClear = PlayerPrefs.GetInt("Space_Cleared", 0) == 1;
            paintClear = PlayerPrefs.GetInt("Paint_Cleared", 0) == 1;
        }

        // Space
        if (spaceClear && !spaceSpawned)
        {
            ActivateFurniture(spaceFurniture, true);

            if (EP2_PuzzleManager.Instance != null)
                EP2_PuzzleManager.Instance.spaceFurnitureSpawned = true;
        }

        // Paint
        if (paintClear && !paintSpawned)
        {
            ActivateFurniture(paintFurniture, true);

            if (EP2_PuzzleManager.Instance != null)
                EP2_PuzzleManager.Instance.paintFurnitureSpawned = true;
        }

        // ⭐ 둘 다 클리어 (AllClear 대체)
        if (spaceClear && paintClear)
        {
            if (finalObject != null)
            {
                finalObject.SetActive(true);
            }

            if (SaveManager.instance != null)
            {
                SaveManager.instance.curData.ep3_open = true;
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