package apigateway

import (
	"context"
	"encoding/json"
	"fmt"
	lbapidynamodb "legalbrawlapi/aws/dynamodb"
	lbapiconfig "legalbrawlapi/config"
	"legalbrawlapi/secret"
	"log"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-secretsmanager-caching-go/secretcache"
)

var (
	secretCache, _ = secretcache.New()
	env            = lbapiconfig.New()
)

type Body struct {
	Handinfo HandInfo `json:"handinfo"`
}

type HandInfo struct {
	Name     string  `json:"playerName"`
	PlayerId string  `json:"playerId"`
	Version  string  `json:"version"`
	Cards    []int16 `json:"cards"`
}

// Parse the body of request (in JSON) into usable structs
func ParseBody(b string) (*Body, error) {
	var body Body
	err := json.Unmarshal([]byte(b), &body)
	if err != nil {
		return nil, err
	}

	return &body, nil
}

func HandleRequest(ctx context.Context, request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	fmt.Printf("Processing request data for request %s.\n", request.RequestContext.RequestID)

	apiSecret := secret.RetrieveSecrets(ctx, secretCache, env.LegalBrawlSecretName)
	secret.ScrubRequest(&request, apiSecret)
	fmt.Printf("Body size = %d.\n", len(request.Body))

	fmt.Println("Headers:")
	for key, value := range request.Headers {
		fmt.Printf("    %s: %s\n", key, value)
	}

	var content string
	switch request.Path {
	case "/v1/formationId":
		content = "Hello, World!"
	case "/v1/playerHand":
		if request.HTTPMethod == "POST" {
			pB, err := ParseBody(request.Body)
			if err != nil {
				return events.APIGatewayProxyResponse{Body: "Error", StatusCode: 500}, fmt.Errorf("error: %v", err)
			}
			//INVOKE THE DYNAMO DB WRITER LOGIC
			var hand lbapidynamodb.Hand
			hand.PlayerName = pB.Handinfo.Name
			hand.PlayerId = pB.Handinfo.PlayerId
			hand.Version = pB.Handinfo.Version
			hand.Cards = pB.Handinfo.Cards //Hands are showing as null

			cfg, err := config.LoadDefaultConfig(ctx)
			if err != nil {
				log.Fatalf("error loading SDK config: %v\n", err)
			}
			dDBHandler := lbapidynamodb.DDBHandler{
				DynamoDbClient: dynamodb.NewFromConfig(cfg),
				TableName:      env.PlayerHandTableName,
			}
			err = lbapidynamodb.DDBHandler.AddHand(dDBHandler, hand)
			if err != nil {
				log.Fatalf("error adding item to table: %v", err)
			}
			content = "Peeps that database"
		}

	}

	return events.APIGatewayProxyResponse{Body: content, StatusCode: 200}, nil
}
