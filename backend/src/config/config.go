package config

import (
	"github.com/kelseyhightower/envconfig"
)

type EnvConfig struct {
	LegalBrawlSecretName string `split_words:"true" required:"true"`
	PlayerHandTableName  string `split_words:"true" required:"true" default:"playerHands"`
}

func New() EnvConfig {
	var envConfig EnvConfig
	envconfig.MustProcess("", &envConfig)

	return envConfig
}
