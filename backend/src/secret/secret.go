package secret

import (
	"context"
	"encoding/json"
	"log"
	"strings"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-secretsmanager-caching-go/secretcache"
)

var (
	secretCache, _ = secretcache.New()
)

type LegalBrawlSecret struct {
	ApiToken string `json:"apiToken"`
}

func RetrieveSecrets(ctx context.Context, sc *secretcache.Cache, sn string) LegalBrawlSecret {

	result, _ := sc.GetSecretString(sn)

	var secret LegalBrawlSecret
	err := json.Unmarshal([]byte(result), &secret)
	if err != nil {
		log.Fatal(err.Error())
	}

	return secret
}

// In case the token is somehow included in the request, let's scrub it.
func ScrubRequest(request *events.APIGatewayProxyRequest, s LegalBrawlSecret) {
	scrubbed := strings.ReplaceAll(request.Body, s.ApiToken, "********")
	request.Body = scrubbed
}
