using UnityEngine;
using static PuzzleInfo.EP4_Puzzle4_Cube;
[CreateAssetMenu(fileName = "PuzzleCube", menuName = "Create CubeData", order = 1)]
public class EP4_Puzzle4_CubeObj : ScriptableObject
{
    public int[] place = new int[2];
    public bool[] colorBool = new bool[3];
    public switchCondition cond;
    public int value;
}