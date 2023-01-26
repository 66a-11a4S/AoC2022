/*
day14

* 砂が洞窟に流れ込んできた.  頭上にある洞窟の2次元の垂直スライスをスキャンする

  * スキャンは、各岩構造のパスをトレースし、パスの形状を形成する (x, y) 座標を表示
  * x は右への距離を表し、y は下への距離を表す
  * 各パスが 1 行のテキストとして表示される
  * 各パスの最初のポイントの後、各ポイントは、前のポイントから引かれる水平または垂直の直線の終点を示す
  * 498,4 -> 498,6 -> 496,6

* 砂は 500, 0 から流れ込んでくる
* 砂は一度に 1 ユニットずつ生成され、次の砂のユニットは、前の砂のユニットが停止するまで生成されない
* 砂は1度に1タイルを満たす
  * 砂タイルは可能であれば1段下に落ちる
  * 下のタイルが(岩や砂によって)ブロックされている場合、1つ斜め左下に移動
  * そこもブロックされている場合、1つ右下に移動
  * 3つの方向がブロックされた場合、砂のユニットは停止. その時点で次の砂がソースに生成される

* 砂をシミュレートしていくとある時点でそれ以上砂が溜まらなくなる

Q1. その時点で砂はいくつ溜まっているか?

Q2.

* 床は y の最大値 + 2 の位置に広がる無限平面と仮定
* 安全な場所をみつけるために、この状態で砂が最大まで溜まった状態をシミュレートする
* これ以上たまらなくなったとき、砂はいくつ溜まってる?
*/

public class Day14
{
    private enum Cell
    {
        None,
        Block,
        Sand
    }

    const int height = 200;
    const int width = 701; // 砂がx=500から流れ、最大で左右にheightマス分広がる

    public static void PartOne()
    {
        var grid = new Cell[height, width];

        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var (segmentsX, segmentsY) = TranslateToPath(line);
            for (var idx = 0; idx < segmentsX.Count - 1; idx++)
            {
                var fromX = segmentsX[idx];
                var fromY = segmentsY[idx];
                var toX = segmentsX[idx + 1];
                var toY = segmentsY[idx + 1];

                if (toX < fromX)
                {
                    var temp = fromX;
                    fromX = toX;
                    toX = temp;
                }

                if (toY < fromY)
                {
                    var temp = fromY;
                    fromY = toY;
                    toY = temp;
                }

                for (var y = fromY; y <= toY; y++)
                    for (var x = fromX; x <= toX; x++)
                        grid[y, x] = Cell.Block;
            }
        }

        var result = FillSand(grid);
        Console.WriteLine(result);
    }

    public static void PartTwo()
    {
        var grid = new Cell[height, width];
        var maxY = 0;

        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var (segmentsX, segmentsY) = TranslateToPath(line);
            for (var idx = 0; idx < segmentsX.Count - 1; idx++)
            {
                var fromX = segmentsX[idx];
                var fromY = segmentsY[idx];
                var toX = segmentsX[idx + 1];
                var toY = segmentsY[idx + 1];

                if (toX < fromX)
                {
                    var temp = fromX;
                    fromX = toX;
                    toX = temp;
                }

                if (toY < fromY)
                {
                    var temp = fromY;
                    fromY = toY;
                    toY = temp;
                }

                for (var y = fromY; y <= toY; y++)
                    for (var x = fromX; x <= toX; x++)
                        grid[y, x] = Cell.Block;

                foreach (var y in segmentsY)
                {
                    if (maxY < y)
                        maxY = y;
                }
            }
        }

        for (var x = 0; x < width; x++)
            grid[maxY + 2, x] = Cell.Block;

        var result = FillSand(grid);
        Console.WriteLine(result);
    }

    private static int FillSand(Cell[,] grid)
    {
        var prevSandX = 0;
        var prevSandY = 0;
        var step = 0;

        while (true)
        {
            var sandX = 500;
            var sandY = 0;
            while (true)
            {
                // 下に移動できるか
                if (height - 1 <= sandY)
                    return step;

                if (grid[sandY + 1, sandX] == Cell.None)
                {
                    sandY++;
                    continue;
                }

                // 左下に移動できるか
                if (sandX < 0)
                    return step;

                if (grid[sandY + 1, sandX - 1] == Cell.None)
                {
                    sandY++;
                    sandX--;
                    continue;
                }

                // 右下に移動できるか
                if (width - 1 <= sandX)
                    return step;

                if (grid[sandY + 1, sandX + 1] == Cell.None)
                {
                    sandY++;
                    sandX++;
                    continue;
                }

                break;
            }

            if (prevSandX == sandX && prevSandY == sandY)
                return step;

            grid[sandY, sandX] = Cell.Sand;
            prevSandX = sandX;
            prevSandY = sandY;
            step++;
        }
    }

    private static (IReadOnlyList<int>, IReadOnlyList<int>) TranslateToPath(string line)
    {
        var resultX = new List<int>();
        var resultY = new List<int>();
        var segments = line.Split(" -> ");
        foreach (var segment in segments)
        {
            var values = segment.Split(",");
            var x = int.Parse(values[0]);
            var y = int.Parse(values[1]);
            resultX.Add(x);
            resultY.Add(y);
        }

        return (resultX, resultY);
    }
}