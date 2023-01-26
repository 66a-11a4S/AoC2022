public class Day20
{
    /*
    day20

    木立の座標を含むファイルがあり、暗号化されている
    ファイルの復号化に関連する主な操作 mixing と呼ぶ

    * 暗号化されたファイルは数字のリスト
    * ファイルを mixing するには、移動する数値に等しい数の位置だけ、ファイル内で各数値を前方または後方に移動します。
    * リストは循環している

    - 平文で 4, 5, 6, 1, 7, 8, 9 のうち、 1 をmixingすると 4, 5, 6, 7, 1, 8, 9
    - 平文で 4, -2, 5, 6, 7, 8, 9 のうち -2 をmixingすると 4, 5, 6, 7, 8, -2, 9.

    * 数列の順番に移動する
    * 目的の座標は、値 0 の後の 1000番目、2000番目、3000番目の数字を調べる
    * 数列長が目的の番より短いときはループしてカウント

    Q1. 3つの数値の合計は?

    Q2. 
    * 各数値に 811589153 をかける
    * 数値リストを10回 mix する
      * mix される順序は mix 前と変わらない

    こうして得られる目的の座標値はいくつ?
    */
 
    public class LingNode<T>
    {
        public T Value { get; }
        public LingNode<T> Next { get; set; }
        public LingNode<T> Prev { get; set; }

        public LingNode(T value) => Value = value;

        public void ShiftForward(long step)
        {
            if (step == 0)
                return;

            var oldPrev = Prev;
            var oldCurr = this;
            var oldNext = Next;

            // prev -> curr -> next を
            // prev -> next -> curr にする
            oldCurr.Next = oldNext.Next;
            oldCurr.Prev = oldNext;

            oldPrev.Next = oldNext;

            oldNext.Prev = oldPrev;
            oldNext.Next.Prev = oldCurr;
            oldNext.Next = oldCurr;

            ShiftForward(step - 1);
        }

        public void ShiftBackward(long step)
        {
            if (step == 0)
                return;

            var oldPrev = Prev;
            var oldCurr = this;
            var oldNext = Next;

            // prev -> curr -> next を
            // curr -> prev -> next にする
            oldCurr.Next = oldPrev;
            oldCurr.Prev = oldPrev.Prev;

            oldPrev.Next = oldNext;

            oldNext.Prev = oldPrev;

            oldPrev.Prev.Next = oldCurr;
            oldPrev.Prev = oldCurr;

            ShiftBackward(step - 1);
        }
    }

    public static void PartOne()
    {
        var original = new List<LingNode<int>>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            original.Add(new LingNode<int>(int.Parse(line)));
        }

        for (var idx = 0; idx < original.Count; ++idx)
        {
            var nextIdx = (idx + 1) % original.Count;
            var prevIdx = 0 < idx
                ? (idx - 1) % original.Count
                : (original.Count + (idx - 1) % original.Count) % original.Count;

            original[idx].Next = original[nextIdx];
            original[idx].Prev = original[prevIdx];
        }

        foreach (var value in original)
        {
            // 余計に巡回しないように、ループ後の位置を計算してから移動する
            if (0 < value.Value)
                value.ShiftForward(value.Value % (original.Count - 1));
            else if (value.Value < 0)
                value.ShiftBackward(Math.Abs(value.Value) % (original.Count - 1));
        }

        var zeroNode = original.First(value => value.Value == 0);

        var result = 0;
        var current = zeroNode;
        for (var idx = 0; idx <= 3000; ++idx)
        {
            if (idx % 1000 == 0)
                result += current.Value;

            current = current.Next;
        }
        Console.WriteLine(result);
    }

    public static void PartTwo()
    {
        var decryptionKey = 811589153;

        var original = new List<LingNode<long>>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            original.Add(new LingNode<long>(long.Parse(line) * decryptionKey));
        }

        for (var idx = 0; idx < original.Count; ++idx)
        {
            var nextIdx = (idx + 1) % original.Count;
            var prevIdx = 0 < idx
                ? (idx - 1) % original.Count
                : (original.Count + (idx - 1) % original.Count) % original.Count;

            original[idx].Next = original[nextIdx];
            original[idx].Prev = original[prevIdx];
        }

        for (var mix = 0; mix < 10; mix++)
        {
            foreach (var value in original)
            {
                if (0 < value.Value)
                    value.ShiftForward(value.Value % (original.Count - 1));
                else if (value.Value < 0)
                    value.ShiftBackward(Math.Abs(value.Value) % (original.Count - 1));
            }
        }

        var zeroNode = original.First(value => value.Value == 0);

        var result = 0L;
        var current = zeroNode;
        for (var idx = 0; idx <= 3000; ++idx)
        {
            if (idx % 1000 == 0)
                result += current.Value;

            current = current.Next;
        }
        Console.WriteLine(result);
    }
}