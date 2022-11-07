#!/usr/bin/env node
import "source-map-support/register"
import * as cdk from "aws-cdk-lib"
import { BackendStack } from "./stacks/backend-stack"

const { AWS_ACCOUNT_ID: accountId, AWS_REGION: region } = process.env

const app = new cdk.App()
new BackendStack(app, "legalBrawlBackendStack", {
  env: {
    account: accountId,
    region: region,
  },
})
