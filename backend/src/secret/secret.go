package secret

import (
	"context"
	"encoding/json"
	"fmt"
	"strings"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-secretsmanager-caching-go/secretcache"
)

type LegalBrawlSecret struct {
	ApiToken string `json:"apiToken"`
}

func RetrieveSecrets(ctx context.Context, sc *secretcache.Cache, sn string) (LegalBrawlSecret, error) {

	result, err := sc.GetSecretString(sn)
	if err != nil {
		return LegalBrawlSecret{}, fmt.Errorf("unable to retrieve secret string: %v", err)
	}

	var secret LegalBrawlSecret
	err = json.Unmarshal([]byte(result), &secret)
	if err != nil {
		return LegalBrawlSecret{}, fmt.Errorf("unable to unmarshal result: %v", err)
	}

	return secret, nil
}

// In case the token is somehow included in the request, let's scrub it.
func ScrubRequest(request *events.APIGatewayProxyRequest, s LegalBrawlSecret) {
	scrubbed := strings.ReplaceAll(request.Body, s.ApiToken, "********")
	request.Body = scrubbed
}
