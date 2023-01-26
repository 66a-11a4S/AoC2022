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
            BuildCubeWrapperConnection();

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

        public void Progress2(int progress)
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

        private void BuildConnections()
        {
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

        private void BuildCubeWrapperConnection()
        {
            // hard coding solution

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

            map.Progress2(progress);
            map.PrintState();

            if (rotateQueue.TryDequeue(out var rotate))
                map.Rotate(rotate);
        }

        map.PrintState();
        map.PrintScore();
    }

    private static Cell[,] CreateGrid()
    {
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
        //var commandStr = Console.ReadLine();
        var commandStr = GetPuzzleCommand();
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

    private static string GetPuzzleCommand() => "31L16R40L45R27L46R17L20R1R41L18L33R17L19R46L27R7L28R31R1L17R28R27L49R24R32L10R30L1R32L48R16R14R20R17L18L29R4L14R36R12R36R28L6L45L17L35R50R49R8L10R25L32R6L39R49R27R5L26L43R37L22L22R13L19L44L8L20R50R44L16R48L17L47L26R1R11R33L1R14L29L10R48L41R10R26L50L23R31L19L10L19R30R24L17L42R31R32R1R1L13R44L14L7R3L10R17R46L42L33R50L44L49L28R16L21L29L4L43R40L29R17L45R22R32R4R25R27R10L18L1L18R14R25L9R3R12L44R17L44R47L1L18R29R10L24R17L47R1L33L21R44R41L5L3L26L26R39R47L14R25L34R43L46L18R26L5L36R3R5L43R45R42R34L3L40R11L46R8L33R24L27L7R24R22R12L41L37R6L17L24L46L2R34L8L8L21R9R25L44L27L14L5R22R35R32R2R14L45R48R21R8L46R20R15L49R1L22L5R31L22R34L35R2L30R22R14L15R22R14R30R43R11L44R27R15R30R20L14L48R14L31R7L13R50L37R26R20R17R17L50L1R34L19R1R47R34L48L11R13R8R28R8L29L10L10R16L36R38L13L1R3R12L48L39R7L39L15L35L24R31L28L21R22R37R13R10R46R8L18R36L3R1R26L23R48L18R40R3L39L6L13R20L28L8L12R27R39L46L49R1L8R39L7L48R19L38L11L45R36R11R10L42R18R20R11R21R22R42L39R42R45L2R50R5L34R2R25R49R32L44L43L17L16R4L49R49R9R4R21R32L18L26L10R26L7L3R27L12L41R39L31R24L40L18L48L26L49R41R17R35R28L30R23L39R35L1L42R4L46L18R28R48R6L30L5L30R42R42R28L47L14L12R5L46R25L32L2R49R14R42L47R27R45R15L17L27R2L20L15R31L36R47L31R41L38L31R41L50R32L28R14L22R39L31R22L9R40L35L10R11L9L37R15L43R49R28R46L3R31L34R9L32L7R29R29L13L6R9R42L7R48R18R26L37L29R45R46L8R37L19L26L13R32R21L41L2L40L30L24L15L46R33L3R50L48R9L35R13R39L30L45R29L13L1L6R22R5R28L22L1R28R27L23R21L44L22L2L18L41L47L25R18L27R11R8R39R7L5L35L1R5R12L43R3R5R36R20L18L47L25R27R30L39L16R14L27R26R21R34R3L13L9L14R17L21L29L21R41R3R22L29L10L26R12R42L32L23L50R36R30R11R14R33L11R24R36L42R29R45R1R43L30L21L5R29R20L13L25L42R33L48R38L18R36R2R24L33R6R8R48L30R37R9L30R24R34R50R14L29L18R43R40L36L14R22R49R43L7L15L1L3L32R37L43R9R29L47L30L22L37R40L35R35R15L50R50L39L25L38R8R38R31L21L33R7L1L5L7L22L6R7R13R9L16R43L3L39R4L44R30R26L7L35L1R22L21L40L47L16R21R17R19L48R14R6R17R37R24L4L44L8L44R2L26L44L3L29R23L1R7L2L21L34R25L11L21L25L46R3L48R50R5R42R16R41R50L12R10R10L14L17L22R5R39L17L36R37R45R25L35R40R4L37L43R20L2L16R10R46L21R6R10L22L12R25R29L1L47L27R14L39R31L37R18R37L30R3L45R40L34L11R38R8R21L8R39R30R35L16R4R6R45L2L3L31R37R27R41L26L39R5L31L3L6L3L5L7L46L3L5R24R25R17L7R32R5R15L26R28L50R47R38R25R17R31R17R44L10R13R11L5L1L22L39R49R40R4R44R30R7R50R30L27L9R36R12L46R40R15L28R49R15L30L44R47L39R42L15L2L36L21L42L2L26R47R32R8L25R1R31R20R23L46R6R28R16R46R2L32L38R47L21R24L14L50R18L45R21R12L13R1L44L40L26L10R16L23L22R18R16L8R25R45L13L48R23R20R14R44L47R31L26R19R47L8L25R42L34R23L11L47L16R20L48R2L39R6L26R10L7L14R29L45L16L42R34R9R24L7R33R1L47L4L8L8R27L29L23L43R35L42R25R30R15L22L24L43R7R33L39L44R43R29L37R37L23R41L27R44R4R3R43R19L49L21L35L24L1R41R36L12R29L4L1L36L50R18R47L12L33R34L33L4L40R8L40R40L45R12L14R49L29R5R49R5L32R47L38L37L23R50R17L49L48R45L6R16L35L34R42L9R34L50L26L45L8L34R42L25L29L12L25R38R19R18R46L8L1R5R22R10L16R39L32R41R25R48R9R33L44R1R44R21L41R37R26L42L29R18R43L19L48L4L20R37R40L46L42R21L41L36R24R6L48L19L1R11L21R10R23L18R12L49R24R34L13R34R44L45R27L48R39R18L45R31L34L47R8R1L32R37R43L40R37L45R13R36L14R32R33R33R46L34R42R25L30R42R14R7R21R1R14R4L40R7L31R10L32L15R11R16L1L25L29R9R37R32L26R35L46R8L45R14R26R9R35L7R31L36L50L43L19R35L38R34R48L41L8L35L26L20R34L26R21L18R50L41L32R47L37R1L48R7L6L5R33R20R23L45R35L42R20R16R20L42L1L43R48L41R44R37R5R17R5L9R27R11L43R6R20R40L44L48R40R18L16L38L48L4L21L9L9L26L29R10R42R46R35R45L11L5R17R17R27L1R2L41L44L20R3R23L9L1R33R28R22L16L18L7R8L19L35R49L23L12R16R6L15R19L41R22R6L26R17L36L22L26L19L12R33L11L8R10R47L29R23L2R42R34L14R14R16L26R41R15R45L13R38L33R19L10L45L8L36R27R12L15L14L43L45R7R46L31R21L35L22L24R24L21R38R6R35R28R21R26R16L48R2R23L27L28L38L44R7L22L21L5R29R1R44L44R10L45R46L9L43L35R18R35L5L22L30L22L27R7R46R17R45L23R12L23R46R13R22R23L4L21R5R43L45R48R19L25R41R37L37L19L34R50R32R24R33L36R26L34L47R33R43L9L25L15L15R48R23L31R14R2R16L13L22L23R39L2R11R5R27L50L10L1R46R14L13R44L18R5R4R42L27L8L48L4L44L17L27R15R30R43R41L14R32L35R21L19R12R8L29R12L50L38R46R49R14L10L3L21R17L19L36R28R43L11R9R32R15L27L1R21R43L48R12R18L12R38R12R36R11L24L46L34L29L32R36R12L16L40L45L10L11R8R40L10L1R21L23L11R29L49L50L41L50L48L36R3R15L45L13L20L27L49L39R22R29R46R15L49L44L6L7L4R36R47R41L5R36L4L29R10R13L50R40L2R26L41L45R12R38R6R20R30R41R11L2L18L21L44R4L10R32L3L9R14R28R34L38L9L21L5L22R26R4R33R35L11R26R21R36R19R23R32R34L19L49L24L2R37L19R17R12R19R3L24L25L1R33L15L43R32L41L7R30L30R41L28R10R44R31R36R23R25R15R28L1L33L33R49L19L16L21L29R13R21L27L31R36L32L31L29R25R6L26L8L42R14R9R32R29R8L16L17R46R13R44R49R15L15L9L25R31L21L15L17R21L48R37L32R1R38L13L22L22R43R1L49L15R12R1R26R32L2R20R13L5R22R15L8L37L30R26R9L23L24R46L10L37L39L16L28R42R46R32L12R8R17R46L12R28R27L46L46R32R2L42L39L43L22R8L9R17L44L35L25L2L32L46R9L3L28L37R17R37R8R13R20L35L1L15R33R11R22L43R17L30R50R32R44L50L14R43L31L41R25L20L35R28R39L30R26L22L45R23L14R31R11R18L20R22L19R28L17R27R25L10L3L39R14L15L27L29R33L12R29R30L47R32R12L4L34R48L43R40L20L16L28L2L32R36L42L22R11R12L29L22R42R47L18L24R48R7R34L19L19R10R2L8L10R3R33R35R23R7L23R39R17L8L12R47R17R25L44R36L12R16L34R37L36R10L48L47R29R13R48R36L47R48R14R27R28R4R30R40R15R46R46R43R12R36L11L28R21L23L46R4R38L30L11R29R23L48L35L36R48L18R9R24L25R9R36L14L21L29L9L38R18R34L44L38L48L41R39R25L4R17L21R49L40L24R21R10L20L1L30R5R12L8R33R22R44L31R48L15R25L34L22R32R49L23R10R50R40L34R1L14L47L30L7L37R17R41R44L43L13L12L48R43L17L43L5R35L5R44L49R21R10R42R11L22R8R18R13R44L6L31R45R43L26R13L21L49L47L12R24L4L34R4R45R16R21L42R22L27R45R23R35R36R18R8L11R17R10L5R30R26R47L47R10R30L50R6L49R14L20R27R27L6R7L13L1L26R44L42R8R24L34L5R40R26L5L3L13L18L35L46R33R14R1L2R50L21L14R9L35R7R47R18L15R33L46R40R19R38R20R2R43L24R29R11R33R17R49R46L46R5L37L32L46R19L1L42L14R12R6L17R40L11L18R9L28R41R24R10R39L20R11L37R42R25L47R44L37L14R24R19L12L49L14L5R15R37L49L19R37L28L40L13R16L34L34L28L31R19L49L50R4L37R44R49L12L17R44R33";
}