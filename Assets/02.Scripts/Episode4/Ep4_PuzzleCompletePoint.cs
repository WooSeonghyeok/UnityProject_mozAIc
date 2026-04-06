using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ep4_PuzzleCompletePoint : MonoBehaviour
{
    public Collider col_Exit;
    public int puzzleNumber;
    public GameObject exit_Wall;
    private readonly string player = "Player";
    private void Awake()  //씬 진입 시 해제
    {
        col_Exit.isTrigger = true;
        exit_Wall.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(player))
        {
            col_Exit.isTrigger = false;
        }
        exit_Wall.SetActive(true);
        if (SaveManager.instance != null)
        {
            switch (puzzleNumber)
            {
                case 1: SaveManager.instance.curData.ep4_puzzle1Clear = true; break;
                case 2: SaveManager.instance.curData.ep4_puzzle2Clear = true; break;
                case 3: SaveManager.instance.curData.ep4_puzzle3Clear = true; break;
            }
        }
    }
}