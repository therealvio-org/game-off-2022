import * as cdk from "aws-cdk-lib"
import * as path from "path"
import { Construct } from "constructs"
import { Duration, aws_apigateway as apigateway } from "aws-cdk-lib"
import { aws_dynamodb as dynamodb } from "aws-cdk-lib"
import { aws_iam as iam } from "aws-cdk-lib"
import { aws_lambda as lambda } from "aws-cdk-lib"
import { aws_secretsmanager as secretsmanager } from "aws-cdk-lib"
import { aws_ssm as ssm } from "aws-cdk-lib"

export class BackendStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    const playerHandsTable = new dynamodb.Table(this, "playerHandsTable", {
      partitionKey: { name: "version", type: dynamodb.AttributeType.STRING },
      sortKey: { name: "playerId", type: dynamodb.AttributeType.STRING },
      billingMode: dynamodb.BillingMode.PROVISIONED,
      pointInTimeRecovery: false,
      removalPolicy: cdk.RemovalPolicy.DESTROY, //Swap this to `retain` when we got something that is shaping up nicely
      tableName: "playerHands",
    })

    //The Lambda to handle requests coming through the API Gateway
    //Lambda would be written in golang
    const lambdaFn = new lambda.Function(this, "legalBrawlApiHandler", {
      architecture: lambda.Architecture.ARM_64,
      runtime: lambda.Runtime.PROVIDED_AL2,
      handler: "bootstrap",
      code: lambda.Code.fromAsset(path.join(__dirname, "../../../src/dist")),
      timeout: Duration.seconds(20),
      environment: {
        LEGAL_BRAWL_SECRET_NAME: "legalBrawl/prod/api/v1/",
        PLAYER_HAND_TABLE_NAME: playerHandsTable.tableName,
        //The card balancing version. This should later be controlled via some flag, release tag
        //or higher-level environment var during release
        PLAYER_HAND_VERSION: "1.0",
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
    lambdaFn.addToRolePolicy(
      new iam.PolicyStatement({
        actions: [
          "secretsmanager:GetSecretValue",
          "secretsmanager:DescribeSecret",
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:Query",
          "dynamodb:UpdateItem",
        ],
        resources: [
          `${apiToken.secretArn}-${SecretArnSuffix}`,
          playerHandsTable.tableArn,
        ],
      })
    )

    const api = new apigateway.LambdaRestApi(this, "legalBrawlApi", {
      handler: lambdaFn,
      //Setting proxy to `true` will forward *all* requests to Lambda
      //TODO: Figure out what resources, and what methods we are looking to implement
      proxy: false,
    })

    const apiIntegration = new apigateway.LambdaIntegration(lambdaFn)

    //playerHand scheme
    const playerHandModel = api.addModel("playerHand", {
      contentType: `application/json`,
      modelName: "playerHandv1",
      schema: {
        schema: apigateway.JsonSchemaVersion.DRAFT4,
        title: "PlayerHand",
        type: apigateway.JsonSchemaType.OBJECT,
        required: ["handInfo"],
        properties: {
          handInfo: {
            type: apigateway.JsonSchemaType.OBJECT,
            properties: {
              playerName: { type: apigateway.JsonSchemaType.STRING },
              playerId: { type: apigateway.JsonSchemaType.STRING },
              version: { type: apigateway.JsonSchemaType.STRING },
              cards: {
                type: apigateway.JsonSchemaType.ARRAY,
                items: { type: apigateway.JsonSchemaType.INTEGER },
              },
            },
          },
        },
      },
    })

    new ssm.StringParameter(this, "paramLegalBrawlApiUrl", {
      parameterName: "/legalBrawl/api/url",
      stringValue: api.url,
    })

    const v1 = api.root.addResource("v1")
    const playerHand = v1.addResource("playerHand")

    //Need to create individual validator objects, refer to: https://github.com/aws/aws-cdk/issues/7613
    const playerHandPostValidator = new apigateway.RequestValidator(
      this,
      "playerHandPostValidator",
      {
        restApi: api,
        requestValidatorName: "playerHandPostValidator",
        validateRequestBody: true,
        validateRequestParameters: false,
      }
    )
    const playerHandGetValidator = new apigateway.RequestValidator(
      this,
      "playerHandGetValidator",
      {
        restApi: api,
        requestValidatorName: "playerHandGetValidator",
        validateRequestParameters: true,
      }
    )
    const playerHandPutValidator = new apigateway.RequestValidator(
      this,
      "playerHandPutValidator",
      {
        restApi: api,
        requestValidatorName: "playerHandPutValidator",
        validateRequestBody: true,
        validateRequestParameters: false,
      }
    )

    const playerHandPost = playerHand.addMethod("POST", apiIntegration, {
      apiKeyRequired: true,
      requestModels: {
        "application/json": playerHandModel,
      },
      requestValidator: playerHandPostValidator,
    })
    const playerHandGet = playerHand.addMethod("GET", apiIntegration, {
      apiKeyRequired: true,
      requestParameters: {
        "method.request.querystring.playerId": true,
      },
      requestValidator: playerHandGetValidator,
    })
    const playerHandPut = playerHand.addMethod("PUT", apiIntegration, {
      apiKeyRequired: true,
      requestModels: {
        "application/json": playerHandModel,
      },
      requestValidator: playerHandPutValidator,
    })
    playerHand.addMethod("OPTIONS", apiIntegration, {
      apiKeyRequired: false,
    })

    const plan = api.addUsagePlan("legalBrawlUsagePlan", {
      throttle: {
        rateLimit: 50,
        burstLimit: 10,
      },
    })

    plan.addApiStage({
      stage: api.deploymentStage,
      throttle: [
        {
          method: playerHandPost,
          throttle: {
            rateLimit: 50,
            burstLimit: 10,
          },
        },
        {
          method: playerHandGet,
          throttle: {
            rateLimit: 50,
            burstLimit: 10,
          },
        },
        {
          method: playerHandPut,
          throttle: {
            rateLimit: 50,
            burstLimit: 10,
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
