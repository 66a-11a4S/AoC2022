using Library;
using System.Text.RegularExpressions;

/*
day21

* 各サルは、特定の数字を叫ぶか、数値演算の結果を叫ぶ
  * 数値を叫ぶサルはすべて、最初から自分の番号を知っている
  * 算術演算のサルは、他の2匹のサルが数字を叫ぶのを待つ

root: pppw + sjmn
dbpl: 5
cczh: sllz + lgvd
ptdq: humn - dvpt
sjmn: drzm * dbpl
pppw: cczh / lfqf
...

Q1. `root` という名前のサルが叫ぶ数はいくつ?

Q2. 

* `root` という名前の猿は2つの値が等しいことか確認する
* `humn` という名前の猿は自分がどの値を叫ぶべきか知らない
* `root` が値が等しいと叫ぶために、`humn` が叫ぶべき数値はいくつ?
*/

public static class Day21
{
    private static Regex expressionPattern = new Regex(@"(\S+|\+|\-|\*|\/)");

    private static void BuildExpression(string name, Dictionary<string, string> expressionStringTable,
        Dictionary<string, IFactor<long>> factorTable)
    {
        var expressionString = expressionStringTable[name];
        var matches = expressionPattern.Matches(expressionString);

        if (matches.Count == 1)
        {
            var value = matches[0].Value;
            if (!factorTable.ContainsKey(name))
            {
                //Console.WriteLine($"value : {value}");
                factorTable[name] = new ValueFactor<long>{ Value = long.Parse(value) };
            }
        }
        else if (1 < matches.Count)
        {
            var lhs = matches[0].Value;
            var operand = matches[1].Value;
            var rhs = matches[2].Value;
            //Console.WriteLine($"expression : {lhs} {operand} {rhs}");

            if (!factorTable.ContainsKey(lhs))
            {
                BuildExpression(lhs, expressionStringTable, factorTable);
            }
            if (!factorTable.ContainsKey(rhs))
            {
                BuildExpression(rhs, expressionStringTable, factorTable);
            }

            factorTable[name] = BuildOperand(factorTable[lhs], factorTable[rhs], operand);
        }
    }

    private static IFactor<long> BuildOperand(IFactor<long> lhs, IFactor<long> rhs, string operandString)
    {
        switch(operandString)
        {
            case "+":
                return new AddObject<long>(lhs, rhs);
            case "-":
                return new SubtractObject<long>(lhs, rhs);
            case "*":
                return new MultiplyObject<long>(lhs, rhs);
            case "/":
                return new DivideObject<long>(lhs, rhs);
            default:
                throw new ArgumentOutOfRangeException(operandString);
        }
    }

    public static void PartOne()
    {
        var expressionStringTable = new Dictionary<string, string>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var contents = line.Split(':');
            var name = contents[0];
            var expressionString = contents[1];
            expressionStringTable[name] = expressionString;
        }

        var factorTable = new Dictionary<string, IFactor<long>>();
        BuildExpression("root", expressionStringTable, factorTable);
        Console.WriteLine(factorTable["root"].Value);
    }

    public static void PartTwo()
    {
        var expressionStringTable = new Dictionary<string, string>();
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var contents = line.Split(':');
            var name = contents[0];
            var expressionString = contents[1];
            expressionStringTable[name] = expressionString;
        }

        var factorTable = new Dictionary<string, IFactor<long>>();
        var matches = expressionPattern.Matches(expressionStringTable["root"]);
        var lhs = matches[0].Value;
        var rhs = matches[2].Value;
        BuildExpression(lhs, expressionStringTable, factorTable);
        BuildExpression(rhs, expressionStringTable, factorTable);

        var lhsFactor = factorTable[lhs];
        var rhsFactor = factorTable[rhs];
        var humnFactor = factorTable["humn"] as ValueFactor<long>;

        var lower = 0L;
        var upper = 5000000000000L; // TODO: 初期値をいい感じに求める方法は? (long.MaxValue だと式木の途中でオーバーフロー)
        var initialLhs = lhsFactor.Value;
        var initialRhs = rhsFactor.Value;
        var lowerFactor = initialLhs < initialRhs ? lhsFactor : rhsFactor;
        var upperFactor = initialLhs < initialRhs ? rhsFactor : lhsFactor;
        while(1 < upper - lower)
        {
            var mid = (upper + lower) / 2;
            humnFactor.Value = mid;

            if (lowerFactor.Value < upperFactor.Value)
                lower = mid;
            else
                upper = mid;
        }

        Console.WriteLine(upper);
    }
}