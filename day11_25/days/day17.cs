using Library;
/*
day17

* トンネルは最終的に非常に高くて狭い部屋に通じているます
* 大きな奇妙な形の岩が上から部屋に落ちてくる
* 5種類の岩はそれぞれ形が異なる. # は岩、 は .は空きスペース:

####

.#.
###
.#.

..#
..#
###

#
#
#
#

##
##


* 岩は↑の順番で落ちてくる. 6番目以降はループ
* 岩は回転しない
* 岩は `<`(左) や `>`(右) 方向に移動する
  * この移動パターンは連続して与えられ、最後までいくとループする

>>><<><>><<<>><>>><<<>>><<<><<<>><>><<>>

* 洞窟は横幅7マス
* 岩はユニットの左端が左壁から2マス離れる位置からスタート
* 岩は現在一番高く積み上がった位置の4マス上がユニットの一番下になる位置からスタート
* 移動パターンにそって1マス左右に動いてから、岩は1マス落ちる
* 岩が下に移動できなくなったらそこで落下終了し、次の岩が落ちる

Q1. 2022 個の岩が落下し終わったとき、一番高い位置の高さは?

Q2. 1_000_000_000_000 個の岩が落下し終わったとき、一番高い位置の高さは?
*/


public class Day17
{
    private class Block
    {
        public bool[,] Shape { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyCollection<Vector2Int> Points { get; }

        public Block(bool[,] shape)
        {
            Shape = shape;
            Width = shape.GetLength(1);
            Height = shape.GetLength(0);

            var points = new List<Vector2Int>();
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (shape[y, x])
                        points.Add(new Vector2Int(x, y));
                }
            }
            Points = points;
        }
    }

    private class StateCache
    {
        public long Height { get; init; }
        public long Step { get; init; }
    }

    private const int MaxWidth = 7;

    private static long _simulationTime = 0;
    private static List<bool[]> _grid = new List<bool[]>();

    public static void PartOne()
    {
        var (blockPatterns, movePatterns) = ReadInput();

        var highest = -1L;

        for (var count = 0; count < 2022; count++)
        {
            var block = blockPatterns[count % blockPatterns.Count];
            var start = new Vector2Int(2, (int)(highest + 4));  // 一番下の3マス上, 左から2マス目ににブロックの原点がくるようにスタート

            highest = Simulate(start, block, highest, movePatterns);
            //PrintState(highest);
            //Console.WriteLine($"{count} / {highest}");
            //Console.WriteLine();
        }

        PrintState(highest);
        Console.WriteLine(highest + 1);
    }

    public static void PartTwo()
    {
        var (blockPatterns, movePatterns) = ReadInput();

        var highest = -1L;
        const long maxSteps = 1000000000000L;

        var heightHistory = new List<long>();
        var topStateHistory = new List<int>();
        var patternLengthHistory = new List<int>();

        for (var step = 0L; step < maxSteps; step++)
        {
            highest = Stack(step, highest, blockPatterns, movePatterns);
            heightHistory.Add(highest);
            topStateHistory.Add(CalculateTopLineState(highest));

            // 一番高い行の履歴のうち最長共通の区間長を出す
            var duration = CalculatePatternLength(topStateHistory);
            //Console.WriteLine($"{step} / {duration} / {highest}");
            patternLengthHistory.Add(duration);

            // 短い周期は偶然部分的に同じ行が現れた場合なので足切り
            // TODO: 適切な周期長を求める方法は?
            if (duration < 30)
                continue;

            // 1周期前と同じなら
            if (patternLengthHistory[(int)step] == patternLengthHistory[(int)step - duration])
            {
                // 1周期分の高さ
                var heightPerDuration = highest - heightHistory[(int)step - duration];

                // このforループ内ではすでにブロックを置いたのでカウントアップ
                step += 1;

                // 1周期繰り返せなくなったあと、maxSteps に到達するまで残りの手数を積む
                var restSteps = maxSteps - step;
                var restOfDuration = restSteps % duration;
                for (var j = 0; j < restOfDuration; j++)
                    highest = Stack(step + j, highest, blockPatterns, movePatterns);

                // 1周期分の高さを、周期を繰り返せるだけ追加
                var repeatedTotalHeight = heightPerDuration * (restSteps / duration);
                highest += repeatedTotalHeight;
                //Console.WriteLine($"heightPerDuration {heightPerDuration} / repeat times { (restSteps / duration) - 1 }");
                break;
            }
        }

        Console.WriteLine(highest + 1);

        static long Stack(long step, long highest, IReadOnlyList<Block> blockPatterns, IReadOnlyList<int> movePatterns)
        {
            var block = blockPatterns[(int)(step % blockPatterns.Count)];
            var start = new Vector2Int(2, (int)(highest + 4));  // 一番下の3マス上, 左から2マス目ににブロックの原点がくるようにスタート
            return Simulate(start, block, highest, movePatterns);
        }
    }

    private static long Simulate(Vector2Int start, Block block, long highest, IReadOnlyList<int> movePatterns)
    {
        var lackOfHeight = start.Y - highest;
        for (var count = 0; count < lackOfHeight; count++)
            _grid.Add(new bool[MaxWidth]);

        var position = new Vector2Int(start.X, start.Y);
        while (true)
        {
            var moveX = movePatterns[(int)(_simulationTime % movePatterns.Count)];
            var canMoveX = true;
            if (moveX < 0)
            {
                foreach (var point in block.Points)
                {
                    var simulatePoint = point + position;
                    simulatePoint.X += moveX;
                    if (simulatePoint.X < 0 || _grid[simulatePoint.Y][simulatePoint.X])
                    {
                        canMoveX = false;
                        break;
                    }
                }
            }
            else if (0 < moveX)
            {
                foreach (var point in block.Points)
                {
                    var simulatePoint = point + position;
                    simulatePoint.X += moveX;
                    var rightX = simulatePoint.X;
                    if (MaxWidth <= rightX || _grid[simulatePoint.Y][rightX])
                    {
                        canMoveX = false;
                        break;
                    }
                }
            }

            if (canMoveX)
                position.X += moveX;


            var moveY = -1;
            var canMoveY = true;
            foreach (var point in block.Points)
            {
                var simulatePoint = point + position;
                simulatePoint.Y += moveY;
                var bottomY = simulatePoint.Y;
                if (bottomY < 0 || _grid[bottomY][simulatePoint.X])
                {
                    canMoveY = false;
                    break;
                }
            }

            _simulationTime++;
            if (canMoveY)
                position.Y += moveY;
            else
                break;
        }

        // 積み上がる
        for (var y = 0; y < block.Height; y++)
        {
            for (var x = 0; x < block.Width; x++)
            {
                _grid[position.Y + y][position.X + x] |= block.Shape[y, x];
                if (highest < position.Y + y)
                    highest = position.Y + y;
            }
        }

        return highest;
    }


    private static int CalculateTopLineState(long height)
    {
        var lineHash = 0;
        var width = _grid[0].Length;
        for (var x = 0; x < width; x++) 
        {
            if (_grid[(int)(height)][x])
                lineHash |= 1 << x;
        }
        return lineHash;
    }

    private static int CalculatePatternLength(IReadOnlyList<int> lineStates)
    {
        var result = 0;
        for (var i = 0; i < lineStates.Count; ++i)
        {
            // i文字目からの部分配列
            var subStr = lineStates.Skip(i).Take(lineStates.Count - i).ToArray();
            var zResult = ZAlgorithm(subStr);

            //j文字目までの最長共通接頭辞長を求める
            for (var j = 0; j < subStr.Length; ++j)
                result = Math.Max(result, Math.Min(j, zResult[j]));
        }

        return result;

        static IReadOnlyList<int> ZAlgorithm(IReadOnlyList<int> s)
        {
            var z = new int[s.Count];
            z[0] = s.Count;

            var index = 1;
            var matched = 0;
            while (index < s.Count)
            {
                while (index + matched < s.Count && s[matched] == s[index + matched])
                    matched++;

                z[index] = matched;
                if (matched == 0)
                {
                    index++;
                    continue;
                }

                int copyLength = 1;
                while (copyLength < matched && copyLength + z[copyLength] < matched)
                {
                    z[index + copyLength] = z[copyLength];
                    copyLength++;
                }
                index += copyLength;
                matched -= copyLength;
            }

            return z;
        }
    }

    private static (IReadOnlyList<Block>, IReadOnlyList<int>) ReadInput()
    {
        Console.WriteLine("input block");
 
        var blockPatterns = new List<Block>();
        while (true)
        {
            var lines = new List<string>();
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;
                lines.Add(line);
            }

            if (!lines.Any())
                break;

            var shape = new bool[lines.Count, lines[0].Length];
            for (var y = 0; y < lines.Count; y++)
            {
                for (var x = 0; x < lines[0].Length; x++)
                {
                    shape[lines.Count - 1 - y, x] = lines[y][x] == '#';
                }
            }
            blockPatterns.Add(new Block(shape));
        }

        Console.WriteLine("input move patterns");
        var movePatterns = Console.ReadLine().Select(c => 
        {
            switch (c)
            {
                case '<': return -1;
                case '>': return 1;
                default: throw new ArgumentOutOfRangeException();
            }
        }).ToArray();

        return (blockPatterns, movePatterns);
    }

    private static void PrintState(long highest)
    {
        for (var y = (int)highest + 3; -1 <= y; y--)
        {
            for (var x = 0; x < MaxWidth; x++)
            {
                if (y == -1)
                    Console.Write('*');
                else
                    Console.Write(_grid[y][x] ? '#' : '.');
            }
            Console.WriteLine();
        }
    }
}