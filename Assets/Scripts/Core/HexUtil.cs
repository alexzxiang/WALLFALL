using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// Pointy-top hex grid math, odd-r offset coordinates.
    /// Lane board: cols 0..4, rows 0..5. Rows 0-2 = your half (bottom), rows 3-5 = enemy half (top).
    /// </summary>
    public static class HexUtil
    {
        public const float Size = 0.45f;                       // corner radius — sized for the 8x9 board
        public static readonly float Width = Mathf.Sqrt(3f) * Size;
        public static readonly float RowStep = 1.5f * Size;

        public static Vector2 ToWorld(int col, int row)
        {
            float x = (col + 0.5f * (row & 1)) * Width;
            float y = row * RowStep;
            return new Vector2(x, y);
        }

        // odd-r offset -> cube coords
        static Vector3Int ToCube(int col, int row)
        {
            int q = col - (row - (row & 1)) / 2;
            int r = row;
            return new Vector3Int(q, r, -q - r);
        }

        public static int Distance(int c1, int r1, int c2, int r2)
        {
            Vector3Int a = ToCube(c1, r1), b = ToCube(c2, r2);
            return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
        }

        // Neighbor offsets for odd-r: [rowParity][direction] = (dCol, dRow)
        static readonly int[][][] Neighbors =
        {
            new[] { new[]{1,0}, new[]{0,-1}, new[]{-1,-1}, new[]{-1,0}, new[]{-1,1}, new[]{0,1} },   // even row
            new[] { new[]{1,0}, new[]{1,-1}, new[]{0,-1},  new[]{-1,0}, new[]{0,1},  new[]{1,1} }    // odd row
        };

        public static void GetNeighbors(int col, int row, System.Collections.Generic.List<Vector2Int> result)
        {
            result.Clear();
            var table = Neighbors[row & 1];
            for (int i = 0; i < 6; i++)
            {
                int c = col + table[i][0];
                int r = row + table[i][1];
                if (c >= 0 && c < GameConfig.BoardCols && r >= 0 && r < GameConfig.BoardRowsPerSide * 2)
                    result.Add(new Vector2Int(c, r));
            }
        }
    }
}
