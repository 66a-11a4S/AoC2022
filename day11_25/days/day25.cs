/*
day25

熱気球の "Bob" を飛ばす. そのためには各バーナーに燃料を必要分入れる.
必要な量は SNAFU 数で記述されている.

* SNAFU 数は5進数のように振る舞う
* 2, 1, 0, と -(マイナス), =(マイナス2) を使って表現する
  * 1: 1
  * 3: 1=
  * 10: 20
  * 20: 1-0
  * 976: 2=-01 ( 625 * 2  +  -2 * 125  +  -1 * 25  +  0 * 5  +  1)

気球へ入れる燃料数は SNAFU 数での入力が必要.

Q. リストの SNAFU 数を合計した値の SNAFU 数は?
*/

public class Day25
{
    public static void PartOne()
    {
        var sum = 0L;
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            var digit = ConvertSNAFUToDigit(line);
            sum += digit;
        }

        var five = ConvertDigitTo(sum, 5);
        var snafu = ConvertFiveToSNAFU(five);
        Console.WriteLine(snafu);
    }

    private static string ConvertFiveToSNAFU(string five)
    {
        var snafuDigits = five.Reverse().Select(c => new string(GetSNAFUValue(c).Reverse().ToArray())).ToArray();
        var result = snafuDigits[0];
        if (snafuDigits.Length < 1)
            return new string(result.Reverse().ToArray());

        // 2桁以上なら繰り上がりする可能性がある
        var roundedUp = 1 < snafuDigits[0].Length;
        for (var digit = 1; digit < snafuDigits.Length; digit++)
        {
            var snafuDigit = snafuDigits[digit];

            // 直前に繰り上げが発生してない
            if (!roundedUp)
            {
                result += snafuDigit;
                roundedUp = 1 < snafuDigit.Length;
                continue;
            }

            // 繰り上げ処理
            var c = snafuDigit.First();
            var add = new string(AddSNAFU(c, result.Last()).Reverse().ToArray());   // 繰り上げ後の SNAFU 数を求めて
            var actualDigit = snafuDigit.Remove(0, count: 1).Insert(0, add);
            result = result.Remove(result.Length - 1, count: 1);   // 先頭の1文字を繰り上げ後の SNAFU 数に書き換え
            result = result + actualDigit;

            roundedUp = 1 < actualDigit.Length;
        }

        return new string(result.Reverse().ToArray());

        static string AddSNAFU(char lhs, char rhs)
        {
            switch (lhs)
            {
                case '=':
                    switch (rhs)
                    {
                        // case '=': return "1";    // 繰り上がり時、この組み合わせは発生しない
                        // case '-': return "2";
                        case '0': return "=";
                        case '1': return "-";
                        case '2': return "0";
                    }
                    break;

                case '-':
                    switch (rhs)
                    {
                        // case '=': return "2";
                        // case '-': return "=";
                        case '0': return "-";
                        case '1': return "0";
                        case '2': return "1";
                    }
                    break;

                case '0':
                    switch (rhs)
                    {
                        case '=': return "=";
                        case '-': return "-";
                        case '0': return "0";
                        case '1': return "1";
                        case '2': return "2";
                    }
                    break;

                case '1':
                    switch (rhs)
                    {
                        case '=': return "-";
                        case '-': return "0";
                        case '0': return "1";
                        case '1': return "2";
                        case '2': return "1=";
                    }
                    break;

                case '2':
                    switch (rhs)
                    {
                        case '=': return "0";
                        case '-': return "1";
                        case '0': return "2";
                        case '1': return "1=";
                        case '2': return "1-";
                    }
                    break;
            }

            throw new InvalidOperationException("unexpected pair detected.");
        }
    }

    private static string ConvertDigitTo(long value, int n)
    {
        var result = string.Empty;
        while (0 < value)
        {
            var digit = value % n;
            result = digit.ToString() + result;
            value = value / n;
        }

        return result;
    }

    private static long ConvertSNAFUToDigit(string snafu)
    {
        var result = 0L;
        var digit = 0;

        foreach(var c in snafu.Reverse())
        {
            result += GetValue(c) * (long)Math.Pow(5, digit);
            digit++;
        }

        return result;
    }

    private static int GetValue(char c)
    {
        switch(c)
        {
            case '2': return 2;
            case '1': return 1;
            case '0': return 0;
            case '-': return -1;
            case '=': return -2;
            default: throw new ArgumentOutOfRangeException(nameof(c));
        }
    }

    private static string GetSNAFUValue(char c)
    {
        switch(c)
        {
            case '0': return "0";
            case '1': return "1";
            case '2': return "2";
            case '3': return "1=";
            case '4': return "1-";
            default: throw new ArgumentOutOfRangeException(nameof(c));
        }
    }
}
