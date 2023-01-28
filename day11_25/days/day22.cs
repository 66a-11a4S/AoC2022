using Library;
using System.Text.RegularExpressions;

/*
day22

メモの指示にそってマップを進み、到達した地点からパスワードを求める

        ...#
        .#..
        #...
        ....
...#.......#
........#...
..#....#....
..........#.
        ...#....
        .....#..
        .#......
        ......#.

10R5L5R10L4R5L5

* 地図は、開いたタイル(.)と壁(#) のセットで構成
* 移動指示は数字と文字で構成
  * 数字: 今向いている方向に移動するタイル数. 壁にぶつかると、そこで止まる
  * 文字: その場で時計回り(R)か反時計回り(L)のどちらに90度回転するか
  * 例: `10R5` は 10歩前進 -> 時計回り90度 -> 5歩前進
* マップの一番上の行の、一番左の列から、右を向いてスタート
* 進んだ先がマップの外側なら、ループして反対側から出てくる

最終的なパスワードは到達した位置の座標と向きから
Y * 1000 + X * 4 + 向き の合計値で求められる.
* 向きは 右:0 下:1 左:2 上:3

Q1. 最終的なパスワードはいくつ?

Q2.

マップは折りたたむと大きな立方体になった. 6つの面はそれぞれ 50x50の正方形

* 以前と同じ姿勢・向きでスタート. 最終的なパスワードの求め方も同じ
* マップの領域外に出るときは、代わりに立方体の隣の面のますに移動する

これで得られる最終的なパスワードはいくつ?
*/

public static class Day22
{
    private const int edgeSize = 50;

    private enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    private enum Cell
    {
        OutOfField,
        None,
        Obstacle,
    }

    private readonly struct Segment
    {
        public readonly int Start;
        public readonly int End;
        public Segment(int start, int end) => (Start, End) = (start, end);
    }

    private readonly struct Plain
    {
        public Vector2Int UpperLimit { get; }
        public Plain(Vector2Int upperLimit) => UpperLimit = upperLimit; 
    }

    private class Map
    {
        private readonly Cell[,] _grid;
        private readonly int _width, _height;

        private readonly Segment[] _vertical, _horizontal;
        private Vector2Int _current;
        private Direction _direction;

        private Dictionary<uint, Plain> _plains = new Dictionary<uint, Plain>();
        private Dictionary<(Vector2Int, Direction), (Vector2Int, Direction)> _connection = new Dictionary<(Vector2Int, Direction), (Vector2Int, Direction)>();

        public Map(Cell[,] grid)
        {
            _grid = grid;
            _direction = Direction.Right;

            _width = _grid.GetLength(dimension: 1);
            _height = _grid.GetLength(dimension: 0);

            _vertical = new Segment[_width];
            _horizontal = new Segment[_height];

            BuildSegments();

            // for PartTwo
            BuildPlains();
            //BuildSampleConnections();
            BuildPuzzleConnections();

            _current = new Vector2Int(_horizontal[0].Start, 0);
        }

        public void Rotate(bool toClockwise)
        {
            if (toClockwise)
            {
                switch (_direction)
                {
                    case Direction.Up:
                        _direction = Direction.Right;
                        break;
                    case Direction.Right:
                        _direction = Direction.Down;
                        break;
                    case Direction.Down:
                        _direction = Direction.Left;
                        break;
                    case Direction.Left:
                        _direction = Direction.Up;
                        break;
                }

                return;
            }

            switch (_direction)
            {
                case Direction.Up:
                    _direction = Direction.Left;
                    break;
                case Direction.Left:
                    _direction = Direction.Down;
                    break;
                case Direction.Down:
                    _direction = Direction.Right;
                    break;
                case Direction.Right:
                    _direction = Direction.Up;
                    break;
            }
        }

        public void Progress(int progress)
        {
            if (_direction == Direction.Up)
            {
                for (var count = 0; count < progress; count++)
                {
                    var destination = _current.Y - 1;
                    if (destination < 0 || _grid[destination, _current.X] == Cell.OutOfField)
                        destination = _vertical[_current.X].End;

                    if (_grid[destination, _current.X] == Cell.Obstacle)
                        break;

                    _current.Y = destination;
                }
                return;
            }

            if (_direction == Direction.Down)
            {
                for (var count = 0; count < progress; count++)
                {
                    var destination = _current.Y + 1;
                    if (_height <= destination || _grid[destination, _current.X] == Cell.OutOfField)
                        destination = _vertical[_current.X].Start;

                    if (_grid[destination, _current.X] == Cell.Obstacle)
                        break;

                    _current.Y = destination;
                }
                return;
            }

            if (_direction == Direction.Left)
            {
                for (var count = 0; count < progress; count++)
                {
                    var destination = _current.X - 1;
                    if (destination < 0 || _grid[_current.Y, destination] == Cell.OutOfField)
                        destination = _horizontal[_current.Y].End;

                    if (_grid[_current.Y, destination] == Cell.Obstacle)
                        break;

                    _current.X = destination;
                }
                return;
            }

            if (_direction == Direction.Right)
            {
                for (var count = 0; count < progress; count++)
                {
                    var destination = _current.X + 1;
                    if (_width <= destination || _grid[_current.Y, destination] == Cell.OutOfField)
                        destination = _horizontal[_current.Y].Start;

                    if (_grid[_current.Y, destination] == Cell.Obstacle)
                        break;

                    _current.X = destination;
                }
                return;
            }
        }

        public void ProgressPartTwo(int progress)
        {
            for (var count = 0; count < progress; count++)
            {
                var destination = _current + GetFrontVec(_direction);
                var direction = _direction;
                if (destination.X < 0 || _width <= destination.X ||
                    destination.Y < 0 || _height <= destination.Y ||
                    _grid[destination.Y, destination.X] == Cell.OutOfField)
                    (destination, direction) = _connection[(_current, _direction)];

                if (_grid[destination.Y, destination.X] == Cell.Obstacle)
                    break;

                _direction = direction;
                _current = destination;
            }

            static Vector2Int GetFrontVec(Direction dir)
            {
                switch (dir)
                {
                    case Direction.Up: return new Vector2Int(0, -1);
                    case Direction.Right: return new Vector2Int(1, 0);
                    case Direction.Down: return new Vector2Int(0, 1);
                    case Direction.Left: return new Vector2Int(-1, 0);
                    default: throw new ArgumentOutOfRangeException(nameof(dir));
                }
            }
        }

        private void BuildSegments()
        {
            for (var x = 0; x < _width; x++)
            {
                var start = -1;
                var end = 0;

                for (var y = 0; y < _height; y++)
                {
                    var value = _grid[y, x];
                    if (value != Cell.OutOfField)
                    {
                        end = y;
                        if (start == -1)
                            start = y;
                    }
                }

                _vertical[x] = new Segment(start, end);
            }

            for (var y = 0; y < _height; y++)
            {
                var start =  -1;
                var end = 0;

                for (var x = 0; x < _width; x++)
                {
                    var value = _grid[y, x];
                    if (value != Cell.OutOfField)
                    {
                        end = x;
                        if (start == -1)
                            start = x;
                    }
                }

                _horizontal[y] = new Segment(start, end);
            }
        }

        private void BuildPlains()
        {
            var id = 1u;
            for (var y = 0; y < _height; y += edgeSize)
            {
                for (var x = 0; x < _width; x += edgeSize)
                {
                    if (_grid[y, x] == Cell.OutOfField)
                        continue;

                    _plains[id] = new Plain(new Vector2Int(x, y));
                    id++;
                }
            }
        }

        private void BuildSampleConnections()
        {
            // hard coded connections

            // 1 <-> 2
            for (var x = 0; x < edgeSize; x++)
            {
                var upperLimit = _plains[1].UpperLimit;
                var from = upperLimit + new Vector2Int(x, 0); // 上辺
                var to = _plains[2].UpperLimit + new Vector2Int(edgeSize - 1 - x, 0); // 上辺
                _connection[(from, Direction.Up)] = (to, Direction.Down);
                _connection[(to, Direction.Up)] = (from, Direction.Down);
            }

            // 1 <-> 3
            for (var y = 0; y < edgeSize; y++)
            {
                var upperLimit = _plains[1].UpperLimit;
                var from = upperLimit + new Vector2Int(0, y); // 左辺
                var to = _plains[3].UpperLimit + new Vector2Int(y, 0); // 上辺
                _connection[(from, Direction.Left)] = (to, Direction.Down);
                _connection[(to, Direction.Up)] = (from, Direction.Right);
            }

            // 1 <-> 6
            for (var y = 0; y < edgeSize; y++)
            {
                var upperLimit = _plains[1].UpperLimit;
                var from = upperLimit + new Vector2Int(edgeSize - 1, y); // 右辺
                var to = _plains[6].UpperLimit + new Vector2Int(edgeSize - 1, edgeSize - 1 - y); // 右辺
                _connection[(from, Direction.Right)] = (to, Direction.Left);
                _connection[(to, Direction.Right)] = (from, Direction.Left);
            }

            // 2 <-> 5
            for (var x = 0; x < edgeSize; x++)
            {
                var upperLimit = _plains[2].UpperLimit;
                var from = upperLimit + new Vector2Int(x, edgeSize - 1); // 底辺
                var to = _plains[5].UpperLimit + new Vector2Int(edgeSize - 1 - x, edgeSize - 1); // 底辺
                _connection[(from, Direction.Down)] = (to, Direction.Up);
                _connection[(to, Direction.Down)] = (from, Direction.Up);
            }

            // 2 <-> 6
            for (var y = 0; y < edgeSize; y++)
            {
                var upperLimit = _plains[2].UpperLimit;
                var from = upperLimit + new Vector2Int(0, y); // 左辺
                var to = _plains[6].UpperLimit + new Vector2Int(edgeSize - 1 - y, edgeSize - 1); // 底辺
                _connection[(from, Direction.Left)] = (to, Direction.Up);
                _connection[(to, Direction.Down)] = (from, Direction.Right);
            }

            // 3 <-> 5
            for (var x = 0; x < edgeSize; x++)
            {
                var upperLimit = _plains[3].UpperLimit;
                var from = upperLimit + new Vector2Int(x, edgeSize - 1); // 底辺
                var to = _plains[5].UpperLimit + new Vector2Int(0, edgeSize - 1 - x); // 左辺
                _connection[(from, Direction.Down)] = (to, Direction.Left);
                _connection[(to, Direction.Right)] = (from, Direction.Up);
            }

            // 4 <-> 6
            for (var y = 0; y < edgeSize; y++)
            {
                var upperLimit = _plains[4].UpperLimit;
                var from = upperLimit + new Vector2Int(edgeSize - 1, y);	// 右辺
                var to = _plains[6].UpperLimit + new Vector2Int(edgeSize - 1 - y, 0); // 上辺
                _connection[(from, Direction.Right)] = (to, Direction.Down);
                _connection[(to, Direction.Up)] = (from, Direction.Left);
            }
        }

        private void BuildPuzzleConnections()
        {
            // hard coded connections

            // 1 <-> 4
            for (var y = 0; y < edgeSize; y++)
            {
                var upperLimit = _plains[1].UpperLimit;
                var from = upperLimit + new Vector2Int(0, y); // 左辺
                var to = _plains[4].UpperLimit + new Vector2Int(0, edgeSize - 1 - y); // 左辺
                _connection[(from, Direction.Left)] = (to, Direction.Right);
                _connection[(to, Direction.Left)] = (from, Direction.Right);
            }

            // 1 <-> 6
            for (var x = 0; x < edgeSize; x++)
            {
                var upperLimit = _plains[1].UpperLimit;
                var from = upperLimit + new Vector2Int(x, 0); // 上辺
                var to = _plains[6].UpperLimit + new Vector2Int(0, x); // 左辺
                _connection[(from, Direction.Up)] = (to, Direction.Right);
                _connection[(to, Direction.Left)] = (from, Direction.Down);
            }

            // 2 <-> 3
            for (var x = 0; x < edgeSize; x++)
            {
                var upperLimit = _plains[2].UpperLimit;
                var from = upperLimit + new Vector2Int(x, edgeSize - 1); // 底辺
                var to = _plains[3].UpperLimit + new Vector2Int(edgeSize - 1, x); // 右辺
                _connection[(from, Direction.Down)] = (to, Direction.Left);
                _connection[(to, Direction.Right)] = (from, Direction.Up);
            }

            // 2 <-> 5
            for (var y = 0; y < edgeSize; y++)
            {
                var upperLimit = _plains[2].UpperLimit;
                var from = upperLimit + new Vector2Int(edgeSize - 1, y); // 右辺
                var to = _plains[5].UpperLimit + new Vector2Int(edgeSize - 1, edgeSize - 1 - y); // 右辺
                _connection[(from, Direction.Right)] = (to, Direction.Left);
                _connection[(to, Direction.Right)] = (from, Direction.Left);
            }

            // 2 <-> 6
            for (var x = 0; x < edgeSize; x++)
            {
                var upperLimit = _plains[2].UpperLimit;
                var from = upperLimit + new Vector2Int(x, 0); // 上辺
                var to = _plains[6].UpperLimit + new Vector2Int(x, edgeSize - 1); // 底辺
                _connection[(from, Direction.Up)] = (to, Direction.Up);
                _connection[(to, Direction.Down)] = (from, Direction.Down);
            }

            // 3 <-> 4
            for (var y = 0; y < edgeSize; y++)
            {
                var upperLimit = _plains[3].UpperLimit;
                var from = upperLimit + new Vector2Int(0, y); // 左辺
                var to = _plains[4].UpperLimit + new Vector2Int(y, 0); // 上辺
                _connection[(from, Direction.Left)] = (to, Direction.Down);
                _connection[(to, Direction.Up)] = (from, Direction.Right);
            }

            // 5 <-> 6
            for (var x = 0; x < edgeSize; x++)
            {
                var upperLimit = _plains[5].UpperLimit;
                var from = upperLimit + new Vector2Int(x, edgeSize - 1);	// 底辺
                var to = _plains[6].UpperLimit + new Vector2Int(edgeSize - 1, x); // 右辺
                _connection[(from, Direction.Down)] = (to, Direction.Left);
                _connection[(to, Direction.Right)] = (from, Direction.Up);
            }
        }

        public void PrintState() => Console.WriteLine($"x: {_current.X}, y: {_current.Y}, dir: {_direction}");

        public void PrintScore()
        {
            var x = _current.X + 1;
            var y = _current.Y + 1;
            var direction = GetDirectionScore(_direction);
            Console.WriteLine(y * 1000 + x * 4 + direction);

            static int GetDirectionScore(Direction dir)
            {
                switch (dir)
                {
                    case Direction.Up: return 3;
                    case Direction.Right: return 0;
                    case Direction.Left: return 2;
                    case Direction.Down: return 1;

                    default: throw new ArgumentOutOfRangeException(nameof(dir));
                }
            }
        }
    }

    public static void PartOne()
    {
        var grid = CreateGrid();
        var map = new Map(grid);

        var (progressQueue, rotateQueue) = ReadCommands();
        while (true)
        {
            if (!progressQueue.TryDequeue(out var progress))
                break;

            map.Progress(progress);

            if (rotateQueue.TryDequeue(out var rotate))
                map.Rotate(rotate);
        }

        map.PrintState();
        map.PrintScore();
    }

    public static void PartTwo()
    {
        var grid = CreateGrid();
        var map = new Map(grid);

        var (progressQueue, rotateQueue) = ReadCommands();
        while (true)
        {
            if (!progressQueue.TryDequeue(out var progress))
                break;

            map.ProgressPartTwo(progress);
            map.PrintState();

            if (rotateQueue.TryDequeue(out var rotate))
                map.Rotate(rotate);
        }

        map.PrintState();
        map.PrintScore();
    }

    private static Cell[,] CreateGrid()
    {
        Console.WriteLine("input grid");
        var mapString = new List<string>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            mapString.Add(line);
        }

        var width = mapString.Select(str => str.Length).Max();
        var height = mapString.Count;
 
        var grid = new Cell[height, width];
        for (var y = 0; y < height; ++y)
        {
            var actualWidth = Math.Min(mapString[y].Length, width);
            for (var x = 0; x < actualWidth; ++x)
            {
                switch (mapString[y][x])
                {
                    case '.':
                        grid[y, x] = Cell.None;
                        break;
                    case '#':
                        grid[y, x] = Cell.Obstacle;
                        break;
                }
            }
        }
        return grid;
    }

    private static (Queue<int>, Queue<bool>) ReadCommands()
    {
        var commandStr = Console.ReadLine();
        var valuePattern = new Regex(@"\d+");
        var progresses = valuePattern.Matches(commandStr).Select(match => int.Parse(match.Value));
        var alphabetPattern = new Regex(@"(L|R)+");
        var rotates = alphabetPattern.Matches(commandStr).Select(match => match.Value == "R" ? true : false);   // is clockWise or not

        var progressQueue = new Queue<int>();
        foreach (var progress in progresses)
            progressQueue.Enqueue(progress);

        var rotateQueue = new Queue<bool>();
        foreach (var rotate in rotates)
            rotateQueue.Enqueue(rotate);

        return (progressQueue, rotateQueue);
    }
}
