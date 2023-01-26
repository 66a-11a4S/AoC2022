package day5

/*
* 必要な物資が他のクレート(木枠)の下に埋もれている
* クレートはクレーンで移動してスタックされる。クレートを再配置すると、目的のクレートが各スタックの一番上になる
* クレートの開始スタックと再配置手順が与えられる

```
    [D]
[N] [C]
[Z] [M] [P]
 1   2   3
move 1 from 2 to 1
move 3 from 1 to 3
move 2 from 2 to 1
move 1 from 1 to 2
```
↑の場合、3つの stack があり
1番目の stack には2つの crate がつまれて、Nがtopにある

* 配置指示は move {いくつ} from {どこから} to {どこへ} の形式.
* 指示をこなすと最終的に各 stack の top は CMZ となる

Q1. 各スタックの top の crate は何?

Q2. procedure の操作は実は, move 回分 pop ではなく move こまとめて 1回 pop だった.
このルール下で、操作後の各スタックの top の crate は何?
*/

import (
	"bufio"
	"fmt"
	"os"
	"sort"
	"strconv"
	"strings"

	"github.com/golang-collections/collections/stack"
)

func initialState() []string {
	return []string{
		"SCVN",
		"ZMJHNS",
		"MCTGJND",
		"TDFJWRM",
		"PFH",
		"CTZHJ",
		"DPRQFSLZ",
		"CSLHDFPW",
		"DSMPFNGZ",
	}
}

func scanLine(sc *bufio.Scanner) string {
	sc.Scan()
	return sc.Text()
}

func parseProcedure(line string) (int, int, int, bool) {
	elements := strings.Split(line, " ")
	if len(elements) != 6 {
		return 0, 0, 0, false
	}

	move, _ := strconv.Atoi(elements[1])
	from, _ := strconv.Atoi(elements[3])
	to, _ := strconv.Atoi(elements[5])
	return move, from, to, true
}

func parseStackInitialState(line string) *stack.Stack {
	stack := stack.New()
	length := len(line)
	for idx := 0; idx < length; idx++ {
		stack.Push((rune)(line[idx]))
	}
	return stack
}

func PartOne() {

	stacks := map[int]*stack.Stack{}
	initialState := initialState()
	for idx, state := range initialState {
		key := idx + 1
		stacks[key] = parseStackInitialState(state)
	}

	scanner := bufio.NewScanner(os.Stdin)
	for {
		line := scanLine(scanner)
		move, from, to, success := parseProcedure(line)
		//fmt.Printf("%d, %d, %d, %t", move, from, to, success)

		if !success {
			break
		}

		for count := 0; count < move; count++ {
			crate := stacks[from].Pop()
			stacks[to].Push(crate)
		}
	}

	//	for key, stack := range stacks {
	//		fmt.Printf("%d: %d %c\n", key, stack.Len(), stack.Peek())
	//	}
	for key, stack := range stacks {
		fmt.Printf("%d: %c\n", key, stack.Peek())
	}
}

func PartTwo() {

	stacks := map[int]*stack.Stack{}
	initialState := initialState()
	for idx, state := range initialState {
		key := idx + 1
		stacks[key] = parseStackInitialState(state)
	}

	scanner := bufio.NewScanner(os.Stdin)
	buffer := stack.New()
	for {
		line := scanLine(scanner)
		move, from, to, success := parseProcedure(line)

		if !success {
			break
		}

		for count := 0; count < move; count++ {
			crate := stacks[from].Pop()
			buffer.Push(crate)
		}
		for count := 0; count < move; count++ {
			crate := buffer.Pop()
			stacks[to].Push(crate)
		}
	}

	keys := []int{}
	for key := range stacks {
		keys = append(keys, key)
	}

	sort.Ints(keys)
	for _, key := range keys {
		fmt.Printf("%d: %c\n", key, stacks[key].Peek())
	}
}
