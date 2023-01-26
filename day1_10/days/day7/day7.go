package day7

/*
day7

* デバイスの容量が足りないので、いらないファイルを消して空けることにした
* デバイスのCLIでは ls, cd コマンドが使える

Q1. デバイスの容量を空けるために 100000 までのディレクトリをすべて知りたい.
そのようなディレクトリの合計サイズはいくつ?

Q2.
* ファイルシステムで利用可能な合計ディスク容量は 7_000_0000
* そのうち少なくとも 3_000_0000 の空きが必要
* 更新を実行するのに十分な容量を解放する削除可能なディレクトリを見つける必要があります
* 十分な容量を空けるのに削除する必要がある最小のディレクトリのサイズ合計は?
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

type InputType int

const (
	Invalid InputType = iota
	Cd
	Ls
	State
)

type Directory struct {
	fileSize            int
	totalFileSize       int
	name                string
	parent              string
	compoundDirectories hashset.Set
}

func scanLine(sc *bufio.Scanner) string {
	sc.Scan()
	return sc.Text()
}

func parseInput(line string) (string, string, InputType) {

	elements := strings.Split(line, " ")

	if elements[0] == "$" {

		if elements[1] == "cd" {
			return elements[1], elements[2], Cd
		}

		if elements[1] == "ls" {
			return elements[1], "", Ls
		}

		return "", "", Invalid
	}

	if len(elements) == 2 {
		return elements[0], elements[1], State
	}

	return "", "", Invalid
}

func totalSize(name string, directories map[string]*Directory) int {

	size := directories[name].fileSize
	currentDirectory := directories[name]

	for _, directory := range currentDirectory.compoundDirectories.Values() {
		childName, _ := directory.(string)
		size += totalSize(childName, directories)
	}

	currentDirectory.totalFileSize = size
	return size
}

func PartOne() {

	scanner := bufio.NewScanner(os.Stdin)
	directories := map[string]*Directory{}

	currentDirectory := &Directory{fileSize: 0, name: "", parent: ""}
	for {

		line := scanLine(scanner)

		if len(line) == 0 {
			break
		}

		arg1, arg2, inputType := parseInput(line)
		if inputType == Cd {
			dest := arg2
			if dest == ".." {
				currentDirectory = directories[currentDirectory.parent]
			} else {

				if dest != "/" {
					dest = currentDirectory.name + "/" + dest
				}

				if _, exists := directories[dest]; !exists {
					directories[dest] = &Directory{
						name:                dest,
						parent:              currentDirectory.name,
						compoundDirectories: *hashset.New(),
					}
				}

				currentDirectory = directories[dest]
			}
			continue
		}

		if inputType == State {
			fileName := arg2
			if arg1 == "dir" {
				currentDirectory.compoundDirectories.Add(currentDirectory.name + "/" + fileName)
			} else {
				value, _ := strconv.Atoi(arg1)
				currentDirectory.fileSize += value
			}
			continue
		}

		if inputType == Ls {
			continue
		}

		errMss := line + "invalid command detected"
		panic(errMss)
	}

	totalSize("/", directories)

	result := 0
	for _, dir := range directories {
		if 0 < dir.totalFileSize && dir.totalFileSize < 100000 {
			result += dir.totalFileSize
			//fmt.Printf("%s, %d %d \n", key, dir.fileSize, dir.totalFileSize)
		}
	}

	fmt.Println(result)
}

func PartTwo() {
	scanner := bufio.NewScanner(os.Stdin)
	directories := map[string]*Directory{}

	currentDirectory := &Directory{fileSize: 0, name: "", parent: ""}
	for {

		line := scanLine(scanner)

		if len(line) == 0 {
			break
		}

		arg1, arg2, inputType := parseInput(line)
		if inputType == Cd {
			dest := arg2
			if dest == ".." {
				currentDirectory = directories[currentDirectory.parent]
			} else {

				if dest != "/" {
					dest = currentDirectory.name + "/" + dest
				}

				if _, exists := directories[dest]; !exists {
					directories[dest] = &Directory{
						name:                dest,
						parent:              currentDirectory.name,
						compoundDirectories: *hashset.New(),
					}
				}

				currentDirectory = directories[dest]
			}
			continue
		}

		if inputType == State {
			fileName := arg2
			if arg1 == "dir" {
				currentDirectory.compoundDirectories.Add(currentDirectory.name + "/" + fileName)
			} else {
				value, _ := strconv.Atoi(arg1)
				currentDirectory.fileSize += value
			}
			continue
		}

		if inputType == Ls {
			continue
		}

		errMss := line + "invalid command detected"
		panic(errMss)
	}

	totalSize("/", directories)

	// 使用済みファイルサイズをどれだけ減らすべきか
	requiredSize := directories["/"].totalFileSize - 40000000
	result := math.MaxInt
	for _, dir := range directories {
		if requiredSize <= dir.totalFileSize && dir.totalFileSize <= result {
			result = dir.totalFileSize
			//fmt.Printf("%s, %d %d \n", key, dir.fileSize, dir.totalFileSize)
		}
	}

	fmt.Println(result)
}
