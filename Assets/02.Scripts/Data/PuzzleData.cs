namespace PuzzleInfo
{
    public class PuzzleCube
    {
        public int id;
        public int[] place = new int[2];
        public bool[] colorBool = new bool[3];
        public enum switchCondition { near, row, column, color };
        public switchCondition cond;
        public int switchValue;
    }
}