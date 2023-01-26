/*
day 12

* デバイスに周辺領域の高さマップが表示される
* グリッドの各マスの標高は、1つの小文字で指定される
  * a は最低、bは次に低い、最高の標高 z まで続きます

* S から開始して E に到達したいが、できるだけ少ないステップで到達したい
  * S の高さは a, E の高さは z
* 各ステップでは1マス上下左右に移動
* 1マス高い場所かそれより低い場所なら移動できる

Q1. S から E への最短経路長はいくつ?
Q2. 高さaの任意の位置からEまでの最短経路長はいくつ?
*/

public class Day12
{
    public class Node
    {
        public int x = 0;
        public int y = 0;
        public int height = 0;
        public IReadOnlyCollection<Node> Children => _children;
        private List<Node> _children = new List<Node>();

        public void TryConnect(Node to)
        {
            var diff = to.height - height;
            if (diff <= 1)
                _children.Add(to);
        }
    }

    public static int[,] GetDistanceTable(IReadOnlyList<IReadOnlyList<Node>> grid, Node start)
    {
        var width = grid[0].Count();
        var height = grid.Count;

        var result = new int[height, width];
        var calculatedCostTable = new bool[height, width];
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                calculatedCostTable[y, x] = false;
                result[y, x] = int.MaxValue;
            }
        }

        // TODO: .NET6 で PriorityQueue が入るのでそちらに差し替え
        var candidates = new HashSet<Node>();

        // 最初のノードを追加
        candidates.Add(start);
        result[start.y, start.x] = 0;
        calculatedCostTable[start.y, start.x] = true;

        while(candidates.Any())
        {
            // コストが最小のものへ遷移
            Node current = null;
            var lowestCost = long.MaxValue;
            foreach (var c in candidates)
            {
                if (result[c.y, c.x] < lowestCost)
                {
                    lowestCost = result[c.y, c.x];
                    current = grid[c.y][c.x];
                }
            }
            candidates.Remove(current);
            calculatedCostTable[current.y, current.x] = false;

            foreach (var to in current.Children)
            {
                var costTo = result[current.y, current.x] + 1;
                if (costTo < result[to.y, to.x])
                {
                    result[to.y, to.x] = costTo;
                    if (!calculatedCostTable[to.y, to.x])
                    {
                        calculatedCostTable[to.y, to.x] = true;
                        candidates.Add(grid[to.y][to.x]);
                    }
                }
            }
        }

        return result;
    }

    public static void PartOne()
    {
        Console.WriteLine("start input");

        var (startX, startY) = (0, 0);
        var (endX, endY) = (0, 0);
        var costTable = new List<int[]>();
        var grid = new List<List<Node>>();
        while (true)
        {
            var input = Console.ReadLine();
            if (!input.Any())
                break;

            var line = new List<Node>();
            var y = grid.Count;
            for (var x = 0; x < input.Length; ++x)
            {
                var altitude = 0;
                if (input[x] == 'S') {
                    startX = x;
                    startY = y;
                    altitude = 0;
                }
                else if (input[x] == 'E')
                {
                    endX = x;
                    endY = y;
                    altitude = 'z' - 'a';
                }
                else
                {
                    altitude = (int)(input[x] - 'a');
                }
                line.Add(new Node() {x = x, y = y, height = altitude});
            }

            grid.Add(line);
        }

        var width = grid[0].Count;
        var height = grid.Count;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var left = x - 1;
                var right = x + 1;
                var up = y - 1;
                var down = y + 1;

                if (0 <= left)
                    grid[y][x].TryConnect(grid[y][left]);
                    
                if (right < width)
                    grid[y][x].TryConnect(grid[y][right]);

                if (0 <= up)
                    grid[y][x].TryConnect(grid[up][x]);

                if (down < height)
                    grid[y][x].TryConnect(grid[down][x]);
            }
        }

        var result = GetDistanceTable(grid, grid[startY][startX]);
        Console.WriteLine(result[endY, endX]);
	}


    public static void PartTwo()
    {
        Console.WriteLine("start input");

        var (endX, endY) = (0, 0);
        var costTable = new List<int[]>();
        var grid = new List<List<Node>>();
        while (true)
        {
            var input = Console.ReadLine();
            if (!input.Any())
                break;

            var line = new List<Node>();
            var y = grid.Count;
            for (var x = 0; x < input.Length; ++x)
            {
                var altitude = 0;
                if (input[x] == 'S') {
                    altitude = 0;
                }
                else if (input[x] == 'E')
                {
                    endX = x;
                    endY = y;
                    altitude = 'z' - 'a';
                }
                else
                {
                    altitude = (int)(input[x] - 'a');
                }
                line.Add(new Node() {x = x, y = y, height = altitude});
            }

            grid.Add(line);
        }

        var width = grid[0].Count;
        var height = grid.Count;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var left = x - 1;
                var right = x + 1;
                var up = y - 1;
                var down = y + 1;

                if (0 <= left)
                    grid[y][x].TryConnect(grid[y][left]);
                    
                if (right < width)
                    grid[y][x].TryConnect(grid[y][right]);

                if (0 <= up)
                    grid[y][x].TryConnect(grid[up][x]);

                if (down < height)
                    grid[y][x].TryConnect(grid[down][x]);
            }
        }

        var min = int.MaxValue;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                if (grid[y][x].height != 0)
                    continue;

                var result = GetDistanceTable(grid, grid[y][x]);
                if (result[endY, endX] < min)
                    min = result[endY, endX];
            }
        }
        Console.WriteLine($"{min}");
	}
}
