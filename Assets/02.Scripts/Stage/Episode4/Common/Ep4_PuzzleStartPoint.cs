using UnityEngine;
public class Ep4_PuzzleStartPoint : MonoBehaviour
{
    public int puzzleNumber;
    private readonly string player = "Player";
    public TextboxCtrl_Ep4 cutsceneManager;
    private bool isPlayed = false;  //각 시작 지점마다 대사 연출을 1회만 재생하도록 체크
    private void OnTriggerEnter(Collider other)
    {
        if (isPlayed) return;
        if (other.gameObject.CompareTag(player))
        {
            isPlayed = true;
            switch (puzzleNumber)
            {
                case 1: StartCoroutine(cutsceneManager.Puzzle1Start()); break;
                case 2: StartCoroutine(cutsceneManager.Puzzle2Start()); break;
                case 3: StartCoroutine(cutsceneManager.Puzzle3Start()); break;
                case 4: StartCoroutine(cutsceneManager.Puzzle4Start()); break;
                default: break;
            }
        }
    }
}