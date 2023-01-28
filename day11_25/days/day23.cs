using Library;

/*
day23

枯れた森の真ん中にエルフの大群が形成されている.
今から各エルフは雑草を引き抜いて苗木を植える

....#..
..###.#
#...#.#
.#...##
#.###..
##.#.##
.#..#..

* エルフの位置は `#`,  空の地面は `.`
* スキャン範囲外には空の地面が無限に伸びている
* スキャンは、北が上になるように方向付けられる

エルフの動きはラウンドごとに更新される

[1ラウンドの流れ]

* ラウンド前半
  * エルフは自分に隣接する8方向となりを考慮
  * 8つの位置のいずれかに他のエルフがいない場合、エルフはこのラウンド中に何もしない
  * それ以外の場合、エルフは次の順序で 4 つの方向のそれぞれを見て、最初の有効な方向に1 ステップ移動することを `提案` する
    * 北、北東、北西に隣接する位置すべてにエルフがいない場合、エルフは北に 1 歩移動
    * 南、南東、南西に隣接する位置すべてにエルフがいない場合、エルフは南に 1 歩移動
    * 西、北西、南西に隣接する位置すべてにエルフがいない場合、エルフは西に 1 歩移動
    * 東、北東、南東に隣接する位置すべてにエルフがいない場合、エルフは東に 1 歩移動

* ラウンド後半
  * エルフがその位置への移動を提案した唯一のエルフである場合、提案された位置に移動
  * 2人以上のエルフが同じ位置に移動する提案をした場合、移動しない

* ラウンド終了時
  * 検討する方向リストの最初が最後に移動する.
    * 2ラウンド目は、エルフは南から調べる
    * 3ラウンド目は、エルフは西から調べる

ラウンド 10 の後に、すべてのエルフを含む最小の矩形に含まれる空の地面タイルの数を数える

Q1. 長方形内の空マスの数はいくつ?

Q2. どのエルフも動かなくなるのは何ラウンド目?
*/

public class Day23
{
    public class Elf
	{
		public Vector2Int Point { get; set; }
		public Vector2Int ReservedPoint { get; set; }
	}

	public enum Direction
	{
		North,
		South,
		West,
		East,
	}

	private static readonly Vector2Int[] _allAdjacentPoints = new[]
	{
		new Vector2Int(0, -1),
		new Vector2Int(1, -1),
		new Vector2Int(1, 0),
		new Vector2Int(1, 1),
		new Vector2Int(0, 1),
		new Vector2Int(-1, 1),
		new Vector2Int(-1, 0),
		new Vector2Int(-1, -1),
	};

	private static readonly Vector2Int[] _northAdjacentPoints = new[]
	{
		new Vector2Int(0, -1),
		new Vector2Int(1, -1),
		new Vector2Int(-1, -1),
	};

	private static readonly Vector2Int[] _southAdjacentPoints = new[]
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 1),
		new Vector2Int(-1, 1),
	};

	private static readonly Vector2Int[] _westAdjacentPoints = new[]
	{
		new Vector2Int(-1, 0),
		new Vector2Int(-1, -1),
		new Vector2Int(-1, 1),
	};

	private static readonly Vector2Int[] _eastAdjacentPoints = new[]
	{
		new Vector2Int(1, 0),
		new Vector2Int(1, -1),
		new Vector2Int(1, 1),
	};

	public static void PartOne()
	{
		var elves = Load();

		var proposeDirection = new Queue<Direction>();
		proposeDirection.Enqueue(Direction.North);
		proposeDirection.Enqueue(Direction.South);
		proposeDirection.Enqueue(Direction.West);
		proposeDirection.Enqueue(Direction.East);

		for (var count = 0; count < 10; count++)
		{
			ExecuteRound(proposeDirection, elves);
		}

		PrintState(elves);
	}


	public static void PartTwo()
	{
		var elves = Load();

		var proposeDirection = new Queue<Direction>();
		proposeDirection.Enqueue(Direction.North);
		proposeDirection.Enqueue(Direction.South);
		proposeDirection.Enqueue(Direction.West);
		proposeDirection.Enqueue(Direction.East);

		var round = 0;
		while (true)
		{
			round++;
			var anyElfWasMoved = ExecuteRound(proposeDirection, elves);
			if (!anyElfWasMoved)
				break;
		}

		Console.WriteLine(round);
	}

	private static bool ExecuteRound(Queue<Direction> proposeDirection, IReadOnlyCollection<Elf> elves)
	{
		var elfPoints = elves.Select(elf => elf.Point).ToHashSet();
		var reservedPoints = new Dictionary<Vector2Int, int>();
		var anyElfWasMoved = false;

		foreach (var elf in elves)
			elf.ReservedPoint = null;

		foreach (var elf in elves)
		{
			if (!CanAct(elf.Point, elfPoints))
				continue;

			foreach (var dir in proposeDirection)
			{
				if (!TryPropose(elf.Point, dir, elfPoints, out var proposedPoint))
					continue;

				if (!reservedPoints.ContainsKey(proposedPoint))
					reservedPoints[proposedPoint] = 0;

				reservedPoints[proposedPoint]++;
				elf.ReservedPoint = proposedPoint;
				break;
			}
		}

		foreach (var elf in elves)
		{
			if (elf.ReservedPoint == null)
				continue;

			if (reservedPoints[elf.ReservedPoint] == 1)
			{
				elf.Point = elf.ReservedPoint;
				anyElfWasMoved = true;
			}
		}

		var latest = proposeDirection.Dequeue();
		proposeDirection.Enqueue(latest);

		return anyElfWasMoved;
	}

	private static bool CanAct(Vector2Int pos, IReadOnlyCollection<Vector2Int> elfPoints)
	{
		var adjacentPoints = _allAdjacentPoints.Select(adjacent => pos + adjacent);
		return adjacentPoints.Any(point => elfPoints.Contains(point));
	}

	private static bool TryPropose(Vector2Int pos, Direction direction, IReadOnlyCollection<Vector2Int> elfPoints, out Vector2Int proposedPoint)
	{
		var adjacentPoints = GetAdjacentPoint(direction).Select(adjacent => pos + adjacent);
		if (adjacentPoints.Any(point => elfPoints.Contains(point)))
		{
			proposedPoint = null;
			return false;
		}

		proposedPoint = adjacentPoints.FirstOrDefault(point => !elfPoints.Contains(point));
		return proposedPoint is not null;
	}

	private static IReadOnlyCollection<Vector2Int> GetAdjacentPoint(Direction dir)
	{
		switch(dir)
		{
			case Direction.North: return _northAdjacentPoints;
			case Direction.South: return _southAdjacentPoints;
			case Direction.West: return _westAdjacentPoints;
			case Direction.East: return _eastAdjacentPoints;
			default: throw new ArgumentOutOfRangeException(nameof(dir));
		}
	}

	private static List<Elf> Load()
	{
		var elves = new List<Elf>();
		var y = 0;
		while (true)
		{
			var line = Console.ReadLine();
			if (string.IsNullOrEmpty(line))
				break;

			for (var x = 0; x < line.Length; ++x)
			{
				if (line[x] == '#')
					elves.Add(new Elf(){ Point = new Vector2Int(x, y) });
			}
			y++;
		}

		return elves;
	}

	private static void PrintState(IReadOnlyCollection<Elf> elves)
	{
		var elfPoints = elves.Select(elf => elf.Point).ToHashSet();
		var minX = elves.Select(elf => elf.Point.X).Min();
		var maxX = elves.Select(elf => elf.Point.X).Max();
		var minY = elves.Select(elf => elf.Point.Y).Min();
		var maxY = elves.Select(elf => elf.Point.Y).Max();

		for (var y = minY; y <= maxY; y++)
		{
			for (var x = minX; x <= maxX; x++)
			{
				var pos = new Vector2Int(x, y);
				if (elfPoints.Contains(pos))
					Console.Write('#');
				else
					Console.Write('.');
			}
			Console.WriteLine();
		}
		Console.WriteLine();

		Console.WriteLine((maxX - minX + 1) * (maxY - minY + 1) - elves.Count);
	}
}
