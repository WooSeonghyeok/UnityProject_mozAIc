using UnityEngine;
using System.Collections;

public class Episode2Manager : MonoBehaviour
{
    [Header("Space Clear 가구")]
    public GameObject[] spaceFurniture;

    [Header("Paint Clear 가구")]
    public GameObject[] paintFurniture;

    [Header("둘 다 클리어 시 (LobbySet)")]
    public GameObject finalObject;

    public float delay = 1.5f;

    void Start()
    {
        ApplyImmediateState();
        StartCoroutine(ApplyDelayedState());
    }

    // 🔹 즉시 상태 적용 (씬 진입 시)
    void ApplyImmediateState()
    {
        if (SaveManager.instance == null) return;

        var data = SaveManager.instance.curData;

        // Space 가구
        if (data.ep2_spaceClear)
        {
            ActivateFurniture(spaceFurniture, false);
        }

        // Paint 가구
        if (data.ep2_paintClear)
        {
            ActivateFurniture(paintFurniture, false);
        }

        // ❌❌❌ 절대 finalObject 건드리지 않음 ❌❌❌
    }

    // 🔹 딜레이 후 연출 적용
    IEnumerator ApplyDelayedState()
    {
        yield return new WaitForSeconds(delay);

        if (SaveManager.instance == null) yield break;

        var data = SaveManager.instance.curData;

        // Space
        if (data.ep2_spaceClear)
        {
            ActivateFurniture(spaceFurniture, true);
        }

        // Paint
        if (data.ep2_paintClear)
        {
            ActivateFurniture(paintFurniture, true);
        }

        // ⭐ 다음 스테이지만 열기 (OK)
        if (data.ep2_spaceClear && data.ep2_paintClear)
        {
            SaveManager.instance.curData.ep3_open = true;
        }

        // ❌ finalObject 절대 건드리지 않음
    }

    // 🔹 가구 활성화 + 연출
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