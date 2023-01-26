package day2

/*
day2

じゃんけんトーナメントが行われ、2人でじゃんけんしていく.
1人のエルフが暗号化された戦略ガイドをくれた

* 各行には1ラウンドごとのじゃんけんの指示
* 1文字目mは対戦相手が出す手
  * A は rock, B は paper, C は scissors.
* 毎回勝つのは怪しいので、2文字目は、それに応じて出すべき手
  * X は rock, Y は paper、Z は scissors .

トーナメント全体の勝者は、最高得点のプレイヤーである
各ラウンドごとにスコアを出す.
* 1ラウンドの得点は、選択した手の得点 = (rock: 1, paper: 2, scissors: 3) + ラウンドの結果 (負け: 0, 引き分け: 3, 勝ち: 6)

Q1. 戦略ガイドどおりに進んだ場合、合計スコアはいくつ?

Q2.

戦略リストの指示を読み替える

* X は負けるべきであることを意味する
* Y は引き分けるべき, Zは勝つべきであることを意味する

これで戦略ガイド通りに進んで得られる合計スコアはいくつ?
*/

import (
	"bufio"
	"fmt"
	"os"
	"strings"
)

func scanRound(sc *bufio.Scanner) (string, string, bool) {
	sc.Scan()

	str := sc.Text()

	hands := strings.Split(str, " ")
	if len(hands) < 2 {
		return "", "", false
	}

	return hands[0], hands[1], true
}

func getHandScore(self string) int {

	switch self {
	case "X":
	case "a":
		return 1
	case "Y":
	case "b":
		return 2
	case "Z":
	case "c":
		return 3
	}

	return 0
}

func getResultScore(opponent string, self string) int {

	switch opponent {
	case "A":
		switch self {
		case "X":
		case "a":
			return 3
		case "Y":
		case "b":
			return 6
		case "Z":
		case "c":
			return 0
		}

	case "B":
		switch self {
		case "X":
		case "a":
			return 0
		case "Y":
		case "b":
			return 3
		case "Z":
		case "c":
			return 6
		}

	case "C":
		switch self {
		case "X":
		case "a":
			return 6
		case "Y":
		case "b":
			return 0
		case "Z":
		case "c":
			return 3
		}
	}

	return 0
}

func getScore(opponent string, self string) int {
	return getHandScore(self) + getResultScore(opponent, self)
}

func getGuessedHand(opponent string, roundResult string) string {

	switch opponent {
	// rock
	case "A":
		switch roundResult {
		// まけろ
		case "X":
			return "c"
		// 引き分けろ
		case "Y":
			return "a"
		// かて
		case "Z":
			return "b"
		}

	// paper
	case "B":
		switch roundResult {
		// まけろ
		case "X":
			return "a"
		// 引き分けろ
		case "Y":
			return "b"
		// かて
		case "Z":
			return "c"
		}

	// scissors
	case "C":
		switch roundResult {
		// まけろ
		case "X":
			return "b"
		// ひきわけろ
		case "Y":
			return "c"
		// かて
		case "Z":
			return "a"
		}
	}

	return ""
}

func getGuessedScore(opponent string, roundResult string) int {

	self := getGuessedHand(opponent, roundResult)
	return getHandScore(self) + getResultScore(opponent, self)
}

func PartOne() {

	sc := bufio.NewScanner(os.Stdin)
	result := 0

	for {
		opponent, self, success := scanRound(sc)

		if !success {
			break
		}

		result += getScore(opponent, self)
	}

	fmt.Println(result)
}

func PartTwo() {

	sc := bufio.NewScanner(os.Stdin)
	result := 0

	for {
		opponent, round, success := scanRound(sc)

		if !success {
			break
		}

		result += getGuessedScore(opponent, round)
	}

	fmt.Println(result)
}
