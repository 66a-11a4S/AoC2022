/*
day18

* 溶岩の飛沫があなたのそばを通り過ぎるときにすばやくスキャンします
* 3Dグリッド上の 1x1x1 の立方体で溶岩滴の形状を近似し、それぞれが x、y、z 位置として与えられます
* 表面積を概算するには、別の立方体に直接接続されていない各立方体の側面の数を数えます。
* 1,1,1 と 2,1,1 のような2つの隣接する立方体のみである場合、
  各立方体は1つの側面が覆われ、5 つの側面が露出され、合計 10 側面の表面積になる

Q1. スキャンした溶岩滴の表面積は?

Q2. ブロックで囲まれた内側の空洞は表面積に含まれないようにする。このときの表面積は?
*/

public class Day18
{
    public class Node
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Z { get; init; }
        public int CalculateSurfaceCount() => 6 - connection.Count;

        private HashSet<Node> connection = new HashSet<Node>();

        public bool TryConnect(Node node)
        {
            return connection.Add(node) && node.connection.Add(this);
        }
    }

    private enum Cell
    {
        None = 0,
        Obstacle,
        Water,
    }

    public static void PartOne()
    {
        var maxX = 0;
        var maxY = 0;
        var maxZ = 0;

        var nodes = new List<Node>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var values = line.Split(',');
            var node = new Node{ X = int.Parse(values[0]), Y = int.Parse(values[1]), Z = int.Parse(values[2]) };
            nodes.Add(node);

            maxX = Math.Max(maxX, node.X);
            maxY = Math.Max(maxY, node.Y);
            maxZ = Math.Max(maxZ, node.Z);
        }

        var grid = new Node[maxX + 1, maxY + 1, maxZ + 1];
        foreach (var node in nodes)
            grid[node.X, node.Y, node.Z] = node;

        var result = 0;
        var island = new HashSet<Node>();
        foreach (var node in nodes)
        {
            var connectionCount = DFS(node, grid, island);
            result += node.CalculateSurfaceCount();
        }
        Console.WriteLine(result);
    }

    public static void PartTwo()
    {
        var maxX = 0;
        var maxY = 0;
        var maxZ = 0;

        var points = new List<(int X, int Y, int Z)>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var values = line.Split(',');
            var point = (X: int.Parse(values[0]), Y: int.Parse(values[1]), Z: int.Parse(values[2]));
            points.Add(point);

            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
            maxZ = Math.Max(maxZ, point.Z);
        }

        // DFSで回り込めるように1マス余分に確保
        var field = new Cell[maxX + 2, maxY + 2, maxZ + 2];
        foreach (var point in points)
            field[point.X, point.Y, point.Z] = Cell.Obstacle;

        FillCell(0, 0, 0, field);

        var width = field.GetLength(dimension: 0);
        var height = field.GetLength(dimension: 1);
        var depth = field.GetLength(dimension: 2);
        var nodes = new List<Node>();
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                for (var z = 0; z < depth; z++)
                {
                    if (field[x, y, z] == Cell.Water)
                        continue;

                    var node = new Node{ X = x, Y = y, Z = z };
                    nodes.Add(node);
                }
            }
        }

        var grid = new Node[maxX + 1, maxY + 1, maxZ + 1];
        foreach (var node in nodes)
            grid[node.X, node.Y, node.Z] = node;

        var result = 0;
        var island = new HashSet<Node>();
        foreach (var node in nodes)
        {
            var connectionCount = DFS(node, grid, island);
            result += node.CalculateSurfaceCount();
        }
        Console.WriteLine(result);

        static void FillCell(int x, int y, int z, Cell[,,]field)
        {
            var left = x - 1;
            var right = x + 1;
            var top = y - 1;
            var bottom = y + 1;
            var back = z - 1;
            var forward = z + 1;

            var width = field.GetLength(dimension: 0);
            var height = field.GetLength(dimension: 1);
            var depth = field.GetLength(dimension: 2);

            field[x, y, z] = Cell.Water;
            if (0 <= left)
            {   
                if (field[left, y, z] == Cell.None)
                    FillCell(left, y, z, field);
            }
            if (right < width)
            {
                if (field[right, y, z] == Cell.None)
                    FillCell(right, y, z, field);
            }

            if (0 <= top)
            {
                if (field[x, top, z] == Cell.None)
                    FillCell(x, top, z, field);
            }
            if (bottom < height)
            {
                if (field[x, bottom, z] == Cell.None)
                    FillCell(x, bottom, z, field);
            }

            if (0 <= back)
            {
                if (field[x, y, back] == Cell.None)
                    FillCell(x, y, back, field);
            }
            if (forward < depth)
            {
                if (field[x, y, forward] == Cell.None)
                    FillCell(x, y, forward, field);
            }
        }
    }

    private static int DFS(Node current, Node[,,]grid, HashSet<Node> island)
    {
        island.Add(current);

        var (x, y, z) = (current.X, current.Y, current.Z);
        var left = x - 1;
        var right = x + 1;
        var top = y - 1;
        var bottom = y + 1;
        var back = z - 1;
        var forward = z + 1;

        var width = grid.GetLength(dimension: 0);
        var height = grid.GetLength(dimension: 1);
        var depth = grid.GetLength(dimension: 2);
        var result = 0;

        if (0 <= left)
        {   
            var leftNode = grid[left, y, z];
            if (leftNode != null && current.TryConnect(leftNode))
            {
                result++;
                result += DFS(leftNode, grid, island);
            };
        }
        if (right < width)
        {
            var rightNode = grid[right, y, z];
            if (rightNode != null && current.TryConnect(rightNode))
            {
                result++;
                result += DFS(rightNode, grid, island);
            }
        }

        if (0 <= top)
        {   
            var topNode = grid[x, top, z];
            if (topNode != null && current.TryConnect(topNode))
            {
                result++;
                result += DFS(topNode, grid, island);
            }
        }
        if (bottom < height)
        {
            var bottomNode = grid[x, bottom, z];
            if (bottomNode != null && current.TryConnect(bottomNode))
            {
                result++;
                result += DFS(bottomNode, grid, island);
            }
        }

        if (0 <= back)
        {   
            var backNode = grid[x, y, back];
            if (backNode != null && current.TryConnect(backNode))
            {
                result++;
                result += DFS(backNode, grid, island);
            }
        }
        if (forward < depth)
        {
            var forwardNode = grid[x, y, forward];
            if (forwardNode != null && current.TryConnect(forwardNode))
            {
                result++;
                result += DFS(forwardNode, grid, island);
            }
        }

        return result;
    }
}
