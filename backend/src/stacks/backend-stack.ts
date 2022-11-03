import * as cdk from "aws-cdk-lib"
import * as path from "path"
import { Construct } from "constructs"
import { aws_dynamodb as dynamodb } from "aws-cdk-lib"
import { aws_apigateway as apigateway } from "aws-cdk-lib"
import { aws_lambda as lambda } from "aws-cdk-lib"

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
    const lambdafn = new lambda.Function(this, "lambda", {
      architecture: lambda.Architecture.ARM_64,
      runtime: lambda.Runtime.PROVIDED_AL2,
      handler: "bootstrap",
      code: lambda.Code.fromAsset(
        path.join(__dirname, "../../lambda-code/dist")
      ),
    })

    const api = new apigateway.LambdaRestApi(this, "api", {
      handler: lambdafn,
      //Setting proxy to `true` will forward *all* requests to Lambda
      //TODO: Figure out what resources, and what methods we are looking to implement
      proxy: false,
    })

    //b.s. example of API paths
    const formation = api.root.addResource("formationId")
    formation.addMethod("POST")
  }
}
