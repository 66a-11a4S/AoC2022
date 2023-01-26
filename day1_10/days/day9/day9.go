package day9

/*
day9

* ロープのシミュレートをする
* 頭が尾から離れると、尻尾は頭の方に引っ張られる

* 頭(H)と尾(T)は常に接触している必要がある
  * 対角線に隣接してる場合、重なってる場合も接触してるとみなす
* 頭が尻尾から同じ行・列に1マス離れているとき、尻尾もその方向に1歩移動
* 頭と尾が接触しておらず、同じ行・列にない場合、尾は1歩斜めに移動
* 初期状態で H と T は重なっている

* 入力は一連の頭の移動
  * UDLR(上下左右) に x マス動く

Q1. 一連の動きをしたあと、尾が1回以上訪れたマスはいくつ?

Q2.

* ロープを10マスに拡張する
* 各部分は Q1 の T と同じ動きをする
* ロープの尾が1回以上訪れたマスはいくつ?
*/

import (
	"bufio"
	"fmt"
	"math"
	"os"
	"strconv"
	"strings"

	"github.com/emirpasic/gods/sets/hashset"
)

type Direction int

const (
	Up Direction = iota
	Down
	Right
	Left
)

type Command struct {
	direction Direction
	value     int
}

type Point struct {
	x int
	y int
}

func parseCommand(line string) Command {

	input := strings.Split(line, " ")
	var direction Direction

	switch input[0] {
	case "U":
		direction = Up
	case "D":
		direction = Down
	case "R":
		direction = Right
	case "L":
		direction = Left
	}

	value, _ := strconv.Atoi(input[1])
	return Command{direction: direction, value: value}
}

func scanLine(sc *bufio.Scanner) string {
	sc.Scan()
	return sc.Text()
}

func printPosition(hX int, hY int, tX int, tY int, visitedTable hashset.Set) {

	width := 6
	height := 5

	for y := height - 1; 0 <= y; y-- {
		for x := 0; x < width; x++ {

			if visitedTable.Contains(convertToString(x, y)) {
				fmt.Print("#")
				continue
			}

			if x == hX && y == hY {
				fmt.Print("H")
			} else if x == tX && y == tY {
				fmt.Print("T")
			} else {
				fmt.Printf(".")
			}

		}

		fmt.Printf("\n")
	}
}

func printAttitude(body []Point, visitedTable hashset.Set) {

	width := 10
	height := 10

	for y := height - 1; 0 <= y; y-- {
		for x := 0; x < width; x++ {

			containsValue := -1
			for idx := 0; idx < len(body); idx++ {
				if x == body[idx].x && y == body[idx].y {
					containsValue = idx
					fmt.Printf("%d", idx)
					break
				}
			}

			if containsValue == -1 {
				fmt.Printf(".")
			}
		}

		fmt.Printf("\n")
	}
}

func convertToString(x int, y int) string {
	str := strconv.Itoa(x)
	str += " , "
	str += strconv.Itoa(y)
	return str
}

func convertToStringFromPoint(p *Point) string {
	return convertToString(p.x, p.y)
}

func isAdjacent(x1 int, y1 int, x2 int, y2 int) bool {
	diffX := math.Abs(float64(x2 - x1))
	diffY := math.Abs(float64(y2 - y1))

	if 3 <= diffX+diffY {
		return false
	}

	if (1 < diffX) || (1 < diffY) {
		return false
	}

	return true
}

func getTailMovedPosition(x int, diff int) int {
	if 0 < diff {
		x += 1
	} else if diff < 0 {
		x -= 1
	}

	return x
}

func PartOne() {

	scanner := bufio.NewScanner(os.Stdin)
	commands := make([]Command, 0)

	for {
		line := scanLine(scanner)

		if len(line) == 0 {
			break
		}

		command := parseCommand(line)
		commands = append(commands, command)
	}

	hX := 0
	hY := 0
	tX := 0
	tY := 0

	visitedPoints := hashset.New()
	visitedPoints.Add(convertToString(tX, tY))

	for _, command := range commands {
		for count := 0; count < command.value; count++ {

			//fmt.Printf("%d, %d \n", command.direction, command.value)

			switch command.direction {
			case Up:
				hY++
			case Down:
				hY--
			case Right:
				hX++
			case Left:
				hX--
			}

			if !isAdjacent(hX, hY, tX, tY) {

				diffX := hX - tX
				diffY := hY - tY

				if 1 < diffY {
					tY += 1
					tX += diffX
				} else if diffY < -1 {
					tY -= 1
					tX += diffX
				} else if 1 < diffX {
					tX += 1
					tY += diffY
				} else if diffX < -1 {
					tX -= 1
					tY += diffY
				}

			}

			visitedPoints.Add(convertToString(tX, tY))
			//printPosition(hX, hY, tX, tY, *visitedPoints)
		}
		//fmt.Printf("\n")
	}

	fmt.Println(visitedPoints.Size())
}

func PartTwo() {
	scanner := bufio.NewScanner(os.Stdin)
	commands := make([]Command, 0)

	for {
		line := scanLine(scanner)

		if len(line) == 0 {
			break
		}

		command := parseCommand(line)
		commands = append(commands, command)
	}

	const bodyLength = 10
	body := [bodyLength]Point{}

	visitedPoints := hashset.New()
	visitedPoints.Add(convertToStringFromPoint(&body[bodyLength-1]))

	for _, command := range commands {
		for count := 0; count < command.value; count++ {

			switch command.direction {
			case Up:
				body[0].y++
			case Down:
				body[0].y--
			case Right:
				body[0].x++
			case Left:
				body[0].x--
			}

			//fmt.Printf("%d, %d \n", command.direction, command.value)
			for idx := 1; idx < bodyLength; idx++ {

				hX := body[idx-1].x
				hY := body[idx-1].y
				tX := body[idx].x
				tY := body[idx].y

				if !isAdjacent(hX, hY, tX, tY) {

					diffX := hX - tX
					diffY := hY - tY

					if 1 < diffY {
						tY += 1
						tX = getTailMovedPosition(tX, diffX)
					} else if diffY < -1 {
						tY -= 1
						tX = getTailMovedPosition(tX, diffX)
					} else if 1 < diffX {
						tX += 1
						tY = getTailMovedPosition(tY, diffY)
					} else if diffX < -1 {
						tX -= 1
						tY = getTailMovedPosition(tY, diffY)
					}
				}

				body[idx].x = tX
				body[idx].y = tY
			}

			visitedPoints.Add(convertToStringFromPoint(&body[bodyLength-1]))
			//printAttitude(body[:], *visitedPoints)
		}
		//fmt.Printf("\n")
	}

	fmt.Println(visitedPoints.Size())
}
