package main

import (
	"context"
	"fmt"
	"legalbrawlapi/config"
	"legalbrawlapi/secret"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-secretsmanager-caching-go/secretcache"
)

var (
	secretCache, _ = secretcache.New()
	env            = config.New()
)

func HandleRequest(ctx context.Context, request events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	fmt.Printf("Processing request data for request %s.\n", request.RequestContext.RequestID)

	apiSecret := secret.RetrieveSecrets(ctx, secretCache, env.LegalBrawlSecretName)
	secret.ScrubRequest(&request, apiSecret)
	fmt.Printf("Body size = %d.\n", len(request.Body))

	fmt.Println("Headers:")
	for key, value := range request.Headers {
		fmt.Printf("    %s: %s\n", key, value)
	}

	content := fmt.Sprintf("Hello, World!\n%v", request.Body)

	return events.APIGatewayProxyResponse{Body: content, StatusCode: 200}, nil
}

func main() {
	lambda.Start(HandleRequest)
}
