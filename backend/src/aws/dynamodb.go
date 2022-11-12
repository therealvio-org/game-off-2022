package lbapiaws

import (
	"context"
	"log"

	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go/aws"
)

// BUG: Need to check that types match, and contents are not null before marshalling.
//
// Adds the player's submitted hand from the client to the Database.
func (ddbh dDBHandler) addHand(h handInfo) error {
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
