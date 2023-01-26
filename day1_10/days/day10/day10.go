package day10

/*
day10

* 通信デバイスの画面が壊れた
* クロックとスクリーンとCPUで構成される.
* CPU は 値が1で始まるレジスタ X が 1つ
* サポートされる命令は
  - `addx V` : 完了するまで2サイクルかかる. 完了すると X レジスタの値は V 増える
  - `noop` : 完了するまで1サイクルかかる

* 信号の強さ = Xレジスタ値 * サイクル数 を20サイクル毎にモニタリングする

Q1. 20,60, 100, ..., 220サイクルの40サイクルごとの信号の強さの合計は?

Q2.

* X レジスタがスプライトの水平位置を制御している
* スプライトの幅は3ピクセルで、Xレジスタはスプライトの中央の位置を指す
* CRT(ブラウン管)は (width, height) = (40, 6) のピクセルで構成
* CRTはピクセルの左上から右下にかけて、1行ずつ出力する
  * 左上の位置は 0, 右上の位置は 39
* CRT は1サイクルで1つのピクセルを描画する
* スプライトが表示される位置のピクセルは #, ない位置は . と表示される

1サイクル目
<addx 15>
Sprite pos: ###.......
Display :   #

2サイクル目
Sprite pos: ###.......
Display :   ##

3サイクル目
<addx -11>
Sprite pos: ...............###
Display :   ##.

4サイクル目
Sprite pos: ...............###
Display :   ##..

5サイクル目
<addx 6>
Sprite pos: ....###...........
Display :   ##..#

といった具合.
コマンドを処理していった結果、CRTに表示される8つの大文字は何?
*/

import (
	"bufio"
	"fmt"
	"os"
	"strconv"
	"strings"
)

type Command int

const (
	Noop = iota
	Addx
)

func scanLine(sc *bufio.Scanner) string {
	sc.Scan()
	return sc.Text()
}

func ParseCommand(command string) (Command, int, int) {
	elements := strings.Split(command, " ")

	var commandType Command = Noop
	var duration int
	var value int

	switch elements[0] {
	case "addx":
		commandType = Addx
		duration = 2
		value, _ = strconv.Atoi(elements[1])
	case "noop":
		commandType = Noop
		duration = 1
	}

	return commandType, duration, value
}

func calcStrength(value int, cycle int) int {
	return value * cycle
}

func calcPosition(X int, width int) (int, int) {
	x := X % width
	y := X / width
	return x, y
}

func willShowPixel(position int, spriteCenter int, width int) bool {
	horizontalSpriteCenter := spriteCenter % width
	horizontalPosition := position % width
	return horizontalSpriteCenter-1 <= horizontalPosition && horizontalPosition <= horizontalSpriteCenter+1
}

func PartOne() {

	scanner := bufio.NewScanner(os.Stdin)

	X := 1
	cycle := 0
	batchCount := 40
	strengthFootprint := make([]int, 0)

	for {
		line := scanLine(scanner)

		if len(line) == 0 {
			break
		}

		command, duration, value := ParseCommand(line)
		for count := 0; count < duration; count++ {
			cycle++
			if cycle%20 == 0 {
				//fmt.Printf("cycle: %d, value: %d \n", cycle, calcStrength(X, cycle))
				strengthFootprint = append(strengthFootprint, calcStrength(X, cycle))
			}
		}

		if command == Addx {
			X += value
		}
	}

	result := 0
	for cycleCount := 20; cycleCount <= 220; cycleCount += batchCount {
		idx := cycleCount/20 - 1
		result += strengthFootprint[idx]
		fmt.Println(result)
	}

	fmt.Println(result)
}

func PartTwo() {

	scanner := bufio.NewScanner(os.Stdin)

	X := 1
	cycle := 0
	const width = 40
	const height = 6
	display := [height][width]rune{}

	for {
		line := scanLine(scanner)

		if len(line) == 0 {
			break
		}

		command, duration, value := ParseCommand(line)
		for count := 0; count < duration; count++ {

			x, y := calcPosition(cycle, width)
			//fmt.Printf("x: %d, y: %d, cycle: %d, X:%d \n", x, y, cycle, X)
			if willShowPixel(cycle, X, width) {
				display[y][x] = '#'
			} else {
				display[y][x] = '.'
			}

			cycle++
		}

		if command == Addx {
			X += value
		}
	}

	for y := 0; y < height; y++ {
		for x := 0; x < width; x++ {
			fmt.Printf("%c", display[y][x])
		}
		fmt.Printf("\n")
	}
}
