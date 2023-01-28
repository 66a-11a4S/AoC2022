/*
day24

谷と吹雪の地図を作成します

#.#####
#.....#
#>....#
#.....#
#...v.#
#.....#
#####.#

* 壁は # で表示される
* 現在ブリザードがないグラウンドは . として描画
  * ブリザードは、上 (^)、下 (v)、左 (<)、または右 (>) の移動方向を示す矢印で描画
  * 1分間で、各ブリザードはそれが指している方向に1つ移動
  * 谷の壁に到達すると、谷の反対側に新しい吹雪が形成される
  * ブリザードは互いに通り抜けられ、重ねられる

* 最上列の壁のない唯一の位置からスタート
* 最下列の壁のない唯一の位置がゴール
* 1分ごとに、上下左右に移動したり、その場で待機したりできる
* あなたとブリザードは同時に行動し、ブリザードと位置を共有することはできません。(すれ違いは可能)

Q1. 吹雪を避けてゴールに到達するのに必要な最短時間は?

Q2. スタート -> ゴール -> スタート -> ゴールに到着するのにかかる最短時間は?
*/

public class Day24
{
    private enum Cell
    {
        None,
        Obstacle,
        BlizzardUp,
        BlizzardRight,
        BlizzardDown,
        BlizzardLeft,
    }

    public static void PartOne()
    {
        var lines = new List<string>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            lines.Add(line);
        }

        var startX = lines[0].IndexOf('.');
        var startY = 0;
        var goalX = lines.Last().IndexOf('.');
        var goalY = lines.Count - 1;

        var rawGrid = CreateGrid(lines);
        var result = CalculateStep(startX, startY, goalX, goalY, startTime: 0, rawGrid);
        Console.WriteLine(result);
    }

    public static void PartTwo()
    {
        var lines = new List<string>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            lines.Add(line);
        }

        var startX = lines[0].IndexOf('.');
        var startY = 0;
        var goalX = lines.Last().IndexOf('.');
        var goalY = lines.Count - 1;

        var rawGrid = CreateGrid(lines);

        // start -> goal
        int? time = 0;
        time = CalculateStep(startX, startY, goalX, goalY, startTime: time.Value, rawGrid);

        if (!time.HasValue)
        {
            Console.WriteLine("failed to reach.");
            return;
        }

        // goal -> start
        time = CalculateStep(goalX, goalY, startX, startY, startTime: time.Value, rawGrid);

        if (!time.HasValue)
        {
            Console.WriteLine("failed to reach.");
            return;
        }

        // start -> goal
        time = CalculateStep(startX, startY, goalX, goalY, startTime: time.Value, rawGrid);
        if (!time.HasValue)
        {
            Console.WriteLine("failed to reach.");
            return;
        }

        Console.WriteLine(time);
    }

    private static int? CalculateStep(int startX, int startY, int goalX, int goalY, int startTime, Cell[,] rawGrid)
    {
        var grid = new Dictionary<int, bool[,]>(){ {0, CreateGrid(rawGrid, 0)} };
        var height = rawGrid.GetLength(dimension: 0);
        var width = rawGrid.GetLength(dimension: 1);

        var visitedStatus = new HashSet<(int, int, int)>();
        var queue = new Queue<(int X, int Y, int T)>();
        queue.Enqueue((X: startX, Y: startY, T: startTime));

        while (queue.Any())
        {
            var p = queue.Dequeue();

            // 時刻順に調べているので、最初に goal に到達したら最速のはず
            if (p.X == goalX && p.Y == goalY)
                return p.T;

            foreach (var nextP in NextSteps(p.X, p.Y, p.T))
            {
                // ある時刻で訪れたことがある位置は再調査しない
                if (visitedStatus.Contains(nextP))
                    continue;

                // 吹雪や障害物の位置には移動しない
                if (!IsMovable(nextP.X, nextP.Y, nextP.T))
                    continue;

                queue.Enqueue(nextP);
                visitedStatus.Add(nextP);
            }
        }

        return null;

        bool IsMovable(int x, int y, int t)
        {
            if (x == startX && y == startY)
                return true;

            if (x == goalX && y == goalY)
                return true;

            if (x < 0 || width <= x || y < 0 || height <= y)
                return false;

            // Get or Create Grid
            if (!grid.TryGetValue(t, out var currentGrid))
            {
                grid[t] = CreateGrid(rawGrid, t);
                currentGrid = grid[t];
            }

            return !currentGrid[y, x];
        }

        static IReadOnlyCollection<(int X, int Y, int T)> NextSteps(int x, int y, int t)
        {
            return new[]
            {
                (x, y, t + 1), // 待機
                (x + 1, y, t + 1), // 隣に移動
                (x - 1, y, t + 1),
                (x, y + 1, t + 1),
                (x, y - 1, t + 1),
            };
        }
    }

    private static Cell[,] CreateGrid(IReadOnlyList<string> lines)
    {
        var result = new Cell[lines.Count, lines[0].Length];

        for (var y = 0; y < lines.Count; y++)
        {
            for (var x = 0; x < lines[y].Length; x++)
            {
                switch (lines[y][x])
                {
                    case '#': result[y, x] = Cell.Obstacle; break;
                    case '^': result[y, x] = Cell.BlizzardUp; break;
                    case '>': result[y, x] = Cell.BlizzardRight; break;
                    case 'v': result[y, x] = Cell.BlizzardDown; break;
                    case '<': result[y, x] = Cell.BlizzardLeft; break;
                }
            }
        }

        return result;
    }

    private static bool[,] CreateGrid(Cell[,] rawGrid, int t)
    {
        var height = rawGrid.GetLength(dimension: 0);
        var width = rawGrid.GetLength(dimension: 1);
        var result = new bool[height, width];

        for (var x = 0; x < width; x++)
        {
            result[0, x] = rawGrid[0, x] != Cell.None;
            result[height - 1, x] = rawGrid[height - 1, x] != Cell.None;
        }

        for (var y = 0; y < height; y++)
        {
            result[y, 0] = rawGrid[y, 0]  != Cell.None;
            result[y, width - 1] = rawGrid[y, width - 1] != Cell.None;
        }

        var loopHeight = height - 2;
        var loopWidth = width - 2;
        for (var y = 1; y < height - 1; y++)
        {
            for (var x = 1; x < width - 1; x++)
            {
                var value = rawGrid[y, x];
                if (value == Cell.None)
                    continue;

                var (diffX, diffY) = GetVec(value);
                // ループスタートは1から, ループ終了は長さ - 2
                // (y, x) の Blizzard を t 秒後の位置に移動させる
                var actualY = LoopedValue(y - 1 + diffY * t, loopHeight) + 1;
                var actualX = LoopedValue(x - 1 + diffX * t, loopWidth) + 1;
                result[actualY, actualX] = true;
            }
        }

        return result;

        static int LoopedValue(int value, int loopLength)
        {
            if (0 < value)
                return value % loopLength;

            var offset = value % loopLength;
            return (loopLength + offset) % loopLength;
        }

        static (int DiffX, int DiffY) GetVec(Cell value)
        {
            switch (value)
            {
                case Cell.BlizzardUp: return (0, -1);
                case Cell.BlizzardRight: return (1, 0);
                case Cell.BlizzardDown: return (0, 1);
                case Cell.BlizzardLeft: return (-1, 0);
                default: throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
