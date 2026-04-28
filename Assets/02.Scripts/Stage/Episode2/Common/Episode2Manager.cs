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

    [Header("EP2 그림 힌트 대상")]
    public NPCHintTarget spacePicture; // 구도가 틀어진 그림
    public NPCHintTarget colorPicture;       // 색이 바랜 그림

    public float delay = 1.5f;

    void Start()
    {
        ApplyImmediateState();
        // 저장된 퍼즐 완료 상태를 NPC 힌트 설명에 반영
        ApplyPictureHintState();
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
    // 그림 완성 여부를 NPCHintTarget 설명에 반영
    private void ApplyPictureHintState()
    {
        if (SaveManager.instance == null || SaveManager.instance.curData == null)
            return;

        var data = SaveManager.instance.curData;

        if (spacePicture != null)
        {
            if (data.ep2_spaceClear)
            {
                spacePicture.description = "이미 완성된 그림이다. 구도가 바로잡혀 안정적인 장면이 되었다.";
            }
            else
            {
                spacePicture.description = "아직 완성되지 않은 그림이다. 구도가 틀어져 있어 바로잡아야 한다.";
            }
        }

        if (colorPicture != null)
        {
            if (data.ep2_paintClear)
            {
                colorPicture.description = "이미 완성된 그림이다. 바랬던 색이 다시 돌아왔다.";
            }
            else
            {
                colorPicture.description = "아직 완성되지 않은 그림이다. 색이 바래 있어 되찾아야 한다.";
            }
        }

        Debug.Log($"[Episode2Manager] 그림 힌트 상태 갱신 - 구도:{data.ep2_spaceClear}, 색:{data.ep2_paintClear}");
    }
}