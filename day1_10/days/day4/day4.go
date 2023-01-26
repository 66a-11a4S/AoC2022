package day4

/*
day4

* エルフにキャンプのセクションを片付ける仕事を割り当てられる
* すべてのセクションには一意の ID 番号があり、各エルフにそれぞれ割り当てられる
* 割り当ての多くが重複していることに気付き、重複作業を減らすためにエルフはペアになり、各ペアのセクション割り当てリストを作成する

2-4,6-8
2-3,4-5
5-7,7-9

* この場合
  - 1番目のペアのエルフは 2~4 と 6~8 を対応することになる
  - 2番目のペアのエルフは 2~3 と 4~5 を対応することになる

Q1. 一部のペアは、割り当ての一方が他方を完全に含んでいることに気付いた (2-8,3-7 や 4-6 6-6 など)
このようなペア数はいくつ?

Q2. エルフはオーバーラップするペアの数を知りたい.
部分重複をふくむ、重複したペア数は?
*/

import (
	"bufio"
	"fmt"
	"os"
	"strconv"
	"strings"
)

func scanLine(sc *bufio.Scanner) string {
	sc.Scan()
	return sc.Text()
}

func translateToPair(line string) (int, int, int, int, bool) {

	pair := strings.Split(line, ",")

	if len(pair) != 2 {
		return 0, 0, 0, 0, false
	}

	firstSegment := strings.Split(pair[0], "-")
	firstHalfStart, _ := strconv.Atoi(firstSegment[0])
	firstHalfEnd, _ := strconv.Atoi(firstSegment[1])

	latterSegment := strings.Split(pair[1], "-")
	latterHalfStart, _ := strconv.Atoi(latterSegment[0])
	latterHalfEnd, _ := strconv.Atoi(latterSegment[1])

	return firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd, true
}

func isFullyOverlap(firstHalfStart int, firstHalfEnd int, latterHalfStart int, latterHalfEnd int) bool {

	if firstHalfStart < latterHalfStart || latterHalfEnd < firstHalfStart {
		return false
	}

	if firstHalfEnd < latterHalfStart || latterHalfEnd < firstHalfEnd {
		return false
	}

	return true
}

func isOverlap(firstHalfStart int, firstHalfEnd int, latterHalfStart int, latterHalfEnd int) bool {

	if latterHalfStart <= firstHalfStart && firstHalfStart <= latterHalfEnd {
		return true
	}

	if latterHalfStart <= firstHalfEnd && firstHalfEnd <= latterHalfEnd {
		return true
	}

	return false
}

func PartOne() {

	scanner := bufio.NewScanner(os.Stdin)
	result := 0

	for {

		line := scanLine(scanner)
		firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd, success := translateToPair(line)

		if !success {
			break
		}

		if isFullyOverlap(firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd) {
			//fmt.Print(firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd)
			result++
			continue
		}

		if isFullyOverlap(latterHalfStart, latterHalfEnd, firstHalfStart, firstHalfEnd) {
			//fmt.Print(firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd)
			result++
			continue
		}
	}

	fmt.Println(result)
}

func PartTwo() {

	scanner := bufio.NewScanner(os.Stdin)
	result := 0

	for {

		line := scanLine(scanner)
		firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd, success := translateToPair(line)

		if !success {
			break
		}

		if isOverlap(firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd) {
			//fmt.Print(firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd)
			result++
			continue
		}

		if isOverlap(latterHalfStart, latterHalfEnd, firstHalfStart, firstHalfEnd) {
			//fmt.Print(firstHalfStart, firstHalfEnd, latterHalfStart, latterHalfEnd)
			result++
			continue
		}
	}

	fmt.Println(result)
}
