import * as cdk from "aws-cdk-lib"
import * as path from "path"
import { Construct } from "constructs"
import { aws_dynamodb as dynamodb } from "aws-cdk-lib"
import { aws_apigateway as apigateway } from "aws-cdk-lib"
import { aws_lambda as lambda } from "aws-cdk-lib"
import { aws_iam as iam } from "aws-cdk-lib"
import { aws_secretsmanager as secretsmanager } from "aws-cdk-lib"
import { aws_ssm as ssm } from "aws-cdk-lib"

export class BackendStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    //Best Practices:
    //https://docs.aws.amazon.com/prescriptive-guidance/latest/patterns/estimate-the-cost-of-a-dynamodb-table-for-on-demand-capacity.html?did=pg_card&trk=pg_card#estimate-the-cost-of-a-dynamodb-table-for-on-demand-capacity-best-practices
    new dynamodb.Table(this, "gameDatabase", {
      partitionKey: { name: "playerId", type: dynamodb.AttributeType.STRING },
      sortKey: { name: "formationId", type: dynamodb.AttributeType.STRING },
      billingMode: dynamodb.BillingMode.PROVISIONED,
      pointInTimeRecovery: false,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      tableName: "gameDatabase",
    })

    //The Lambda to handle requests coming through the API Gateway
    //Lambda would be written in golang
    const lambdafn = new lambda.Function(this, "legalBrawlApiHandler", {
      architecture: lambda.Architecture.ARM_64,
      runtime: lambda.Runtime.PROVIDED_AL2,
      handler: "bootstrap",
      code: lambda.Code.fromAsset(path.join(__dirname, "../../../src/dist")),
      environment: {
        LEGAL_BRAWL_SECRET_NAME: "legalBrawl/prod/api/v1/",
      },
    })

    //TODO: Swap this out so the ARN suffix isn't stated in source code
    const apiToken = secretsmanager.Secret.fromSecretNameV2(
      this,
      "apiTokenSecret",
      "legalBrawl/prod/api/v1/"
    )
    const SecretArnSuffix = ssm.StringParameter.valueForStringParameter(
      this,
      "/legalBrawl/secret/arn/suffix"
    )
    lambdafn.addToRolePolicy(
      new iam.PolicyStatement({
        actions: [
          "secretsmanager:GetSecretValue",
          "secretsmanager:DescribeSecret",
        ],
        resources: [`${apiToken.secretArn}-${SecretArnSuffix}`],
      })
    )

    const api = new apigateway.LambdaRestApi(this, "legalBrawlApi", {
      handler: lambdafn,
      //Setting proxy to `true` will forward *all* requests to Lambda
      //TODO: Figure out what resources, and what methods we are looking to implement
      proxy: false,
    })

    new ssm.StringParameter(this, "paramLegalBrawlApiUrl", {
      parameterName: "/legalBrawl/api/url",
      stringValue: api.url,
    })

    //b.s. example of API paths
    const v1 = api.root.addResource("v1")
    const formation = v1.addResource("formationId")
    const formationPostMethod = formation.addMethod("POST", undefined, {
      apiKeyRequired: false,
    }) //Set apiKeyRequired to `true` when ready to start locking down the API

    const plan = api.addUsagePlan("legalBrawlUsagePlan", {
      throttle: {
        rateLimit: 10,
        burstLimit: 2,
      },
    })

    plan.addApiStage({
      stage: api.deploymentStage,
      throttle: [
        {
          method: formationPostMethod,
          throttle: {
            rateLimit: 10,
            burstLimit: 2,
          },
        },
      ],
    })

    const key = api.addApiKey("ApiKey", {
      apiKeyName: "ApiKey",
    })
    plan.addApiKey(key)
  }
}
