package main

import (
	"encoding/csv"
	"errors"
	"io"
	"log"
	"math/rand"
	"os"
	"strconv"
	"time"
)

func main() {
	filePath := "homework/people.csv"
	f, err := os.Open(filePath)
	if err != nil {
		log.Fatalf("can't open file '%s': %s", filePath, err)
	}

	newFilePath := "homework/people_new.csv"
	newF, err := os.Create(newFilePath)
	if err != nil {
		log.Fatalf("can't create file '%s': %s", newFilePath, err)
	}

	csvW := csv.NewWriter(newF)
	csvR := csv.NewReader(f)
	csvR.ReuseRecord = true
	timeNow := time.Now()
	for {
		record, err := csvR.Read()
		if errors.Is(err, io.EOF) {
			break
		} else if err != nil {
			log.Fatalf("can't read file '%s': %s", filePath, err)
		}

		age, err := strconv.Atoi(record[1])
		if err != nil {
			log.Fatalf("can't convert age '%s' to int: %s", record[1], err)
		}

		t := time.Date(timeNow.Year()-age, 1, 1, 0, 0, 0, 0, timeNow.Location())
		t.Add(time.Duration(rand.Intn(365)) * time.Hour * 24)
		record[1] = t.Format("2006-01-02")
		err = csvW.Write(record)
		if err != nil {
			log.Fatalf("can't write csv record to file '%s': %s", newFilePath, err)
		}
	}
}
