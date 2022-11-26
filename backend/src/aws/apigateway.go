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

func HandleRequest(ctx context.Context, request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	var response events.APIGatewayProxyResponse

	httpMethodInput := httpMethodInput{
		ctx:     ctx,
		request: request,
		headers: map[string]string{
			"Access-Control-Allow-Headers": "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token",
			"Access-Control-Allow-Origin":  "*",
			"Access-Control-Allow-Methods": "OPTIONS,POST,GET,PUT",
		},
	}

	fmt.Printf("Processing request data for request %s.\n", request.RequestContext.RequestID)

	apiSecret, err := secret.RetrieveSecrets(ctx, secretCache, env.LegalBrawlSecretName)
	if err != nil {
		return events.APIGatewayProxyResponse{}, fmt.Errorf("error retrieving api secrets: %v", err)
	}
	secret.ScrubRequest(&request, apiSecret)
	fmt.Printf("Body size = %d.\n", len(request.Body))

	fmt.Println("Headers:")
	for key, value := range request.Headers {
		fmt.Printf("    %s: %s\n", key, value)
	}

	switch request.Path {

	case "/v1/playerHand":
		switch request.HTTPMethod {

		case "OPTIONS":
			result, err := handleOptions(httpMethodInput.headers)
			if err != nil {
				return result, fmt.Errorf("%v", err)
			}
			return result, nil

		case "POST":
			result, err := handlePost(httpMethodInput)
			if err != nil {
				return result, fmt.Errorf("%v", err)
			}
			return result, nil

		case "GET":
			result, err := handleGet(httpMethodInput)
			if err != nil {
				return result, fmt.Errorf("%v", err)
			}
			return result, nil

		case "PUT":
			result, err := handlePut(httpMethodInput)
			if err != nil {
				return result, fmt.Errorf("%v", err)
			}
			return result, nil

		default:
			return events.APIGatewayProxyResponse{
				Body:       "invalid method used",
				StatusCode: 501,
			}, nil
		}
	}

	return response, nil
}

func handlePost(hmi httpMethodInput) (events.APIGatewayProxyResponse, error) {

	parsedBody, err := parseBody(hmi.request.Body)
	if err != nil {
		return events.APIGatewayProxyResponse{
				Headers:    hmi.headers,
				Body:       "internal error parsing request",
				StatusCode: 500,
			},
			fmt.Errorf("request body failed parsing: %v", err)
	}

	err = validateBody(*parsedBody)
	if err != nil {
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "parameters in request failed validation",
			StatusCode: 500,
		}, fmt.Errorf("body contents failed validation: %v", err)
	}

	cfg, err := config.LoadDefaultConfig(hmi.ctx)
	if err != nil {
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "internal error",
			StatusCode: 500,
		}, fmt.Errorf("error loading sdk config: %v", err)
	}

	dynamoHandler := dDBHandler{
		DynamoDbClient: dynamodb.NewFromConfig(cfg),
		TableName:      env.PlayerHandTableName,
	}

	playerHandParams := playerHandCompositeKey{
		PlayerId: parsedBody.HandInfo.PlayerId,
		Version:  env.PlayerHandVersion,
	}
	playerExist, _, err := dynamoHandler.checkIfPlayerHandExists(playerHandParams)
	if err != nil {
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "error checking for duplicate player",
			StatusCode: 500,
		}, fmt.Errorf("failed to check for duplicate playerId %v in playerHand table: %v", parsedBody.HandInfo.PlayerId, err)
	}
	if playerExist { //Player exists, use PUT request to update player instead
		log.Printf("specified playerId %v already exists", parsedBody.HandInfo.PlayerId)
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "playerId already exists",
			StatusCode: 400,
		}, nil
	} else { //Add player hand to the DB
		err = dDBHandler.addHand(dynamoHandler, parsedBody.HandInfo)
		if err != nil {
			log.Fatalf("error adding playerId %v to playerHand table: %v", parsedBody.HandInfo.PlayerId, err)
			return events.APIGatewayProxyResponse{
				Headers:    hmi.headers,
				Body:       "error adding item",
				StatusCode: 500,
			}, fmt.Errorf("failed to add playerId %v to playerHand table: %v", parsedBody.HandInfo.PlayerId, err)
		}
		log.Printf("playerId %v added to playerHand table", parsedBody.HandInfo.PlayerId)
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "submitted player info successfully added",
			StatusCode: 200,
		}, nil
	}
}
func handleGet(hmi httpMethodInput) (events.APIGatewayProxyResponse, error) {
	parsedParameters, err := parseParameters(hmi.request.QueryStringParameters)
	if err != nil {
		return events.APIGatewayProxyResponse{
				Headers:    hmi.headers,
				Body:       "internal error parsing request",
				StatusCode: 500,
			},
			fmt.Errorf("unable to parse parameters %v", err)
	}

	err = validateParameters(parsedParameters)
	if err != nil {
		return events.APIGatewayProxyResponse{
				Headers:    hmi.headers,
				Body:       "parameters in request failed validation",
				StatusCode: 500,
			},
			fmt.Errorf("parameters failed validation: %v", err)
	}

	cfg, err := config.LoadDefaultConfig(hmi.ctx)
	if err != nil {
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "internal error",
			StatusCode: 500,
		}, fmt.Errorf("error loading sdk config: %v", err)
	}

	dynamoHandler := dDBHandler{
		DynamoDbClient: dynamodb.NewFromConfig(cfg),
		TableName:      env.PlayerHandTableName,
	}

	playerExists, _, err := dynamoHandler.checkIfPlayerHandExists(parsedParameters)
	if err != nil {
		log.Fatalf("failed to determine if player exists on playerHand table: %v", err)
	}
	if playerExists {
		hands, err := dynamoHandler.queryHands(env.PlayerHandVersion)
		if err != nil {
			log.Fatalf("error querying playerHands table: %v", err)
		}

		selectedHand := dynamoHandler.chooseHand(hands)

		selectedHandJson, err := json.Marshal(selectedHand)
		if err != nil {
			log.Fatalf("error marshalling selectedHand to json: %v", err)
			return events.APIGatewayProxyResponse{
				Headers:    hmi.headers,
				Body:       "internal error",
				StatusCode: 500,
			}, fmt.Errorf("error marshalling selectedHand to json: %v", err)
		}

		log.Printf("matchmaking: requesting playerId %v is matched up again playerId %v", parsedParameters.PlayerId, selectedHand.PlayerId)
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       fmt.Sprintf("%v", string(selectedHandJson)),
			StatusCode: 200,
		}, nil
	} else {
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "playerId does not exist",
			StatusCode: 404,
		}, nil
	}
}
func handlePut(hmi httpMethodInput) (events.APIGatewayProxyResponse, error) {
	parsedBody, err := parseBody(hmi.request.Body)
	if err != nil {
		return events.APIGatewayProxyResponse{
				Headers:    hmi.headers,
				Body:       "internal error parsing request",
				StatusCode: 500,
			},
			fmt.Errorf("error: %v", err)
	}

	err = validateBody(*parsedBody)
	if err != nil {
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "parameters in request failed validation",
			StatusCode: 500,
		}, fmt.Errorf("body contents failed validation: %v", err)
	}

	cfg, err := config.LoadDefaultConfig(hmi.ctx)
	if err != nil {
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "internal error",
			StatusCode: 500,
		}, fmt.Errorf("error loading sdk config: %v", err)
	}

	dynamoHandler := dDBHandler{
		DynamoDbClient: dynamodb.NewFromConfig(cfg),
		TableName:      env.PlayerHandTableName,
	}

	playerHandParams := playerHandCompositeKey{
		PlayerId: parsedBody.HandInfo.PlayerId,
		Version:  env.PlayerHandVersion,
	}

	playerExist, playerEntry, err := dynamoHandler.checkIfPlayerHandExists(playerHandParams)
	if err != nil {
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "error checking for duplicate player",
			StatusCode: 500,
		}, fmt.Errorf("failed to check for duplicate player in playerHand table: %v", err)
	}
	if playerExist { //Check PUT request
		isDupe, err := dynamoHandler.checkForDuplicate(parsedBody.HandInfo, playerEntry)
		if err != nil {
			return events.APIGatewayProxyResponse{
				Headers:    hmi.headers,
				Body:       "error checking for duplicate player",
				StatusCode: 500,
			}, fmt.Errorf("failed to check for duplicate player: %v", err)
		}
		if isDupe { //The card configuration needs to be different
			return events.APIGatewayProxyResponse{
				Headers:    hmi.headers,
				Body:       "specified card configuration for playerId already exists",
				StatusCode: 403,
			}, nil
		} else {
			err = dynamoHandler.updatePlayerHand(parsedBody.HandInfo)
			if err != nil {
				return events.APIGatewayProxyResponse{
					Headers:    hmi.headers,
					Body:       "error updating playerHand",
					StatusCode: 500,
				}, fmt.Errorf("failed to update card configuration for playerId %v: %v", parsedBody.HandInfo.PlayerId, err)
			} else {
				log.Printf("updated playerHand for playerId %v", parsedBody.HandInfo.PlayerId)
				return events.APIGatewayProxyResponse{
					Headers:    hmi.headers,
					Body:       "card configuration for playerId successfully updated",
					StatusCode: 200,
				}, nil
			}

		}
	} else { //Player doesn't exist - the request should be a POST request instead
		log.Printf("playerId %v does not exist", parsedBody.HandInfo.PlayerId)
		return events.APIGatewayProxyResponse{
			Headers:    hmi.headers,
			Body:       "playerId does not exist",
			StatusCode: 404,
		}, nil
	}
}
func handleOptions(headers map[string]string) (events.APIGatewayProxyResponse, error) {
	return events.APIGatewayProxyResponse{
		Headers:    headers,
		Body:       "Options Method invoked successfully!",
		StatusCode: 200,
	}, nil
}

// Parse the body of request (in JSON) into usable structs
func parseBody(b string) (*body, error) {
	var body body
	err := json.Unmarshal([]byte(b), &body)
	if err != nil {
		return nil, err
	}

	return &body, nil
}

func parseParameters(p map[string]string) (playerHandCompositeKey, error) {
	input := make(map[string]interface{})

	for k, v := range p {
		input[k] = v
	}

	var result playerHandCompositeKey
	err := mapstructure.Decode(input, &result)
	if err != nil {
		return playerHandCompositeKey{}, fmt.Errorf("unable to decode get request parameters: %v", err)
	}

	result.Version = env.PlayerHandVersion //Adds the balance version to the struct. Not supplied in GET request

	return result, nil
}

func validateBody(b body) error {
	_, err := uuid.Parse(b.HandInfo.PlayerId)
	if err != nil {
		return fmt.Errorf("playerId is not a valid UUID format: %v", err)
	}

	err = validateSubmittedVersion(b.HandInfo.Version, env.PlayerHandVersion)
	if err != nil {
		return fmt.Errorf("submitted version did not pass validation: %v", err)
	}

	return nil
}

func validateParameters(pg playerHandCompositeKey) error {
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

// Checks if submitted version matches current balance version
func validateSubmittedVersion(sv string, av string) error {
	if sv != av {
		return fmt.Errorf("submitted version (%v) in request, does not match actual balance version %v", sv, av)
	}
	return nil
}
