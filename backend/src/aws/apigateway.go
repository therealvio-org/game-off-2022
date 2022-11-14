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
	var response events.APIGatewayProxyResponse

	fmt.Printf("Processing request data for request %s.\n", request.RequestContext.RequestID)

	apiSecret := secret.RetrieveSecrets(ctx, secretCache, env.LegalBrawlSecretName)
	secret.ScrubRequest(&request, apiSecret)
	fmt.Printf("Body size = %d.\n", len(request.Body))

	fmt.Println("Headers:")
	for key, value := range request.Headers {
		fmt.Printf("    %s: %s\n", key, value)
	}

	switch request.Path {

	case "/v1/formationId":
		response = events.APIGatewayProxyResponse{
			Body:       "Hello, World!",
			StatusCode: 200,
		}

	case "/v1/playerHand":
		switch request.HTTPMethod {

		case "POST":
			parsedBody, err := parseBody(request.Body)
			if err != nil {
				return events.APIGatewayProxyResponse{
						Body:       "request body failed parsing",
						StatusCode: 500,
					},
					fmt.Errorf("error: %v", err)
			}

			cfg, err := config.LoadDefaultConfig(ctx)
			if err != nil {
				log.Fatalf("error loading sdk config: %v\n", err)
			}

			dynamoHandler := dDBHandler{
				DynamoDbClient: dynamodb.NewFromConfig(cfg),
				TableName:      env.PlayerHandTableName,
			}

			err = dDBHandler.addHand(dynamoHandler, parsedBody.HandInfo)
			if err != nil {
				log.Fatalf("error adding item to table: %v", err)
			}
			response = events.APIGatewayProxyResponse{
				Body:       "peeps that database (this is a placeholder, lmao)",
				StatusCode: 200,
			}

		case "GET":
			cfg, err := config.LoadDefaultConfig(ctx)
			if err != nil {
				log.Fatalf("error loading sdk config: %v\n", err)
			}

			dynamoHandler := dDBHandler{
				DynamoDbClient: dynamodb.NewFromConfig(cfg),
				TableName:      env.PlayerHandTableName,
			}

			hands, err := dynamoHandler.queryHands(env.PlayerHandVersion)
			if err != nil {
				log.Fatalf("error querying playerHands database: %v", err)
			}

			selectedHand := dynamoHandler.chooseHand(hands)

			selectedHandJson, err := json.Marshal(selectedHand)
			if err != nil {
				log.Fatalf("error marshalling selectedHand to json: %v", err)
			}

			response = events.APIGatewayProxyResponse{
				Body:       fmt.Sprintf("%v", string(selectedHandJson)),
				StatusCode: 200,
			}

		default:
			return events.APIGatewayProxyResponse{
					Body:       "invalid method returned",
					StatusCode: 500,
				},
				fmt.Errorf("error: invalid method used: %v", request.HTTPMethod)
		}
	}

	return response, nil
}
