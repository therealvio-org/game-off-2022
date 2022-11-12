package lbapiaws

import (
	"context"
	"log"

	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go/aws"
)

type DDBHandler struct {
	DynamoDbClient *dynamodb.Client
	TableName      string
}

type Hand struct {
	PlayerName string  `dynamodbav:"playerName"`
	PlayerId   string  `dynamodbav:"playerId"`
	Version    string  `dynamodbav:"version"`
	Cards      []int16 `dynamodbav:"cards"`
}

// TODO: Handle Duplicates - this should return an error
// TODO: Need to check that types match before marshalling
func (ddbh DDBHandler) AddHand(h Hand) error {
	item, err := attributevalue.MarshalMap(h)
	if err != nil {
		log.Panicf("unable to marshal submitted hand: %v", err)
	}

	_, err = ddbh.DynamoDbClient.PutItem(context.TODO(), &dynamodb.PutItemInput{
		TableName: aws.String(ddbh.TableName),
		Item:      item,
	})
	if err != nil {
		log.Printf("couldn't add item to table: %v\n", err)
	}

	return err
}
