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
	"github.com/google/go-cmp/cmp"
)

var (
	timeoutWindow = 3 * time.Second
)

func (ddbh dDBHandler) addHand(ctx context.Context, h handInfo) error {
	err := make(chan error, 1)

	go func() {
		err <- ddbh.doAddHand(ctx, h)
	}()
	select {
	case <-time.After(timeoutWindow):
		return fmt.Errorf("timeout - could not add to playerHands table in allotted window")

	case err := <-err:
		if err != nil {
			return fmt.Errorf("addHand execution failed: %w", err)
		}
		return nil
	}
}

// Checks if the submitted playerHand item already exists. This is done by checking if the submitted
// hand is a 1:1 match with a pre-existing record. If it is, return true, else return false.
// The intention here is that it can be used to determine whether a POST request needs to in fact be
// a PUT request, and if an isolated PUT request isn't necessary.
//
// This method should be used for `PUT` requests, where a check needs to be made for every single
// attribute for a given item.
func (ddbh dDBHandler) checkForDuplicate(h handInfo, item map[string]types.AttributeValue) (bool, error) {
	var dbHand handInfo

	err := attributevalue.UnmarshalMap(item, &dbHand)
	if err != nil {
		return false, fmt.Errorf("failed to unmarshal response:  %w", err)
	}

	isDupe := cmp.Equal(h, dbHand)
	if isDupe {
		return true, nil
	} else {
		return false, nil
	}
}

// Checks if the player by specified playerId and version exists. If it does, return true, else
// return false. The intention for this is to determine if player has hand has previously been
// recorded on the Database.
//
// Unlike `checkForDuplicate`, this is only a partial check to verify if the player making a request
// is in the DB. It does not check if every attribute is a match.
// TODO: `checkForDuplicate` and `checkIfPlayerHandExists` could be made as a single method achieving
// the same thing
func (ddbh dDBHandler) checkIfPlayerHandExists(ctx context.Context, p playerHandCompositeKey) (bool, map[string]types.AttributeValue, error) {
	result := make(chan checkPlayerHandExistsResult, 1)

	go func() {
		result <- ddbh.doCheckIfPlayerHandExist(ctx, p)
	}()
	select {
	case <-time.After(timeoutWindow):
		return false, nil, fmt.Errorf("timeout - could not return existing player in allotted window")

	case result := <-result:
		return result.PlayerExists, result.PlayerItem, nil
	}
}

// Selects a random entry in []handInfo
//
// It ain't matchmaking, but it's honest work
func (ddbh dDBHandler) chooseHand(h []handInfo) handInfo {
	rand.Seed(time.Now().Unix())
	selectedHand := h[rand.Intn(len(h))]
	return selectedHand
}

// Adds the player's submitted hand from the client to the Database.
//
// BUG: Need to check that types match, and contents are not null before marshalling.
func (ddbh dDBHandler) doAddHand(ctx context.Context, h handInfo) error {
	item, err := attributevalue.MarshalMap(h)
	if err != nil {
		log.Panicf("unable to marshal submitted hand: %v", err)
	}

	_, err = ddbh.DynamoDbClient.PutItem(ctx, &dynamodb.PutItemInput{
		TableName: aws.String(ddbh.TableName),
		Item:      item,
	})
	if err != nil {
		log.Printf("couldn't add item to table: %v\n", err)
	}

	return err
}

func (ddbh dDBHandler) doCheckIfPlayerHandExist(ctx context.Context, p playerHandCompositeKey) checkPlayerHandExistsResult {
	playerInRequest := handInfo{
		PlayerId: p.PlayerId,
		Version:  p.Version, //Incoming request should always be the *latest* playerHand version when checking
	}

	var result handInfo

	dbKey, err := playerInRequest.getKey()
	if err != nil {
		return checkPlayerHandExistsResult{
			PlayerExists: false,
			PlayerItem:   nil,
			Error:        fmt.Errorf("failed to generate dbkey: %w", err),
		}
	}

	response, err := ddbh.DynamoDbClient.GetItem(ctx, &dynamodb.GetItemInput{
		Key:       dbKey,
		TableName: aws.String(ddbh.TableName),
	})
	if err != nil {
		return checkPlayerHandExistsResult{
			PlayerExists: false,
			PlayerItem:   nil,
			Error:        fmt.Errorf("failed to query player: %w", err),
		}
	} else {
		err = attributevalue.UnmarshalMap(response.Item, &result)
		if err != nil {
			return checkPlayerHandExistsResult{
				PlayerExists: false,
				PlayerItem:   nil,
				Error:        fmt.Errorf("failed to unmarshal response:  %w", err),
			}
		}
	}

	// using cmp.Equal checks for *all* values if they match. We can use the unmarshalled result
	// and use that to produce a partial struct like `playerInRequest` for comparison purposes only
	playerInDB := handInfo{
		PlayerId: result.PlayerId,
		Version:  result.Version,
	}

	playerExists := cmp.Equal(playerInRequest, playerInDB)
	if playerExists {
		return checkPlayerHandExistsResult{
			PlayerExists: true,
			PlayerItem:   response.Item,
			Error:        nil,
		}
	} else {
		return checkPlayerHandExistsResult{
			PlayerExists: false,
			PlayerItem:   nil,
			Error:        nil,
		}
	}
}

func (ddbh dDBHandler) doQueryHands(ctx context.Context, version string) queryHandsResult {
	var availableHands []handInfo
	var response *dynamodb.QueryOutput

	keyEx := expression.Key("version").Equal(expression.Value(version))
	expr, err := expression.NewBuilder().WithKeyCondition(keyEx).Build()
	if err != nil {
		return queryHandsResult{
			nil,
			fmt.Errorf("could not build expression to query available hands: %w", err),
		}
	} else {
		response, err = ddbh.DynamoDbClient.Query(ctx, &dynamodb.QueryInput{
			TableName:                 aws.String(ddbh.TableName),
			ExpressionAttributeNames:  expr.Names(),
			ExpressionAttributeValues: expr.Values(),
			KeyConditionExpression:    expr.KeyCondition(),
		})
		if err != nil {
			return queryHandsResult{
				nil,
				fmt.Errorf("could not query for playerHands in v%v: %w", version, err),
			}
		} else {
			err = attributevalue.UnmarshalListOfMaps(response.Items, &availableHands)
			if err != nil {
				return queryHandsResult{
					availableHands,
					fmt.Errorf("couldn't unmarshal query response: %w", err),
				}
			}
		}
	}

	return queryHandsResult{availableHands, nil}
}

func (ddbh dDBHandler) doUpdatePlayerHand(ctx context.Context, h handInfo) error {

	var attributeMap map[string]map[string]interface{}

	dbKey, err := h.getKey()
	if err != nil {
		return fmt.Errorf("failed to generate dbkey: %v", err)
	}

	update := expression.Set(expression.Name("cards"), expression.Value(h.Cards))
	expr, err := expression.NewBuilder().WithUpdate(update).Build()
	if err != nil {
		return fmt.Errorf("could not build expression for updating playerHand %v record: %v", h.PlayerId, err)
	}

	response, err := ddbh.DynamoDbClient.UpdateItem(ctx, &dynamodb.UpdateItemInput{
		Key:                       dbKey,
		TableName:                 &env.PlayerHandTableName,
		ExpressionAttributeNames:  expr.Names(),
		ExpressionAttributeValues: expr.Values(),
		UpdateExpression:          expr.Update(),
	})
	if err != nil {
		return fmt.Errorf("unable to update entry for playerId %v: %v", h.PlayerId, err)
	} else {
		err = attributevalue.UnmarshalMap(response.Attributes, &attributeMap)
		if err != nil {
			return fmt.Errorf("failed to unmarshal response:  %w", err)
		}

		return nil
	}
}

// Creates the composite key for the playerHand dynamodb Table
//
// Use this function if you need to specifically target a player in the database
func (h handInfo) getKey() (map[string]types.AttributeValue, error) {
	version, err := attributevalue.Marshal(h.Version)
	if err != nil {
		return nil, fmt.Errorf("unable to marshal attribute 'version' with value %v: %w", h.Version, err)
	}
	playerId, err := attributevalue.Marshal(h.PlayerId)
	if err != nil {
		return nil, fmt.Errorf("unable to marshal attribute 'playerId' with value %v: %w", h.PlayerId, err)
	}
	return map[string]types.AttributeValue{"version": version, "playerId": playerId}, nil
}

// Queries *all* entries in the database by version number.
//
// NOTE: This operation is gonna be expensive for a Lambda later on, so this result will eventually
// need to be cached later.
func (ddbh dDBHandler) queryHands(ctx context.Context, version string) ([]handInfo, error) {
	result := make(chan queryHandsResult, 1)

	go func() {
		result <- ddbh.doQueryHands(ctx, version)
	}()
	select {
	case <-time.After(timeoutWindow):
		return nil, fmt.Errorf("timeout - could not query for playerHands in allotted window")

	case result := <-result:
		return result.HandInfoSlice, nil
	}

}

func (ddbh dDBHandler) updatePlayerHand(ctx context.Context, h handInfo) error {
	err := make(chan error, 1)

	go func() {
		err <- ddbh.doUpdatePlayerHand(ctx, h)
	}()
	select {
	case <-time.After(timeoutWindow):
		return fmt.Errorf("timeout - could not update playerHand in allotted window")

	case err := <-err:
		if err != nil {
			return fmt.Errorf("updatePlayerHand execution failed: %w", err)
		}
		return nil
	}
}
