using PuzzleInfo;
using System;
using System.Collections.Generic;
using UnityEngine;
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
            bool b = curCube.condition switch
            {
                EP4_Puzzle4_Cube.switchCondition.near   => Mathf.Abs(cubeList[n].posY - curCube.posY) + Mathf.Abs(cubeList[n].posX - curCube.posX) <= 1,
                EP4_Puzzle4_Cube.switchCondition.row    => cubeList[n].posX == curCube.posX,
                EP4_Puzzle4_Cube.switchCondition.column => cubeList[n].posY == curCube.posY,
                EP4_Puzzle4_Cube.switchCondition.color  => cubeList[n].cubeColor == curCube.cubeColor,
                _ => false
            };
            if (b) switchObjects.Add(cubeList[n]);  // 조건을 만족하는 발판을 리스트에 포함
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