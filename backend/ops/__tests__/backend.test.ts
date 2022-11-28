import * as cdk from "aws-cdk-lib"
import { BackendStack } from "../src/stacks/backend-stack"
import { Template } from "aws-cdk-lib/assertions"

describe("legalBrawlBackendStack", () => {
  const app = new cdk.App()

  const stack = new BackendStack(app, "legalBrawlBackendStack", {
    env: {
      account: "12345678901234",
      region: "foo-bar-1",
    },
    tags: {
      asset: "game-off-2022",
      environment: "test",
    },
  })

  test("it synths and snapshots", () => {
    const template = Template.fromStack(stack)
    expect(template).toMatchSnapshot()
  })
})
