package day3

/*
day3

各リュックには2つの大きな区画がある.
リュックの2つの区画には別々のものを入れるべきだが、1つの種類のアイテムが両区画に入ってる

* アイテムタイプ a~z, A~Z の1文字で表現される(大文字/小文字を区別)
* リュックのアイテムリストは、1行の文字列として表示
* リュックの2つの区画には常に同じ数のアイテムが入っている

例えばあるリュックについて `vJrwpWtwJgWrhcsFMMfFFhFp` ならば前半/後半は
`vJrwpWtwJgWr`
`hcsFMMfFFhFp`
両方に現れる唯一の文字は `p`

アイテムには優先順位を付けられる. a-z -> 1~26, A-Z -> 27~52

Q1. 各リュックの両区画に表示されるアイテムの優先度の合計は?

Q2.

エルフを3人グループに分ける.
すべてのエルフはグループを識別するバッジとなるアイテムを持つ.
グループ内では、バッジは3人のエルフすべてが持つ唯一共通のアイテムタイプとなる.

* 入力は3行ごとに1つのグループを表す
* グループごとに、必ず1行ごとに現れる共通のアイテムがある

各グループのバッジに対応するアイテムの優先度の合計は?
*/

import (
	"bufio"
	"fmt"
	"os"

	"github.com/emirpasic/gods/sets/hashset"
)

func scanLine(sc *bufio.Scanner) string {
	sc.Scan()
	return sc.Text()
}

func calcPriority(char rune) int {

	result := (int)(char - 'a' + 1)
	if 0 < result {
		return result
	}

	return (int)(char - 'A' + 27)
}

func PartOne() {
	scanner := bufio.NewScanner(os.Stdin)

	prioritySum := 0

	for {
		line := scanLine(scanner)
		lineLen := len(line)

		if lineLen == 0 {
			break
		}

		firstHalf := line[:lineLen/2]
		latterHalf := line[lineLen/2:]
		firstRunePattern := hashset.New()
		latterRunePattern := hashset.New()

		for idx := 0; idx < lineLen/2; idx++ {
			firstRunePattern.Add(firstHalf[idx])
		}

		for idx := 0; idx < lineLen/2; idx++ {
			char := latterHalf[idx]
			if firstRunePattern.Contains(char) && !latterRunePattern.Contains(char) {
				priority := calcPriority((rune)(char))
				prioritySum += priority
				latterRunePattern.Add(char)
				//fmt.Println(priority)
			}
		}
	}

	fmt.Println(prioritySum)
}

func PartTwo() {

	scanner := bufio.NewScanner(os.Stdin)

	prioritySum := 0

	for {
		firstRunePattern := hashset.New()
		secondRunePattern := hashset.New()

		line := scanLine(scanner)
		if len(line) == 0 {
			break
		}

		for idx := 0; idx < len(line); idx++ {
			firstRunePattern.Add(line[idx])
		}

		secondLine := scanLine(scanner)
		for idx := 0; idx < len(secondLine); idx++ {
			secondRunePattern.Add(secondLine[idx])
		}

		thirdLine := scanLine(scanner)
		for idx := 0; idx < len(thirdLine); idx++ {
			if firstRunePattern.Contains(thirdLine[idx]) &&
				secondRunePattern.Contains(thirdLine[idx]) {
				//fmt.Println(calcPriority(rune(thirdLine[idx])))
				prioritySum += calcPriority(rune(thirdLine[idx]))
				break
			}
		}
	}

	fmt.Println(prioritySum)
}
