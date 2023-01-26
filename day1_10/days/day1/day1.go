package day1

/*
day1

各エルフは軽食をいくつか持っている

* 各エルフは食料を持っており、そのカロリーが各行に表示さる
* 空行区切りで、各エルフが持つ食料を表す

Q1. 最大のカロリーを持つエルフの総カロリー数は?
Q2. 最大カロリーTOP3のエルフが運ぶ総カロリー数の合計は?
*/

import (
	"bufio"
	"fmt"
	"math"
	"os"
	"sort"
	"strconv"
)

func PartOne() {
	sc := bufio.NewScanner(os.Stdin)

	maxCalories := 0
	totalCalories := 0

	for {
		out, err := scanInt(sc)

		if err == nil {
			totalCalories += out
			continue
		}

		// 入力が何もない時
		if totalCalories == 0 {
			break
		}

		// 1人分のカロリーの入力が終わった時
		maxCalories = int(math.Max(float64(maxCalories), float64(totalCalories)))
		totalCalories = 0
	}

	fmt.Println(maxCalories)
}

func PartTwo() {
	sc := bufio.NewScanner(os.Stdin)

	totalCalories := 0
	totalCaloriesTable := []int{}

	for {
		out, err := scanInt(sc)

		if err == nil {
			totalCalories += out
			continue
		}

		// 入力が何もない時
		if totalCalories == 0 {
			totalCaloriesTable = append(totalCaloriesTable, totalCalories)
			break
		}

		// 1人分のカロリーの入力が終わった時
		totalCaloriesTable = append(totalCaloriesTable, totalCalories)
		totalCalories = 0
	}

	// 降順ソート
	sort.Slice(totalCaloriesTable, func(i, j int) bool { return totalCaloriesTable[j] < totalCaloriesTable[i] })
	answer := 0
	for i := 0; i < 3; i++ {
		answer += totalCaloriesTable[i]
	}
}

func scanInt(sc *bufio.Scanner) (int, error) {
	// 読みこみ
	sc.Scan()
	// int に変換
	i, err := strconv.Atoi(sc.Text())
	if err != nil {
		return 0, err
	}
	return i, nil
}
