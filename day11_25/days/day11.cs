/*
day11

* サルがあなたの荷物でキープアウェイをしてる
* サルは各アイテムに対するあなたの `心配度` に基づいて動く
* サルは現在持っているアイテム、それついての `心配度` 、 `心配度` に基づいてどのように行動するかが異なる

Monkey
{
	StartingItem: 今持ってる各アイテムの心配度を調べる順番で表示
	Operation: 猿がアイテムを調べるにつれて、アイテムの心配度がどのように変化するか
	Test: 心配度を元に、次にどこにアイテムを投げるか
	  心配度をいくつで割れるか, true なら ..., false なら ...
}

* サルが `心配度` をもとに `Test` する前に3で割り、最も近い整数に切り捨てる

## 猿の動き

* サルは順番に物を調べたり投げたりする
* 1匹の猿の番では、持っているすべてのアイテムを一度に1つずつ調べて、リストの順序で投げる
* サル0が最初、次にサル1というように、各サルが1周するまで続ける
* 各サルが1回転するプロセスを ラウンド と呼ぶ
* サルが別のサルにアイテムを投げると、そのアイテムは受け取ったサルのリストの最後に追加
* サルがターンの開始時にアイテムを持っていない場合、そのターンは終了

## 求めるもの

* 最も活発な2匹のサルがアイテムを調べた回数の乗算数を `monkey business` とする

Q1. 20ラウンド後の monkey business 値はいくつ?

Q2.
* 心配度を3で割るのをやめた
* 10000ラウンド後の monkey business 値はいくつ?
*/

public class Day11
{
    public static void PartOne()
    {
        var monkeys = GetPuzzle().ToArray() ;
        var inspectCount = new Dictionary<int, Int64>();

        for (var idx = 0; idx < monkeys.Count(); idx++)
            inspectCount[idx] = 0;

        for (var round = 0; round < 20; round++)
        {
            for (var idx = 0; idx < monkeys.Count(); idx++)
            {
                var actor = monkeys[idx];

                foreach (var item in actor.Items)
                {
                    var value = new Value(actor.Operation(item).Execute() / 3);
                    var destination = actor.Test(value);
                    inspectCount[idx]++;
                    monkeys[destination].Items.Add(value);
                }
                actor.Items.Clear();
            }
        }

        var result = inspectCount.Values.OrderByDescending(count => count).Take(2).ToArray();
        Console.WriteLine(result[0] * result[1]);
    }

    public static void PartTwo()
    {
        var monkeys = GetPuzzle().ToArray() ;
        var inspectCount = new Dictionary<int, Int64>();

        for (var idx = 0; idx < monkeys.Count(); idx++)
            inspectCount[idx] = 0;

        for (var round = 1; round <= 10000; round++)
        {
            for (var idx = 0; idx < monkeys.Count(); idx++)
            {
                var actor = monkeys[idx];

                foreach (var item in actor.Items)
                {
                    var calculated = actor.Operation(item);
                    var destination = actor.Test(calculated);
                    inspectCount[idx]++;
                    monkeys[destination].Items.Add(calculated);
                }
                actor.Items.Clear();
            }

            if (round % 1000 == 0)
            {
                Console.WriteLine($"round {round}");

                for (var idx = 0; idx < monkeys.Count(); idx++)
                    Console.Write($"{inspectCount[idx]} ");
                
                Console.WriteLine();
            }
        }

        var result = inspectCount.Values.OrderByDescending(count => count).Take(2).ToArray();
        Console.WriteLine(result[0] * result[1]);
    }

    public interface Factor 
    {
        int Execute();
        int Mod(int value);
    }

    public class Value : Factor
    {
        private int _value;
        public Value(int value) => _value = value;

        int Factor.Execute() => _value;
        int Factor.Mod(int mod) => _value % mod;
    }

    public class Add : Factor
    {
        private Factor _parent;
        private Factor _value;
        private Dictionary<int, int> _modCaches = new Dictionary<int, int>();

        public Add(Factor parent, Factor value) => (_parent, _value) = (parent, value);

        int Factor.Execute() => _parent.Execute() + _value.Execute();
        int Factor.Mod(int mod)
        {
            if (!_modCaches.ContainsKey(mod))
                _modCaches[mod] = (_parent.Mod(mod) + _value.Mod(mod)) % mod;
        
            return _modCaches[mod];
        }
    }

    public class Mul : Factor
    {
        private Factor _parent;
        private Factor _value;
        private Dictionary<int, int> _modCaches = new Dictionary<int, int>();

        public Mul(Factor parent, Factor value) => (_parent, _value) = (parent, value);

        int Factor.Execute() => _parent.Execute() * _value.Execute();
        int Factor.Mod(int mod)
        {
            if (!_modCaches.ContainsKey(mod))
                _modCaches[mod] = (_parent.Mod(mod) * _value.Mod(mod)) % mod;

            return _modCaches[mod];
        }
    }

    public class Monkey
    {
        public List<Factor> Items { get; init; }
        public Func<Factor, Factor> Operation { get; init; }
        public Func<Factor, int> Test { get; init; }
    }

    // hard coded parameters
    public static IReadOnlyCollection<Monkey> GetSample()
    {
        var monkeys = new List<Monkey>();

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(79), new Value(98)},
                Operation = Factor(Factor src) => new Mul(src, new Value(19)),
                Test = int(Factor src) => {
                    if (src.Mod(23) == 0) {
                        return 2;
                    } else {
                        return 3;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(54), new Value(65), new Value(75), new Value(74)},
                Operation = Factor(Factor src) => new Add(src, new Value(6)),
                Test = int(Factor src) => {
                    if (src.Mod(19) == 0) {
                        return 2;
                    } else {
                        return 0;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(79), new Value(60), new Value(97)},
                Operation = Factor(Factor src) => new Mul(src, src),
                Test = int(Factor src) => {
                    if (src.Mod(13) == 0) {
                        return 1;
                    } else {
                        return 3;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(74)},
                Operation = Factor(Factor src) => new Add(src, new Value(3)),
                Test = int(Factor src) => {
                    if (src.Mod(17) == 0) {
                        return 0;
                    } else {
                        return 1;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        return monkeys;
    }

    public static IReadOnlyCollection<Monkey> GetPuzzle()
    {
        var monkeys = new List<Monkey>();

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(84), new Value(72), new Value(58), new Value(51)},
                Operation = Factor(Factor src) => new Mul(src, new Value(3)),
                Test = int(Factor src) => {
                    if (src.Mod(13) == 0) {
                        return 1;
                    } else {
                        return 7;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(88), new Value(58), new Value(58)},
                Operation = Factor(Factor src) => new Add(src, new Value(8)),
                Test = int(Factor src) => {
                    if (src.Mod(2) == 0) {
                        return 7;
                    } else {
                        return 5;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(93), new Value(82), new Value(71), new Value(77), new Value(83), new Value(53), new Value(71), new Value(89)},
                Operation = Factor(Factor src) => new Mul(src, src),
                Test = int(Factor src) => {
                    if (src.Mod(7) == 0) {
                        return 3;
                    } else {
                        return 4;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(81), new Value(68), new Value(65), new Value(81), new Value(73), new Value(77), new Value(96)},
                Operation = Factor(Factor src) => new Add(src, new Value(2)),
                Test = int(Factor src) => {
                    if (src.Mod(17) == 0) {
                        return 4;
                    } else {
                        return 6;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(75), new Value(80), new Value(50), new Value(73), new Value(88)},
                Operation = Factor(Factor src) => new Add(src, new Value(3)),
                Test = int(Factor src) => {
                    if (src.Mod(5) == 0) {
                        return 6;
                    } else {
                        return 0;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(59), new Value(72), new Value(99), new Value(87), new Value(91), new Value(81)},
                Operation = Factor(Factor src) => new Mul(src, new Value(17)),
                Test = int(Factor src) => {
                    if (src.Mod(11) == 0) {
                        return 2;
                    } else {
                        return 3;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(86), new Value(69)},
                Operation = Factor(Factor src) => new Add(src, new Value(6)),
                Test = int(Factor src) => {
                    if (src.Mod(3) == 0) {
                        return 1;
                    } else {
                        return 0;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        {
            var monkey = new Monkey
            {
                Items = new List<Factor>{new Value(91)},
                Operation = Factor(Factor src) => new Add(src, new Value(1)),
                Test = int(Factor src) => {
                    if (src.Mod(19) == 0) {
                        return 2;
                    } else {
                        return 5;
                    }
                }
            };
            monkeys.Add(monkey);
        }

        return monkeys;
    }
}
