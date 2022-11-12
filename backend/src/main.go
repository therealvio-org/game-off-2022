package main

import (
	"legalbrawlapi/aws/apigateway"

	"github.com/aws/aws-lambda-go/lambda"
)

func main() {
	lambda.Start(apigateway.HandleRequest)
}
