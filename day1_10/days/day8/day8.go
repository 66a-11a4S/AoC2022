package day8

/*
day8

* 行または列に沿って直接見たときにグリッドの外側から見える木の数を数える
* 0~9の数値はtreeの高さ.
* treeが上下左右の直線上のいずれかマスより高ければ、そのtreeは `見える`
* グリッドの端にあるすべての木は `見える` .

30373
25512
65332
33549
35390

11111
11101
11011
10101
11111

* ↑の例なら周囲の 16 マス と内側の9マスのうち5マス見えて(1), 4マス見えない(0)

Q1. どれだけの tree が見えますか

Q2.

* ツリーハウスを建てるのに最適な場所を探す. たくさんの木が見える場所がよい
* 端に到達するか、その木と同じ高さ以上の最初の木に到達するまで見える
* `眺めの良さ` は4方向の見える木の数の乗算値
* このスコアの最高値はいくつ?
*/

import (
	"bufio"
	"fmt"
	"os"
)

type Visibility struct {
	top       int
	right     int
	bottom    int
	left      int
	height    int
	isOutside bool
}

func (v *Visibility) IsVisible() int {
	if v.isOutside {
		return 1
	}

	if v.height <= v.top &&
		v.height <= v.right &&
		v.height <= v.bottom &&
		v.height <= v.left {
		return 0
	}

	return 1
}

func calculateVisibility(posX int, posY int, grid [][]Visibility) {

	heightFromLeft := 0
	for x := 0; x < posX; x++ {
		if heightFromLeft < grid[posY][x].height {
			heightFromLeft = grid[posY][x].height
		}
	}

	heightFromRight := 0
	width := len(grid[posY])
	for x := width - 1; posX < x; x-- {
		if heightFromRight < grid[posY][x].height {
			heightFromRight = grid[posY][x].height
		}
	}

	heightFromTop := 0
	for y := 0; y < posY; y++ {
		if heightFromTop < grid[y][posX].height {
			heightFromTop = grid[y][posX].height
		}
	}

	heightFromBottom := 0
	height := len(grid)
	for y := height - 1; posY < y; y-- {
		if heightFromBottom < grid[y][posX].height {
			heightFromBottom = grid[y][posX].height
		}
	}

	grid[posY][posX].left = heightFromLeft
	grid[posY][posX].right = heightFromRight
	grid[posY][posX].top = heightFromTop
	grid[posY][posX].bottom = heightFromBottom
}

func calculateScore(posX int, posY int, grid [][]Visibility) int {

	if grid[posY][posX].isOutside {
		return 0
	}

	width := len(grid[posY])
	height := len(grid)

	scoreToLeft := 1
	for x := posX - 1; 0 < x; x-- {
		if grid[posY][x].height < grid[posY][posX].height {
			scoreToLeft++
		} else {
			break
		}
	}

	scoreToRight := 1
	for x := posX + 1; x < width-1; x++ {
		if grid[posY][x].height < grid[posY][posX].height {
			scoreToRight++
		} else {
			break
		}
	}

	scoreToTop := 1
	for y := posY - 1; 0 < y; y-- {
		if grid[y][posX].height < grid[posY][posX].height {
			scoreToTop++
		} else {
			break
		}
	}

	scoreToBottom := 1
	for y := posY + 1; y < height-1; y++ {
		if grid[y][posX].height < grid[posY][posX].height {
			scoreToBottom++
		} else {
			break
		}
	}

	return scoreToLeft * scoreToRight * scoreToTop * scoreToBottom
}

func scanLine(sc *bufio.Scanner) string {
	sc.Scan()
	return sc.Text()
}

func PartOne() {

	scanner := bufio.NewScanner(os.Stdin)
	grid := make([][]Visibility, 0)

	for {
		input := scanLine(scanner)
		if len(input) == 0 {
			break
		}

		line := make([]Visibility, 0)
		for _, char := range input {
			if char != '\n' {
				height := int(char) - int('0')
				line = append(line, Visibility{height: height})
			}
		}

		grid = append(grid, line)
	}

	height := len(grid)
	width := len(grid[0])

	for y := 0; y < height; y++ {
		grid[y][0].isOutside = true
		grid[y][width-1].isOutside = true
	}
	for x := 0; x < width; x++ {
		grid[0][x].isOutside = true
		grid[height-1][x].isOutside = true
	}

	for y := 0; y < height; y++ {
		for x := 0; x < width; x++ {
			calculateVisibility(x, y, grid)
		}
	}

	result := 0
	for y := 0; y < height; y++ {
		for x := 0; x < width; x++ {
			result += grid[y][x].IsVisible()
			fmt.Printf("%d", grid[y][x].IsVisible())
		}
		fmt.Printf("\n")
	}

	fmt.Println(result)
}

func PartTwo() {
	scanner := bufio.NewScanner(os.Stdin)
	grid := make([][]Visibility, 0)

	for {
		input := scanLine(scanner)
		if len(input) == 0 {
			break
		}

		line := make([]Visibility, 0)
		for _, char := range input {
			if char != '\n' {
				height := int(char) - int('0')
				line = append(line, Visibility{height: height})
			}
		}

		grid = append(grid, line)
	}

	height := len(grid)
	width := len(grid[0])

	for y := 0; y < height; y++ {
		grid[y][0].isOutside = true
		grid[y][width-1].isOutside = true
	}
	for x := 0; x < width; x++ {
		grid[0][x].isOutside = true
		grid[height-1][x].isOutside = true
	}

	result := 0
	for y := 0; y < height; y++ {
		for x := 0; x < width; x++ {
			score := calculateScore(x, y, grid)
			//fmt.Printf("%d ", score)
			if result < score {
				result = score
				//fmt.Printf("updated at: (%d, %d), %d \n", x, y, result)
			}
		}
		//fmt.Printf("\n")
	}

	fmt.Println(result)
}
