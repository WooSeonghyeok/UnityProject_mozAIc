using UnityEngine;
using static PuzzleInfo.PuzzleCube;
[CreateAssetMenu(fileName = "PuzzleCube", menuName = "Create CubeData", order = 1)]
public class PuzzleCubeObj : ScriptableObject
{
    public int[] place = new int[2];
    public bool[] colorBool = new bool[3];
    public switchCondition cond;
    public int value;
}