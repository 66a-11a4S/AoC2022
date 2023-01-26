using System.Text.RegularExpressions;

/*
day16

* 火山が噴火するまであと 30 分. 洞窟の各部屋のバルブを開き、できるだけ解放する圧力量を最大化したい
* デバイスはパイプと圧力解放バルブのネットワークを発見します
* デバイスは、各バルブの流量(1分あたりの圧力)と、バルブ間を移動するために使用できるトンネルを表示します

> Valve AA has flow rate=0; tunnels lead to valves DD, II, BB
> Valve BB has flow rate=13; tunnels lead to valves CC, AA
> ...

* 開始時、すべてのバルブは閉じている
* 開始時、あなたは AA というラベルの付いた部屋にいて、すべてのバルブは閉じている
* 1つのバルブを開くのに1分、あるバルブから別のバルブへのトンネルをたどるのに1分かかる

Q1. あなたが30分で解放できる圧力の最大値は?

Q2.

* 最適なアプローチを行っても圧力が十分でない可能性がある.  そこで1頭の象に助けを求める
* ゾウに開く場所と順番を教えるには4分かかるので、残り時間は26分
* 象と26分協力したとき、解放できる最大圧力は?
*/

public class Day16
{
    private static Dictionary<string, int> _scoreTable = new Dictionary<string, int>();

    public static void PartOne()
    {
        var roomToIdTable = new Dictionary<string, int>();
        var connections = new Dictionary<string, IReadOnlyCollection<string>>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var (roomName, score, connection) = Translate(line);
            connections[roomName] = connection;
            if (score != 0)
                _scoreTable[roomName] = score;
        }

        // TODO: puzzle によって2分くらいかかることがある
        var activatedRooms = new HashSet<string>();
        var dp = new Dictionary<string ,long>();
        dp[string.Empty] = 0;
        DFS("AA", prev: string.Empty, t: 30, state: string.Empty, connections, dp, activatedRooms);

        // 一番スコアを稼いだ経路の値を出す
        Console.Write(dp.Values.Max());
    }

    public static void PartTwo()
    {
        var roomToIdTable = new Dictionary<string, int>();
        var connections = new Dictionary<string, IReadOnlyCollection<string>>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var (roomName, score, connection) = Translate(line);
            connections[roomName] = connection;
            if (score != 0)
                _scoreTable[roomName] = score;
        }

        var activatedRooms = new HashSet<string>();
        var dp = new Dictionary<string ,long>();
        dp[string.Empty] = 0;
        DFS("AA", prev: "AA", t: 26, state: string.Empty, connections, dp, activatedRooms);

        // 2人で協力して開ける量を最大化するなら、同じ部屋を開ける必要はないはず.
        // 1人でかぶりがないように途中まで開けた結果を合算する
        var result = 0L;
        foreach (var (state1, result1) in dp)
        {
            foreach (var (state2, result2) in dp)
            {
                if (Overlap(state1, state2))
                    continue;

                result = Math.Max(result, result1 + result2);
            }
        }
        Console.WriteLine(result);

        static bool Overlap(string state1, string state2)
        {
            for (var idx1 = 0; idx1 <= state1.Length - 2; idx1 += 2)
            {
                var room = state1.Substring(startIndex: idx1, length: 2);
                if (state2.Contains(room))
                    return true;
            }

            return false;
        }
    }

    private static void DFS(string v, string prev, int t, string state,
        Dictionary<string, IReadOnlyCollection<string>> connection,
        Dictionary<string, long> dp,
        HashSet<string> activatedRooms)
    {
        if (t <= 0 || activatedRooms.Count == _scoreTable.Count)
            return;

        // バルブをあける
        var updated = Activate(state, v, activatedRooms);
        if (state != updated)
        {
            activatedRooms.Add(v);

            dp.TryGetValue(updated, out var value);
            dp[updated] = Math.Max(value, dp[state] + CalculateScore(v, t - 1));
            //Console.WriteLine($"current {current} / t {t} / score {dp[s] + CalculateScore(v, t-1)}");
            DFS(v, v, t - 1, updated, connection, dp, activatedRooms);

            activatedRooms.Remove(v);
        }

        // バルブを開けずに移動
        foreach (var c in connection[v])
        {
            // バルブを開けずに前の部屋に戻る必要はないはず
            if (c == prev)
                continue;

            DFS(c, v, t - 1, state, connection, dp, activatedRooms);
        }
    }

    private static int CalculateScore(string v, int t)
    {
        return _scoreTable.TryGetValue(v, out var score) ? score * t : 0;
    }

    private static string Activate(string current, string v, HashSet<string> activatedRooms)
    {
        // 今いる部屋を ON にしても意味がないときはスルー
        if (!_scoreTable.ContainsKey(v) || activatedRooms.Contains(v))
            return current;

        return current + v;
    }

    private static (string Name, int Rate, IReadOnlyCollection<string> Connection) Translate(string line)
    {
        var roomNamePattern = new Regex(@"([A-Z]){2,}");
        var ratePattern = new Regex(@"\d+");

        var rooms = roomNamePattern.Matches(line).Select(match => match.Value.Replace(" ", string.Empty)).ToArray();
        var roomName = rooms[0];
        var connections = rooms.Any() ? rooms.Skip(1).ToArray() : Array.Empty<string>();
        var rateString = ratePattern.Matches(line).Single().Value;
        var rate = int.Parse(rateString);
        return (roomName, rate, connections);
    }
}