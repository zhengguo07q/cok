using GameBase;
using System;
using System.Collections.Generic;


namespace GameLogic.Country.Model
{
    public class KingdomManager : Singleton<KingdomManager>
    {
        /// <summary>
        /// 当前王国ID
        /// </summary>
        public float KingdomId;

        public List<Kingdom> KingdomList = new();

        private int[,] grid;
        private Dictionary<int, Tuple<int, int>> numberToPosition = new Dictionary<int, Tuple<int, int>>();

        public void SetKingdomList(IList<global::Country.V1.Kingdom> kingdomList)
        {
            KingdomList.Clear();
            foreach (var item in kingdomList)
            {
                KingdomList.Add(Kingdom.FromPb(item));
            }
            int size = GetMinGridSize(KingdomList.Count);
            PrecomputedGrid(size);
        }

        public void PrecomputedGrid(int size)
        {
            grid = new int[size, size];
            FillSpiral(size);
        }

        private void FillSpiral(int size)
        {
            int x = size / 2, y = size / 2; // 中心开始
            int num = 0;
            int step = 1;
            int direction = 0; // 0: 右, 1: 下, 2: 左, 3: 上

            while (num <= 1000)
            {
                for (int i = 0; i < step; i++)
                {
                    if (num > 1000) break;
                    grid[y, x] = num;
                    numberToPosition[num] = Tuple.Create(y, x);
                    num++;
                    Move(ref x, ref y, direction);
                }
                direction = (direction + 1) % 4;
                if (direction % 2 == 0) step++; // 每两次转向增加一步
                for (int i = 0; i < step; i++)
                {
                    if (num > 1000) break;
                    grid[y, x] = num;
                    numberToPosition[num] = Tuple.Create(y, x);
                    num++;
                    Move(ref x, ref y, direction);
                }
                direction = (direction + 1) % 4;
            }
        }



        public List<int> GetNeighbors(int number)
        {
            var neighbors = new List<int>();
            if (!numberToPosition.ContainsKey(number)) return neighbors;

            var pos = numberToPosition[number];
            int y = pos.Item1;
            int x = pos.Item2;

            // 上下左右四个方向
            foreach (var dir in new[] { Tuple.Create(-1, 0), Tuple.Create(1, 0), Tuple.Create(0, -1), Tuple.Create(0, 1) })
            {
                int ny = y + dir.Item1;
                int nx = x + dir.Item2;

                if (ny >= 0 && ny < grid.GetLength(0) && nx >= 0 && nx < grid.GetLength(1))
                {
                    int neighborNumber = grid[ny, nx];
                    if (neighborNumber >= 0 && neighborNumber <= 1000)
                    {
                        neighbors.Add(neighborNumber);
                    }
                }
            }

            return neighbors;
        }

        public static void Move(ref int x, ref int y, int direction)
        {
            switch (direction)
            {
                case 0: x++; break; // 右
                case 1: y++; break; // 下
                case 2: x--; break; // 左
                case 3: y--; break; // 上
            }
        }

        // 计算包含给定数字的最小环
        private static int GetRing(int number)
        {
            if (number == 0) return 0;
            double k = Math.Sqrt((number - 1) / 4.0);
            return (int)Math.Ceiling(k);
        }

        // 计算最小网格的边长
        public static int GetMinGridSize(int number)
        {
            int ring = GetRing(number);
            return 1 + 2 * ring;
        }
    }
}
