using PuzzleInfo;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UI;
public class EP4_CubeSwitch : MonoBehaviour
{
    private BoxCollider col;
    [SerializeField] private EP4_Puzzle4_CubeCtrl curCube;
    [SerializeField] private EP4_Puzzle4_CubeCtrl[] cubeList;
    [SerializeField] private List<EP4_Puzzle4_CubeCtrl> switchObjects;
    public bool switchContact = false;
    private PlayerInput user;
    private PlayerMovement userMove;
    private readonly string playerTag = "Player";
    public event Action SwitchClick;
    private SoundTrigger sound;
    void Awake()
    {
        col = GetComponent<BoxCollider>();
        cubeList = GameObject.Find("CubePuzzle").GetComponentsInChildren<EP4_Puzzle4_CubeCtrl>();
        curCube = GetComponentInParent<EP4_Puzzle4_CubeCtrl>();
        switchObjects = new List<EP4_Puzzle4_CubeCtrl>();
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        userMove = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerMovement>();
        sound = gameObject.GetComponent<SoundTrigger>();
    }
    private void OnEnable()
    {
        user.Interact += UseSwitch_Puzzle4;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (curCube != null && other.gameObject.CompareTag("Player"))
        {
            switchContact = true;
            Puzzle4Manager.instance.interactionUI.GetComponent<Image>().color = Puzzle4Manager.instance.retry_count == 0 ? Color.white : SwitchColor(curCube.switchValue);
            Puzzle4Manager.instance.interactionUI.SetActive(true);
            ConditionCheck();
        }
    }
    public Color SwitchColor(int colorCode)  // 스위치 사용 시 변경 후 색을 띄워주는 코드
    {
        int curRed = curCube.isRed ? 1 : 0;
        int curGreen = curCube.isGreen ? 2 : 0;
        int curBlue = curCube.isBlue ? 4 : 0;
        bool isRed = curRed != (colorCode & 1);
        bool isGreen = curGreen != (colorCode & 2);
        bool isBlue = curBlue != (colorCode & 4);
        return new Color(isRed ? 1 : 0, isGreen ? 1 : 0, isBlue ? 1 : 0);
    }
    public void UseSwitch_Puzzle4()
    {
        if (!switchContact) return;
        foreach (var obj in switchObjects)
        {
            obj.OnColorSwitch(curCube.switchValue);
        }
        SwitchClick?.Invoke();
        sound.Play();
    }
    private void ConditionCheck()
    {
        for (int n = 0; n < cubeList.Length; n++)
        {
            switch (curCube.condition)
            {
                case EP4_Puzzle4_Cube.switchCondition.near:
                    if (Mathf.Abs(cubeList[n].column - curCube.column) + Mathf.Abs(cubeList[n].row - curCube.row) <= 1)  //자기 자신 + 이웃한 발판(전후좌우 1칸씩)
                    {
                        switchObjects.Add(cubeList[n]);
                    }
                    break;
                case EP4_Puzzle4_Cube.switchCondition.row:
                    if (cubeList[n].row == curCube.row)  // 행 값이 같으면
                    {
                        switchObjects.Add(cubeList[n]);
                    }
                    break;
                case EP4_Puzzle4_Cube.switchCondition.column:
                    if (cubeList[n].column == curCube.column)  // 열 값이 같으면
                    {
                        switchObjects.Add(cubeList[n]);
                    }
                    break;
                case EP4_Puzzle4_Cube.switchCondition.color:
                    if (cubeList[n].cubeColor == curCube.cubeColor)  // 색상이 같으면
                    {
                        switchObjects.Add(cubeList[n]);
                    }
                    break;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            for (int n = 0; n < cubeList.Length; n++)
            {
                switchObjects.Remove(cubeList[n]);
            }
            Puzzle4Manager.instance.interactionUI.SetActive(false);
            switchContact = false;
        }
    }
}