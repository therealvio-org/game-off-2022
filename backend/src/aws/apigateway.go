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

			err = validateBody(*parsedBody)
			if err != nil {
				return events.APIGatewayProxyResponse{
					Body:       "parameters in request failed validation",
					StatusCode: 500,
				}, fmt.Errorf("body contents failed validation: %v", err)
			}

			cfg, err := config.LoadDefaultConfig(ctx)
			if err != nil {
				log.Fatalf("error loading sdk config: %v\n", err)
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
					Body:       "error checking for duplicate player",
					StatusCode: 500,
				}, fmt.Errorf("failed to check for duplicate playerId %v: %v", parsedBody.HandInfo.PlayerId, err)
			}
			if playerExist { //Player exists, use PUT request to update player instead
				log.Printf("playerId %v already exists in database", parsedBody.HandInfo.PlayerId)
				response = events.APIGatewayProxyResponse{
					Body:       "submitted player info already exists in database",
					StatusCode: 400,
				}
			} else { //Add player hand to the DB
				err = dDBHandler.addHand(dynamoHandler, parsedBody.HandInfo)
				if err != nil {
					log.Fatalf("error adding item to table: %v", err)
				}
				log.Printf("playerId %v added to database", parsedBody.HandInfo.PlayerId)
				response = events.APIGatewayProxyResponse{
					Body:       "submitted player info successfully added to database",
					StatusCode: 200,
				}
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

			playerExists, _, err := dynamoHandler.checkIfPlayerHandExists(parsedParameters)
			if err != nil {
				log.Fatalf("failed to determine if player exists on database: %v", err)
			}
			if playerExists {
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
			parsedBody, err := parseBody(request.Body)
			if err != nil {
				return events.APIGatewayProxyResponse{
						Body:       "request body failed parsing",
						StatusCode: 500,
					},
					fmt.Errorf("error: %v", err)
			}

			err = validateBody(*parsedBody)
			if err != nil {
				return events.APIGatewayProxyResponse{
					Body:       "parameters in request failed validation",
					StatusCode: 500,
				}, fmt.Errorf("body contents failed validation: %v", err)
			}

			cfg, err := config.LoadDefaultConfig(ctx)
			if err != nil {
				log.Fatalf("error loading sdk config: %v\n", err)
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
					Body:       "error checking for duplicate player",
					StatusCode: 500,
				}, fmt.Errorf("failed to check for duplicate player: %v", err)
			}
			if playerExist { //Check PUT request
				isDupe, err := dynamoHandler.checkForDuplicate(parsedBody.HandInfo, playerEntry)
				if err != nil {
					return events.APIGatewayProxyResponse{
						Body:       "error checking for duplicate player",
						StatusCode: 500,
					}, fmt.Errorf("failed to check for duplicate player: %v", err)
				}
				if isDupe { //The card configuration needs to be different
					response = events.APIGatewayProxyResponse{
						Body:       fmt.Sprintf("submitted PUT request for playerId (%v) entry for version %v, is a duplicate", parsedBody.HandInfo.PlayerId, env.PlayerHandVersion),
						StatusCode: 400,
					}
				} else {
					cfg, err := config.LoadDefaultConfig(ctx)
					if err != nil {
						log.Fatalf("error loading sdk config: %v\n", err)
					}

					dynamoHandler := dDBHandler{
						DynamoDbClient: dynamodb.NewFromConfig(cfg),
						TableName:      env.PlayerHandTableName,
					}

					err = dynamoHandler.updatePlayerHand(parsedBody.HandInfo)
					if err != nil {
						return events.APIGatewayProxyResponse{
							Body:       "error updating playerHand",
							StatusCode: 500,
						}, fmt.Errorf("failed to update playerHand for playerId %v: %v", parsedBody.HandInfo.PlayerId, err)
					} else {
						response = events.APIGatewayProxyResponse{
							Body:       fmt.Sprintf("successfully updated playerHand entry for playerId %v", parsedBody.HandInfo.PlayerId),
							StatusCode: 200,
						}
					}

				}
			} else { //Recommend running POST request instead
				response = events.APIGatewayProxyResponse{
					Body:       "player does not exist in database - send a POST request instead",
					StatusCode: 200,
				}
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
