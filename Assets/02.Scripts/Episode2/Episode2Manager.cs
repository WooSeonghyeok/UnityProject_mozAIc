using UnityEngine;
using System.Collections;
using Cinemachine;

public class Episode2Manager : MonoBehaviour
{
    [Header("Space Clear 가구")]
    public GameObject[] spaceFurniture;

    [Header("Paint Clear 가구")]
    public GameObject[] paintFurniture;

    [Header("Final 연출 (캔버스)")]
    public GameObject canvasObject;   // 이동할 흰 캔버스
    public GameObject finalObject;    // 완성된 그림 (처음엔 꺼둘 것)

    [Header("Camera")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera puzzleCam;

    [Header("Timing")]
    public float delay = 1.5f;
    public float camMoveTime = 1.5f;     // 카메라 이동 시간
    public float effectTime = 2.0f;      // 캔버스 연출 시간

    void Start()
    {
        ApplyImmediateState();
        StartCoroutine(ApplyDelayedState());
    }

    void ApplyImmediateState()
    {
        if (EP2_PuzzleManager.Instance.spaceClear && EP2_PuzzleManager.Instance.spaceFurnitureSpawned)
        {
            ActivateFurniture(spaceFurniture, false);
        }

        if (EP2_PuzzleManager.Instance.paintClear && EP2_PuzzleManager.Instance.paintFurnitureSpawned)
        {
            ActivateFurniture(paintFurniture, false);
        }

        // 이미 클리어 상태라면 결과 유지
        if (EP2_PuzzleManager.Instance.AllClear())
        {
            if (finalObject != null)
                finalObject.SetActive(true);
        }
    }

    IEnumerator ApplyDelayedState()
    {
        yield return new WaitForSeconds(delay);

        // Space
        if (EP2_PuzzleManager.Instance.spaceClear && !EP2_PuzzleManager.Instance.spaceFurnitureSpawned)
        {
            ActivateFurniture(spaceFurniture, true);
            EP2_PuzzleManager.Instance.spaceFurnitureSpawned = true;
        }

        // Paint
        if (EP2_PuzzleManager.Instance.paintClear && !EP2_PuzzleManager.Instance.paintFurnitureSpawned)
        {
            ActivateFurniture(paintFurniture, true);
            EP2_PuzzleManager.Instance.paintFurnitureSpawned = true;
        }

        // 🔥 둘 다 클리어 시
        if (EP2_PuzzleManager.Instance.AllClear())
        {
            StartCoroutine(PlaySequence());

            if (SaveManager.instance != null)
            {
                SaveManager.instance.curData.ep3_open = true;
            }
        }
    }

    IEnumerator PlaySequence()
    {
        // 👉 1. 카메라 이동
        SetCameraPriority(puzzleCam, 20);
        SetCameraPriority(playerCam, 10);

        yield return new WaitForSeconds(camMoveTime);

        // 👉 2. 살짝 멈춤 (연출 준비 시간)
        yield return new WaitForSeconds(0.3f);

        // 👉 3. 캔버스 연출 시작
        if (canvasObject != null)
        {
            canvasObject.SetActive(true);
        }

        // 👉 4. 연출 끝까지 대기
        yield return new WaitForSeconds(effectTime);

        // 👉 5. 카메라 복귀
        SetCameraPriority(playerCam, 20);
        SetCameraPriority(puzzleCam, 10);
    }

    void SetCameraPriority(CinemachineVirtualCamera cam, int value)
    {
        if (cam != null)
        {
            cam.Priority = value;
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