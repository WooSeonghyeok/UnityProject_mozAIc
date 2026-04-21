using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ep4_PuzzleCompletePoint : MonoBehaviour
{
    public int puzzleNumber;
    public GameObject exit_Wall;
    private readonly string player = "Player";
    public TextboxCtrl_Ep4 cutsceneManager;
    private bool isPlayed = false;  //각 완료 지점마다 대사 연출을 1회만 재생하도록 체크
    public SaveDataObj CurData;
    private void Awake()  //씬 진입 시 해제
    {
        exit_Wall.SetActive(false);
        CurData = SaveManager.instance.curData;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isPlayed) return;
        if (other.gameObject.CompareTag(player))
        {
            exit_Wall.SetActive(true);
            isPlayed = true;
            if (SaveManager.instance != null)
            {
                switch (puzzleNumber)
                {
                    case 1:
                        {
                            CurData.ep4_puzzle1Clear = true;
                            StartCoroutine(cutsceneManager.Puzzle1Complete());
                            break;
                        }
                    case 2:
                        {
                            CurData.ep4_puzzle2Clear = true;
                            StartCoroutine(cutsceneManager.Puzzle2Complete());
                            break;
                        }
                    case 3:
                        {
                            CurData.ep4_puzzle3Clear = true;
                            StartCoroutine(cutsceneManager.Puzzle3Complete());
                            break;
                        }
                    default: break;
                }
            }
        }
    }
}