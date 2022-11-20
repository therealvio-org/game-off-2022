package lbapiaws

import (
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
)

type body struct {
	HandInfo handInfo `json:"handinfo"`
}

// `Version` refers to the balance version of cards. Functions concerning live service should be
// using the latest version defined in the `PlayerHandVersion` env var
type handInfo struct {
	PlayerName string  `json:"playerName" dynamodbav:"playerName"`
	PlayerId   string  `json:"playerId" dynamodbav:"playerId"`
	Version    string  `json:"version" dynamodbav:"version"`
	Cards      []int16 `json:"cards" dynamodbav:"cards"`
}

type dDBHandler struct {
	DynamoDbClient *dynamodb.Client
	TableName      string
}

type queryHandsResult struct {
	HandInfoSlice []handInfo
	Error         error
}

type checkPlayerHandExistsResult struct {
	PlayerExists bool
	PlayerItem   map[string]types.AttributeValue
	Error        error
}

type playerHandCompositeKey struct {
	PlayerId string
	Version  string
}
