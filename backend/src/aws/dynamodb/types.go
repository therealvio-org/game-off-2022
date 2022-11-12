package dynamodb

import (
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
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
