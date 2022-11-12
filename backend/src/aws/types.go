package lbapiaws

import "github.com/aws/aws-sdk-go-v2/service/dynamodb"

type body struct {
	HandInfo handInfo `json:"handinfo"`
}

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
