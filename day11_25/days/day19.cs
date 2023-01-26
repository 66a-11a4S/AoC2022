using System.Text.RegularExpressions;

/*
Day19

黒曜石を使ってジオード割りロボットを作成し、ジオードを壊すことができます
(ジオード: https://ja.wikipedia.org/wiki/晶洞 )

ジオードを割るために、以下のロボットたちが必要になる

* 黒曜石を集めるには、黒曜石収集ロボットが必要. それには粘土が必要
* 粘土を収穫するには、粘土収集ロボットが必要
* 鉱石を収集するには、鉱石収集ロボットが必要
* 各種類のロボットを作成するには、鉱石が必要
* パックには鉱石収集ロボットが1つだけあり、それを使用してシミュレーションをスタート
* 各ロボットは、1分間にそのリソースタイプを1つ収集できる
* ロボット工場が任意のタイプのロボットを構築するのに1分かかり、構築開始時に必要なリソースが消費される

どのロボットをいつ構築するかを考慮し、24分後に割ったジオードの数を最大化する設計図を特定したい

```
Blueprint 1:
  Each ore robot costs 4 ore.
  Each clay robot costs 2 ore.
  Each obsidian robot costs 3 ore and 14 clay.
  Each geode robot costs 2 ore and 7 obsidian.

Blueprint 2:
  Each ore robot costs 2 ore.
  Each clay robot costs 3 ore.
  Each obsidian robot costs 3 ore and 8 clay.
  Each geode robot costs 3 ore and 12 obsidian.
```

各設計図の品質レベルは設計図の ID 番号に
その設計図を使用して 24 分間で開くことができるジオードの最大数を掛けてもとめられる

Q1. リスト内のすべての設計図の品質レベルを合計した値は?

* ore robot 1つもってスタート
* どんなに資材があっても1分でつくれる robot はどれかの種類を1つだけ.
  * bot を作っても余るほどに資材を掘っても仕方ないはず.
  * geode を最大化したいので geodeBot を優先して作る
* 状態を tree にして全探索する

minute: 0
resource:
bot: Ore=1

minute: 1
resource: Ore=1
bot: Ore=1

	// ClayBot を作った
	minute: 2
	resource: Ore=1
	bot: Ore=1, Cla=1

		// ClayBot を作った
		minute: 4
		resource: Ore=1, Cla=2
		bot: Ore=1, Cla=2

			minute: 6
			resource: Ore=1, Cla=6
			bot: Ore=1, Cla=3

			...

				minute: 8
				resource: Ore=1, Cla=12
				bot: Ore=1, Cla=4

					// ClayBot を作った
					minute: 9
					resource: Ore=2, Cla=12
					bot: Ore=1, Cla=4

				// ClayBot を作らなかった
				minute: 8
				resource: Ore=3, Cla=9
				bot: Ore=1, Cla=3

			// ClayBot を作らなかった
			minute: 6
			resource: Ore=2, Cla=4
			bot: Ore=1, Cla=2

				// OreBot を作った
				minute: 8
				resource: Ore=1, Cla=10
				bot: Ore=2, Cla=2

					minute: 9
					resource: Ore=3, Cla=12
					bot: Ore=2, Cla=2

				// ClayBot を作った
				minute: 8
				resource: Ore=3, Cla=10
				bot: Ore=1, Cla=3

					minute: 9
					resource: Ore=2, Cla=13
					bot: Ore=2, Cla=2

				// 何も作らなかった
				minute: 8
				resource: Ore=5, Cla=10
				bot: Ore=1, Cla=2

			...

Q2.
* リストの最初の3つの設計図だけを考慮する.
* 制限時間を32分に伸ばす
* 3つの各設計図で制限時間内に開いたジオードの各最大数の乗算値は?
*/

public class Day19
{
    const int MaxMinutes = 32;

    private enum ResourceType
    {
        Ore = 0,
        Clay,
        Obsidian,
        Geode,
        Size
    }

    public class Resource
    {
        public int Ore { get; set; }
        public int Clay { get; set; }
        public int Obsidian { get; set; }
        public int Geode { get; set; }

        public Resource Clone() => new Resource { Ore = this.Ore, Clay = this.Clay, Obsidian = this.Obsidian, Geode = this.Geode };
        public static Resource Empty = new Resource();
    }

    private class Blueprint
    {
        public int Id { get; }
        public int[] MaxRequirement => _maxRequirement;

        private int[,] _recipe;
        private int[] _maxRequirement;

        public Blueprint(int id, int[,] recipe)
        {
            Id = id;
            _recipe = recipe;
            _maxRequirement = new int[(int)ResourceType.Size];

            for (var resourceType = default(ResourceType); resourceType < ResourceType.Size; resourceType++)
            {
                for (var robotType = default(ResourceType); robotType < ResourceType.Size; robotType++)
                {
                    // 各ロボットの必要素材のうち、素材種類ごとに一番コストがかかるものを算出
                    var quantity = Quantity(robotType, resourceType);
                    _maxRequirement[(int)resourceType] = Math.Max(quantity, _maxRequirement[(int)resourceType]);
                }
            }
        }

        public int MaxQuantity(ResourceType resourceType) => MaxRequirement[(int)resourceType];
        public int Quantity(ResourceType robotType, ResourceType resourceType) => _recipe[(int)robotType, (int)resourceType];
    }

    private static Blueprint Parse(string line)
    {
        var recipePattern = new Regex(@"(\d+)\s([a-z,A-Z]+)");
        var lists = line.Substring(line.IndexOf(':') + 1).Split('.').ToArray();

        var bpRecipe =  new int[(int)ResourceType.Size, (int)ResourceType.Size];

        for (var robotType = ResourceType.Ore; robotType < ResourceType.Size; robotType++)
        {
            var recipe = recipePattern.Matches(lists[(int)robotType])
                .SelectMany(match => match.Captures)
                .Select(capture => capture.Value);

            foreach (var str in recipe)
            {
                var element = str.Split(' ');
                var quantity = int.Parse(element[0]);
                var resourceType = TranslateToType(element[1]);
                bpRecipe[(int)robotType, (int)resourceType] = quantity;
            }
        }

        var bp = new Blueprint(id: 0, bpRecipe);
        return bp;

        static ResourceType TranslateToType(string str)
        {
            switch (str)
            {
                case "ore": return ResourceType.Ore;
                case "clay": return ResourceType.Clay;
                case "obsidian": return ResourceType.Obsidian;
                case "geode": return ResourceType.Geode;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    public static void PartOne()
    {
        var bps = new List<Blueprint>();
        var result = 0;
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            bps.Add(Parse(line));
        }

        foreach (var (bp, idx) in bps.Select((bp, idx) => (bp, idx)))
        {
            var geodes = DFS(minute: 0,
                resources: Resource.Empty,
                robots: new Resource{ Ore = 1 },
                doNotBuild: Resource.Empty,
                bp);

            var qualityLevel = (idx + 1) * geodes;
            result += qualityLevel;
        }

        Console.WriteLine(result);
    }

    public static void PartTwo()
    {
        var bps = new List<Blueprint>();
        var result = 1;
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            bps.Add(Parse(line));
            if (3 <= bps.Count)
                break;
        }

        foreach (var (bp, idx) in bps.Select((bp, idx) => (bp, idx)))
        {
            var geodes = DFS(minute: 0,
                resources: Resource.Empty,
                robots: new Resource{ Ore = 1 },
                doNotBuild: Resource.Empty,
                bp);

            result *= geodes;
        }

        Console.WriteLine(result);
    }

    private static int DFS(int minute, Resource resources, Resource robots, Resource doNotBuild, Blueprint bp)
    {
        if (MaxMinutes <= minute)
            return resources.Geode;

        var nextDoNotBuild = new Resource();

        // geodeBot はできるだけ作る
        if (resources.Ore >= bp.Quantity(ResourceType.Geode, ResourceType.Ore) &&
            resources.Obsidian >= bp.Quantity(ResourceType.Geode, ResourceType.Obsidian))
        {
            var newResources = MineResources(resources, robots);
            newResources.Ore -= bp.Quantity(ResourceType.Geode, ResourceType.Ore);
            newResources.Obsidian -= bp.Quantity(ResourceType.Geode, ResourceType.Obsidian);
            var newRobots = robots.Clone();
            newRobots.Geode++;

            return DFS(minute + 1, newResources, newRobots, Resource.Empty, bp);
        }

        var maxGeodes = 0;

        // 前ステップに oreBot を作らず
        // oreBot 数 < bot の中で ore が一番必要な数 = 1 step で 1bot 作れないことがある場合は
        // oreBot を作る
        if (doNotBuild.Ore == 0 &&
            bp.Quantity(ResourceType.Ore, ResourceType.Ore) <= resources.Ore &&
            robots.Ore < bp.MaxQuantity(ResourceType.Ore))
        {
            var newResources = MineResources(resources, robots);
            newResources.Ore -= bp.Quantity(ResourceType.Ore, ResourceType.Ore);
            var newRobots = robots.Clone();
            newRobots.Ore++;

            // 次の時刻へ
            var newMaxGeodes = DFS(minute+1, newResources, newRobots, Resource.Empty, bp);
            maxGeodes = Math.Max(maxGeodes, newMaxGeodes);

            // 次の時刻で oreBot を作った場合は探索済みフラグをONに.
            // ↑のDFSと、Botを作らなかったときのDFSを場合分けするのに利用
            nextDoNotBuild.Ore++;
        }

        // clayBot
        if (doNotBuild.Clay == 0 &&
            bp.Quantity(ResourceType.Clay, ResourceType.Ore) <= resources.Ore &&
            robots.Clay < bp.MaxQuantity(ResourceType.Clay))
        {
            var newResources = MineResources(resources, robots);
            newResources.Ore -= bp.Quantity(ResourceType.Clay, ResourceType.Ore);
            var newRobots = robots.Clone();
            newRobots.Clay++;

            var newMaxGeodes = DFS(minute+1, newResources, newRobots, Resource.Empty, bp);
            maxGeodes = Math.Max(maxGeodes, newMaxGeodes);

            nextDoNotBuild.Clay++;
        }

        // obsidianBot
        if (doNotBuild.Obsidian == 0 &&
            bp.Quantity(ResourceType.Obsidian, ResourceType.Ore) <= resources.Ore &&
            bp.Quantity(ResourceType.Obsidian, ResourceType.Clay) <= resources.Clay &&
            robots.Obsidian < bp.MaxQuantity(ResourceType.Obsidian))
        {
            var newResources = MineResources(resources, robots);
            newResources.Ore -= bp.Quantity(ResourceType.Obsidian, ResourceType.Ore);
            newResources.Clay -= bp.Quantity(ResourceType.Obsidian, ResourceType.Clay);
            var newRobots = robots.Clone();
            newRobots.Obsidian++;

            var newMaxGeodes = DFS(minute+1, newResources, newRobots, Resource.Empty, bp);
            maxGeodes = Math.Max(maxGeodes, newMaxGeodes);

            nextDoNotBuild.Obsidian++;
        }

        // ロボットを作らなかったとき
        var updatedResource = MineResources(resources, robots);
        maxGeodes = Math.Max(maxGeodes, DFS(minute+1, updatedResource, robots, nextDoNotBuild, bp));
        return maxGeodes;
    }

    private static Resource MineResources(Resource resource, Resource robots)
    {
        return new Resource
        {
            Ore = resource.Ore + robots.Ore,
            Clay = resource.Clay + robots.Clay,
            Obsidian = resource.Obsidian + robots.Obsidian,
            Geode = resource.Geode + robots.Geode,
        };
    }
}
