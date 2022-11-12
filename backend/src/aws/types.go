package lbapiaws

import "github.com/aws/aws-sdk-go-v2/service/dynamodb"

type Body struct {
	HandInfo HandInfo `json:"handinfo"`
}

type HandInfo struct {
	PlayerName string  `json:"playerName" dynamodbav:"playerName"`
	PlayerId   string  `json:"playerId" dynamodbav:"playerId"`
	Version    string  `json:"version" dynamodbav:"version"`
	Cards      []int16 `json:"cards" dynamodbav:"cards"`
}

type DDBHandler struct {
	DynamoDbClient *dynamodb.Client
	TableName      string
}
