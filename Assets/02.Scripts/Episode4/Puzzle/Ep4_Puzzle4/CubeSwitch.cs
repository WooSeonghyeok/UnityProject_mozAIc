using PuzzleInfo;
using System;
using System.Collections.Generic;
using UnityEngine;
public class CubeSwitch : MonoBehaviour
{
    private BoxCollider col;
    [SerializeField] private PuzzleCubeCtrl curCube;
    [SerializeField] private PuzzleCubeCtrl[] cubeList;
    [SerializeField] private List<PuzzleCubeCtrl> switchObjects;
    public bool switchContact = false;
    private PlayerInput user;
    private PlayerMovement userMove;
    private readonly string playerTag = "Player";
    public event Action SwitchClick;
    public AudioSource source;
    public AudioClip switchClip;
    void Awake()
    {
        col = GetComponent<BoxCollider>();
        cubeList = GameObject.Find("CubePuzzle").GetComponentsInChildren<PuzzleCubeCtrl>();
        curCube = GetComponentInParent<PuzzleCubeCtrl>();
        switchObjects = new List<PuzzleCubeCtrl>();
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        userMove = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerMovement>();
        source = gameObject.GetComponent<AudioSource>();
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
            ConditionCheck();
        }
    }
    public void UseSwitch_Puzzle4()
    {
        if (!switchContact) return;
        foreach (var obj in switchObjects)
        {
            obj.OnColorSwitch(curCube.switchValue);
        }
        SwitchClick?.Invoke();
        source.PlayOneShot(switchClip);
    }
    private void ConditionCheck()
    {
        for (int n = 0; n < cubeList.Length; n++)
        {
            switch (curCube.condition)
            {
                case PE4_Puzzle4_Cube.switchCondition.near:
                    if (Mathf.Abs(cubeList[n].column - curCube.column) + Mathf.Abs(cubeList[n].row - curCube.row) <= 1)  //자기 자신 + 이웃한 발판(전후좌우 1칸씩)
                    {
                        switchObjects.Add(cubeList[n]);
                    }
                    break;
                case PE4_Puzzle4_Cube.switchCondition.row:
                    if (cubeList[n].row == curCube.row)  // 행 값이 같으면
                    {
                        switchObjects.Add(cubeList[n]);
                    }
                    break;
                case PE4_Puzzle4_Cube.switchCondition.column:
                    if (cubeList[n].column == curCube.column)  // 열 값이 같으면
                    {
                        switchObjects.Add(cubeList[n]);
                    }
                    break;
                case PE4_Puzzle4_Cube.switchCondition.color:
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
            switchContact = false;
        }
    }
}