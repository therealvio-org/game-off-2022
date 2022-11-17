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
	"github.com/google/uuid"
	"github.com/mitchellh/mapstructure"
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

func parseParameters(p map[string]string) (GetMethodParameters, error) {
	input := make(map[string]interface{})

	for k, v := range p {
		input[k] = v
	}

	var result GetMethodParameters
	err := mapstructure.Decode(input, &result)
	if err != nil {
		return GetMethodParameters{}, fmt.Errorf("unable to decode get request parameters: %v", err)
	}

	return result, nil
}

func validateParameters(pg GetMethodParameters) error {
	err := validatePlayerId(pg.PlayerId)
	if err != nil {
		return fmt.Errorf("playerId is not a valid UUID format: %v", err)
	}
	return nil
}

func validatePlayerId(pId string) error {
	_, err := uuid.Parse(pId)
	if err != nil {
		return fmt.Errorf("playerId is not a valid UUID format: %v", err)
	}
	return nil
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
			parsedParameters, err := parseParameters(request.QueryStringParameters)
			if err != nil {
				return events.APIGatewayProxyResponse{
						Body:       "internal error parsing get request",
						StatusCode: 500,
					},
					fmt.Errorf("unable to parse parameters %v", err)
			}

			err = validateParameters(parsedParameters)
			if err != nil {
				return events.APIGatewayProxyResponse{
						Body:       "parameters in request failed validation",
						StatusCode: 500,
					},
					fmt.Errorf("parameters failed validation: %v", err)
			}

			cfg, err := config.LoadDefaultConfig(ctx)
			if err != nil {
				log.Fatalf("error loading sdk config: %v\n", err)
			}

			dynamoHandler := dDBHandler{
				DynamoDbClient: dynamodb.NewFromConfig(cfg),
				TableName:      env.PlayerHandTableName,
			}

			playerExist, err := dynamoHandler.checkIfPlayerHandExists(parsedParameters, env)
			if err != nil {
				log.Fatalf("failed to determine if player exists on database: %v", err)
			}
			if playerExist {
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
			} else {
				return events.APIGatewayProxyResponse{
						Body:       "player does not exist on database",
						StatusCode: 400,
					},
					fmt.Errorf("error: invalid method used: %v", request.HTTPMethod)
			}

		case "PUT":
			response = events.APIGatewayProxyResponse{
				Body:       "PUT method invoked!",
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
