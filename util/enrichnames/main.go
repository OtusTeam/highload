package main

import (
	"bufio"
	"fmt"
	"log"
	"math/rand"
	"os"
)

func main() {
	namesFile, err := os.Open("../../homework/people_raw.csv")
	if err != nil {
		log.Fatal(err)
		return
	}
	defer namesFile.Close()

	citiesFile, err := os.Open("../../homework/cities_raw.csv")
	if err != nil {
		log.Fatal(err)
		return
	}
	defer citiesFile.Close()
	var cities []string
	scanner := bufio.NewScanner(citiesFile)
	for scanner.Scan() {
		cities = append(cities, scanner.Text())
	}
	err = scanner.Err()
	if err != nil {
		log.Fatal(err)
	}

	resultFile, err := os.Create("../../homework/people.csv")
	if err != nil {
		log.Fatal(err)
		return
	}
	defer resultFile.Close()

	scanner = bufio.NewScanner(namesFile)
	for scanner.Scan() {
		name := scanner.Text()
		age := rand.Intn(120-10) + 10
		city := cities[rand.Intn(len(cities)-1)]
		_, err := fmt.Fprintf(resultFile, "%s,%d,%s\r\n", name, age, city)
		if err != nil {
			log.Fatal(err)
			return
		}
	}

	err = scanner.Err()
	if err != nil {
		log.Fatal(err)
	}
}
