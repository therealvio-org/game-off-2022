package lbapiaws

import (
	"context"
	"encoding/json"
	"fmt"
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

// Parse the body of request (in JSON) into usable structs
func parseBody(b string) (*body, error) {
	var body body
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
			parsedBody, err := parseBody(request.Body)
			if err != nil {
				return events.APIGatewayProxyResponse{Body: "Error", StatusCode: 500}, fmt.Errorf("error: %v", err)
			}

			cfg, err := config.LoadDefaultConfig(ctx)
			if err != nil {
				log.Fatalf("error loading SDK config: %v\n", err)
			}
			dynamoHandler := dDBHandler{
				DynamoDbClient: dynamodb.NewFromConfig(cfg),
				TableName:      env.PlayerHandTableName,
			}
			err = dDBHandler.addHand(dynamoHandler, parsedBody.HandInfo)
			if err != nil {
				log.Fatalf("error adding item to table: %v", err)
			}
			content = "Peeps that database"
		}

	}

	return events.APIGatewayProxyResponse{Body: content, StatusCode: 200}, nil
}
