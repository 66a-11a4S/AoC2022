using System.Text.RegularExpressions;
using Library;

/*
day15

センサーは適切な読み取りができると思われるスポットを見つけると、最も近い信号源ビーコンの監視を開始する

* センサーとビーコンは常に整数座標に存在
* センサーは、マンハッタン距離で測定された最も近い1つのビーコンにのみロックオンできる
* 2つのビーコンからセンサーまでの距離が同じになることはない

> Sensor at x=2, y=18: closest beacon is at x=-2, y=15
> Sensor at x=9, y=16: closest beacon is at x=10, y=16
> ...

ビーコンが1つの列上に配置できない可能性のある位置を数える

Q1. y=2_000_000 の行で、ビーコンを配置できない位置はいくつ?

Q2.

* 遭難ビーコンの x 座標と y 座標はそれぞれ 0 以上 4_000_000 以下である
* 遭難ビーコンの信号を分離するには `同調周波数` を決定する.
  * これは x * 4000000 + y で求められる

遭難ビーコンは、センサーが探索していない唯一の位置にある。この位置の同調周波数はいくつ?

*/

public class Day15
{
    private class Sensor
    {
        public Vector2Int SelfPosition { get; }
        public Vector2Int DetectedBeaconPosition { get; }

        public int SearchDistance { get; }

        public Sensor(Vector2Int self, Vector2Int beacon)
        {
            SelfPosition = self;
            DetectedBeaconPosition = beacon;
            SearchDistance = ManhattanDistance(self, beacon);
        }

        public (int FromX, int ToX) CalculateSearchRange(int y)
        {
            var diffY = y - SelfPosition.Y;
            var diffX = SearchDistance - Math.Abs(diffY);
            return (SelfPosition.X - diffX, SelfPosition.X + diffX);
        }
    }

    private static int ManhattanDistance(Vector2Int v, Vector2Int v2)
    {
        var diffX = Math.Abs(v2.X - v.X);
        var diffY = Math.Abs(v2.Y - v.Y);
        return diffX + diffY;
    }

    private static Sensor Translate(string line)
    {
        var pattern = new Regex($@"(x|y)+(=)(-)?(\d)+");
        var matches = pattern.Matches(line);
        var x = new List<int>(2);
        var y = new List<int>(2);
        foreach (Match match in matches)
        {
            if (match.Value.Contains("x="))
            {
                x.Add(int.Parse(match.Value.Replace("x=", "")));
                continue;
            }

            if (match.Value.Contains("y="))
            {
                y.Add(int.Parse(match.Value.Replace("y=", "")));
                continue;
            }
            throw new InvalidOperationException("不正な座標");
        }

        return new Sensor(new Vector2Int(x[0], y[0]), new Vector2Int(x[1], y[1]));
    }

    private static Vector2Int CalculateDistressSpot(IReadOnlyCollection<Sensor> sensors, int scanHeight)
    {
        for (var y = 0; y <= scanHeight; y++)
        {
            var segments = sensors
                .Select(sensor => sensor.CalculateSearchRange(y))
                .OrderBy(segment => segment.FromX)
                .ThenBy(segment => segment.ToX)
                .ToArray();

            var mainLine = segments[0];
            for (var idx = 1; idx < segments.Length - 1; idx++)
            {
                var (mergedFrom, mergedTo, success) = MergeSegment(mainLine.FromX, mainLine.ToX, segments[idx].FromX, segments[idx].ToX);
                if (!success)
                    return new Vector2Int(mainLine.ToX + 1, y);

                mainLine = (mergedFrom, mergedTo);
            }
        }

        return new Vector2Int(0, 0);
    }

    private static (int, int, bool) MergeSegment(int from1, int to1, int from2, int to2)
    {
        if (from2 <= from1 && from1 <= to2)
            return (from2, Math.Max(to1, to2), true);

        if (from2 <= to1 && to1 <= to2)
            return (Math.Min(from1, from2), to2, true);

        if (from1 <= from2 && from2 <= to1)
            return (from1, Math.Max(to1, to2), true);

        if (from1 <= to2 && to2 <= to1)
            return (Math.Min(from1, from2), to1, true);

        return (0, 0, false);
    }

    public static void PartOne()
    {
        const int SearchHeight = 2000000;
        var sensors = new List<Sensor>();
 
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var sensor = Translate(line);
            sensors.Add(sensor);
        }

        var result = new HashSet<int>();
        foreach (var sensor in sensors)
        {
            var (fromX, toX) = sensor.CalculateSearchRange(SearchHeight);
            for (var x = fromX; x <= toX; x++)
                result.Add(x);
        }

        foreach (var sensor in sensors)
        {
            if (sensor.SelfPosition.Y == SearchHeight)
                result.Remove(sensor.SelfPosition.X);

            if (sensor.DetectedBeaconPosition.Y == SearchHeight)
                result.Remove(sensor.DetectedBeaconPosition.X);
        }

        Console.WriteLine(result.Count);
    }

    public static void PartTwo()
    {
        var availableRange = 4000000;
        var sensors = new List<Sensor>(); 
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var sensor = Translate(line);
            sensors.Add(sensor);
        }

        var spot = CalculateDistressSpot(sensors, availableRange);
        Console.WriteLine(spot.X * 4000000L + spot.Y);
    }

    private static void PrintState(IReadOnlyCollection<Sensor> sensors)
    {
        var min = new Vector2Int(int.MaxValue, int.MaxValue);
        var max = new Vector2Int(int.MinValue, int.MinValue);
        foreach (var sensor in sensors)
        {
            if (sensor.SelfPosition.X < min.X)
                min.X = sensor.SelfPosition.X;
            if (sensor.DetectedBeaconPosition.X < min.X)
                min.X = sensor.DetectedBeaconPosition.X;

            if (max.X < sensor.SelfPosition.X)
                max.X = sensor.SelfPosition.X;
            if (max.X < sensor.DetectedBeaconPosition.X)
                max.X = sensor.DetectedBeaconPosition.X;

            if (sensor.SelfPosition.Y < min.Y)
                min.Y = sensor.SelfPosition.Y;
            if (sensor.DetectedBeaconPosition.Y < min.Y)
                min.Y = sensor.DetectedBeaconPosition.Y;

            if (max.Y < sensor.SelfPosition.Y)
                max.Y = sensor.SelfPosition.Y;
            if (max.Y < sensor.DetectedBeaconPosition.Y)
                max.Y = sensor.DetectedBeaconPosition.Y;
        }

        var width = max.X - min.X;
        var height = max.Y - min.Y;
        var grid = new char[height + 1, width + 1];

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                grid[y, x] = '.';
            }
        }

        foreach (var sensor in sensors)
        {
            var sensorPos = sensor.SelfPosition - min;
            var beaconPos = sensor.DetectedBeaconPosition - min;

            var searchMin = Math.Max(0, sensorPos.Y - sensor.SearchDistance);
            var searchMax = Math.Min(height, sensorPos.Y + sensor.SearchDistance);
            for (var y = searchMin; y <= searchMax; y++)
            {
                var (from, to) = sensor.CalculateSearchRange(y);
                // 0,0 起点と min 起点のズレを直す
                from -= min.X;
                to -= min.X;
                for (var x = from; x <= to; x++)
                {
                    if (0 <= x && x < width)
                        grid[y, x] = '#';
                }
            }

            grid[sensorPos.Y, sensorPos.X] = 'S';
            grid[beaconPos.Y, beaconPos.X] = 'B';
        }

        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                Console.Write(grid[y, x]);
            }
            Console.WriteLine();
        }
    }
}