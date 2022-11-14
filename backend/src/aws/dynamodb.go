package lbapiaws

import (
	"context"
	"fmt"
	"log"
	"math/rand"
	"time"

	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
	"github.com/aws/aws-sdk-go/aws"
)

// Adds the player's submitted hand from the client to the Database.
//
// BUG: Need to check that types match, and contents are not null before marshalling.
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

// Queries *all* entries in the database by version number.
//
// This operation is gonna be expensive for a Lambda later on, so this result will eventually need
// to be a redis cache later.
//
// TODO: Put in timeout
func (ddbh dDBHandler) queryHands(version string) ([]handInfo, error) {
	var availableHands []handInfo
	var response *dynamodb.QueryOutput

	keyEx := expression.Key("version").Equal(expression.Value(version))
	expr, err := expression.NewBuilder().WithKeyCondition(keyEx).Build()
	if err != nil {
		return availableHands, fmt.Errorf("could not build expression for query. error: %v", err)
	} else {
		response, err = ddbh.DynamoDbClient.Query(context.TODO(), &dynamodb.QueryInput{
			TableName:                 aws.String(ddbh.TableName),
			ExpressionAttributeNames:  expr.Names(),
			ExpressionAttributeValues: expr.Values(),
			KeyConditionExpression:    expr.KeyCondition(),
		})
		if err != nil {
			return availableHands, fmt.Errorf("could not query for playerHands in v%v. error: %v", version, err)
		} else {
			err = attributevalue.UnmarshalListOfMaps(response.Items, &availableHands)
			if err != nil {
				return availableHands, fmt.Errorf("couldn't unmarshal query response. error: %v", err)
			}
		}
	}

	fmt.Printf("%v", availableHands)

	return availableHands, nil
}

// Creates the composite key for the playerHand dynamodb Table
//
// Use this function if you need to specifically target a player in the database
func (h handInfo) GetKey() (map[string]types.AttributeValue, error) {
	version, err := attributevalue.Marshal(h.Version)
	if err != nil {
		return nil, fmt.Errorf("unable to marshal attribute 'version' with value %v, error: %v", h.Version, err)
	}
	playerId, err := attributevalue.Marshal(h.PlayerId)
	if err != nil {
		return nil, fmt.Errorf("unable to marshal attribute 'playerId' with value %v, error: %v", h.PlayerId, err)
	}
	return map[string]types.AttributeValue{"version": version, "playerId": playerId}, nil
}

// Selects a random entry in []handInfo
//
// It ain't matchmaking, but it's honest work
func (ddbh dDBHandler) chooseHand(h []handInfo) handInfo {
	rand.Seed(time.Now().Unix())
	selectedHand := h[rand.Intn(len(h))]
	return selectedHand
}
