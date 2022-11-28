package main

import (
	lbapiaws "legalbrawlapi/aws"

	"github.com/aws/aws-lambda-go/lambda"
)

func main() {
	lambda.Start(lbapiaws.HandleRequest)
}
