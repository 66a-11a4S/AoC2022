/*
day 13

遭難信号からのパケットが順不同でデコードされた

* リストはパケットのペアで構成され、ペアは空白行で区切られる
  * パケットは、データ列と整数で構成.
  * データ列は [ で始まり、] で終わり、0 個以上のコンマ区切りの整数orデータ列で構成
  * パケットは常にデータ列であり、それぞれの行に表示される

正しい順序になっているパケットのペアの数を特定する必要がある.
* 2つの値を比較する場合、最初の値を左、2番目の値を右とする

1. 両方の値が整数の場合は、小さい方の整数が最初に来る必要がある
2. 両方の値がデータ列ならば、各データ列の最初の値を比較し、次に2番目の値、というように比較
  * 左側のリストが最初に項目を使い果たした場合、入力は正しい順序になっています
3. ちょうど1つの値が整数である場合、その整数を唯一の値として含むデータ列に変換してから比較
  * [0,0,0] と 2 を比較する場合、[2]に変換して比較

ペアのインデクスを1, 2, ... とすると

Q1. 正しい順序になってるペアのインデクスの合計はいくつ?

Q2.

遭難信号プロトコルには、以下の2つのパケットを含める

[[2]]
[[6]]

* 前と同じルールですべてのパケット(受信したパケット + 2つの分割パケット) を正しい順序に編成する
* 整列後の分割パケットのインデックス2つを乗算した値は?
*/

public class Day13
{
    public interface Item
    {
        int IsRightOrder(Value value);
        int IsRightOrder(Container container);
    }

    public class Value : Item
    {
        public int Content { get; }
        public Value(int value) => Content = value;

        // -1: 相手 は 自分 より小さい
        //  1: 相手 は 自分 より大きい
        public int IsRightOrder(Value value) => value.Content.CompareTo(Content);

        // -1 : 相手の先頭の値が 自分より小さい
        //  1 : 相手の先頭の値が 自分より大きい
        public int IsRightOrder(Container container)
        {
            var temp = new Container();
            temp.Content.Add(this);
            return temp.IsRightOrder(container);
        }
    }

    public class Container : Item
    {
        public List<Item> Content { get; private set; } = new List<Item>();

        // -1: 自分 の先頭の値が 相手 より大きい
        //  1: 自分 の先頭の値が 相手 より小さい
        public int IsRightOrder(Value value)
        {
            if (!Content.Any())
                return 1;

            var temp = new Container();
            temp.Content.Add(value);
            return Content.First().IsRightOrder(temp);
        }

        // -1: 相手 が 自分 より先に終端についた
        //  1: 自分 が 相手 より先に終端についた
        public int IsRightOrder(Container container)
        {
            for (var idx = 0; idx < Content.Count; idx++)
            {
                if (container.Content.Count <= idx)
                    return -1;

                var result = 0;
                var item = container.Content[idx];
                switch (item)
                {
                    case Value v:
                        result = Content[idx].IsRightOrder(v);
                        break;
                    case Container c:
                        result = Content[idx].IsRightOrder(c);
                        break;
                }

                if (result != 0)
                    return result;
            }

            if (Content.Count == container.Content.Count)
                return 0;

            return 1;
        }
    }

    private static Container Translate(string line)
    {
        var stack = new Stack<Container>();
        var valueLength = 0;

        for (var pos=0; pos < line.Length; pos++)
        {
            switch(line[pos])
            {
                case '[':
                    stack.Push(new Container());
                    break;

                case ']':
                    var instance = stack.Pop();

                    if (valueLength != 0)
                    {
                        var valueStr = line.Substring(pos - valueLength, valueLength);
                        var value = new Value(int.Parse(valueStr));
                        instance.Content.Add(value);
                        valueLength = 0;
                    }

                    if (stack.Any())
                        stack.Peek().Content.Add(instance);
                    else
                        return instance;

                    break;

                case ',':
                    if (valueLength != 0)
                    {
                        var valueStr = line.Substring(pos - valueLength, valueLength);
                        var value = new Value(int.Parse(valueStr));
                        stack.Peek().Content.Add(value);
                        valueLength = 0;
                    }
                    break;

                default:
                    valueLength++;
                    break;
            }
        }

        return stack.Pop();
    }

    private static void Print(Container container)
    {
        Console.Write("[");
        foreach (var c in container.Content)
        {
            switch (c)
            {
                case Value v:
                    Console.Write($" {v.Content} ");
                    break;
                case Container child:
                    Print(child);
                    break;
            }
            Console.Write(",");
        }
        Console.Write("]");
    }

    public static void PartOne()
    {
        var result = 0;
        var pairCount = 1;
        while (true)
        {
            var leftLine = Console.ReadLine();
            if (string.IsNullOrEmpty(leftLine))
                break;

            var left = Translate(leftLine);

            var rightLine = Console.ReadLine();
            var right = Translate(rightLine);

            var isInRightOrder = left.IsRightOrder(right);
            if (isInRightOrder == 1)
                result += pairCount;

            Console.ReadLine();

            pairCount++;
        }

        Console.WriteLine(result);
    }

    public static void PartTwo()
    {
        var packets = new List<Container>();
        while (true)
        {
            var line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
                continue;

            if (line == "EXIT")
                break;

            packets.Add(Translate(line));
        }

        var header1 = Translate("[[2]]");
        var header2 = Translate("[[6]]");
        packets.Add(header1);
        packets.Add(header2);

        packets.Sort((x, y) => y.IsRightOrder(x));

        var (key1, key2) = (0, 0);
        foreach (var (packet, idx) in packets.Select((packet, idx) => (packet, idx)))
        {
            if (packet.Equals(header1))
                key1 = idx + 1;
            else if (packet.Equals(header2))
                key2 = idx + 1;
        }

        Console.WriteLine(key1 * key2);
    }
}
