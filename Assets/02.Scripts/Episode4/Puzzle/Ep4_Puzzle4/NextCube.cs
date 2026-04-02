using UnityEngine;
public class NextCube : MonoBehaviour
{
    private BoxCollider col;
    private PuzzleCubeCtrl curCube;
    private PuzzleCubeCtrl[] cubeList;
    [SerializeField] private PuzzleCubeCtrl nextCube;
    public enum CubeDir { west, east, north, south }
    public CubeDir dir;
    private int nextX;
    private int nextY;
    private void Awake()
    {
        col = GetComponent<BoxCollider>();
        curCube = GetComponentInParent<PuzzleCubeCtrl>();
        cubeList = GameObject.Find("CubePuzzle").GetComponentsInChildren<PuzzleCubeCtrl>();
    }
    private void Start()
    {
        switch (dir)
        {
            case CubeDir.west:
                nextX = curCube.row - 1;
                nextY = curCube.column;
                break;
            case CubeDir.east:
                nextX = curCube.row + 1;
                nextY = curCube.column;
                break;
            case CubeDir.north:
                nextX = curCube.row;
                nextY = curCube.column + 1;
                break;
            case CubeDir.south:
                nextX = curCube.row;
                nextY = curCube.column - 1;
                break;
        }
        for (int i = 0; i < cubeList.Length; i++)
        {
            if (cubeList[i].row == nextX && cubeList[i].column == nextY)
            {
                nextCube = cubeList[i];
                break;
            }
        }
        curCube.OnColorChanged += UpdateCollider;
        if (nextCube != null) nextCube.OnColorChanged += UpdateCollider;
        UpdateCollider();
    }
    private void UpdateCollider()
    {
        if (nextCube != null)
            col.enabled = CanMoveThrough();
        else
            col.enabled = false;
    }
    public bool CanMoveThrough()
    {
        return (curCube.cubeColor != nextCube.cubeColor);
    }
}