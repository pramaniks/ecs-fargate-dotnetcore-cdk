using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecsclustercdk
{
    public class CreateRolesPolicyStackRequest
    {
        public string AccountId { get; set; }
        public string Region { get; set; }
        public string CodePipelineBucketKMSKeyGuid { get; set; }
        public string CodePipelineBucketName { get; set; }
        public string CodePipelineServiceRoleName { get; set; }
        public string CodeBuildServiceRoleName { get; set; }
        public string TaskExecutionRoleName { get; set; }
    }

    public class RolesPolicyStack : Stack
    {
        private CreateRolesPolicyStackRequest _Request;

        internal RolesPolicyStack(Construct scope, string id, CreateRolesPolicyStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;

            createBucketKmsKeyPolicy();
            createCodePipelinePolicy();
            createCodePipelineS3Policy();
            createTaskPolicies();
        }

        private void createCodePipelineS3Policy()
        {
            var s3stmt = new PolicyStatement(new PolicyStatementProps
            {
                Actions = new string[] { "s3:*" },
                Effect = Effect.ALLOW,
            });

            s3stmt.AddResources($"arn:aws:s3:::{_Request.CodePipelineBucketName}");
            s3stmt.AddResources($"arn:aws:s3:::{_Request.CodePipelineBucketName}/*");

            new CfnPolicy(this, "SampleCodePipelineS3BucketPolicy", new CfnPolicyProps
            {
                PolicyName = "SampleCodePipelineS3BucketPolicy",
                PolicyDocument = new PolicyDocument(new PolicyDocumentProps
                {
                    Statements = new PolicyStatement[] { s3stmt }
                }),
                Roles = new string[] { _Request.CodePipelineServiceRoleName}
            });

        }

        private void createCodePipelinePolicy()
        {
            var commonactions = new string[]
          {
                 "codecommit:CancelUploadArchive",
                "codecommit:GetBranch",
                "codecommit:GetCommit",
                "codecommit:GetUploadArchiveStatus",
                "codecommit:UploadArchive",
                  "codedeploy:CreateDeployment",
                "codedeploy:GetApplication",
                "codedeploy:GetApplicationRevision",
                "codedeploy:GetDeployment",
                "codedeploy:GetDeploymentConfig",
                "codedeploy:RegisterApplicationRevision",
                  "codestar-connections:UseConnection",
                     "elasticbeanstalk:*",
                "ec2:*",
                "elasticloadbalancing:*",
                "autoscaling:*",
                "cloudwatch:*",
                "s3:*",
                "sns:*",
                "cloudformation:*",
                "rds:*",
                "sqs:*",
                "ecs:*",
                  "lambda:InvokeFunction",
                "lambda:ListFunctions",
                  "opsworks:CreateDeployment",
                "opsworks:DescribeApps",
                "opsworks:DescribeCommands",
                "opsworks:DescribeDeployments",
                "opsworks:DescribeInstances",
                "opsworks:DescribeStacks",
                "opsworks:UpdateApp",
                "opsworks:UpdateStack",
                  "cloudformation:CreateStack",
                "cloudformation:DeleteStack",
                "cloudformation:DescribeStacks",
                "cloudformation:UpdateStack",
                "cloudformation:CreateChangeSet",
                "cloudformation:DeleteChangeSet",
                "cloudformation:DescribeChangeSet",
                "cloudformation:ExecuteChangeSet",
                "cloudformation:SetStackPolicy",
                "cloudformation:ValidateTemplate",
                  "codebuild:BatchGetBuilds",
                "codebuild:StartBuild",
                "codebuild:BatchGetBuildBatches",
                "codebuild:StartBuildBatch",
                 "devicefarm:ListProjects",
                "devicefarm:ListDevicePools",
                "devicefarm:GetRun",
                "devicefarm:GetUpload",
                "devicefarm:CreateUpload",
                "devicefarm:ScheduleRun",
                  "servicecatalog:ListProvisioningArtifacts",
                "servicecatalog:CreateProvisioningArtifact",
                "servicecatalog:DescribeProvisioningArtifact",
                "servicecatalog:DeleteProvisioningArtifact",
                "servicecatalog:UpdateProduct",
                 "cloudformation:ValidateTemplate",
                   "ecr:DescribeImages",
                     "states:DescribeExecution",
                "states:DescribeStateMachine",
                "states:StartExecution",
                   "appconfig:StartDeployment",
                "appconfig:StopDeployment",
                "appconfig:GetDeployment"
          };
            var commonStatementProps = new PolicyStatementProps
            {
                Actions = commonactions,
                Effect = Effect.ALLOW,
            };
            var commonPolicyStatement = new PolicyStatement(commonStatementProps);
            commonPolicyStatement.AddAllResources();

            var iamactions = new string[] { "iam:PassRole" };
            var conditionDictionary = new Dictionary<string, object>();
            var dict = new Dictionary<string, string[]>();
            dict.Add("iam:PassedToService", new string[] {  "cloudformation.amazonaws.com",
                        "elasticbeanstalk.amazonaws.com",
                        "ec2.amazonaws.com",
                        "ecs-tasks.amazonaws.com"});
            conditionDictionary.Add("StringEqualsIfExists", dict);
            var iamStatementProps = new PolicyStatementProps
            {
                Actions = iamactions,
                Effect = Effect.ALLOW,
                Conditions = conditionDictionary
            };
            var iamPolicyStatement = new PolicyStatement(iamStatementProps);
            iamPolicyStatement.AddAllResources();


            var policyDocumentProps = new PolicyDocumentProps
            {
                Statements = new PolicyStatement[] { commonPolicyStatement, iamPolicyStatement }
            };

            var cfnPolicyProps = new CfnPolicyProps
            {
                PolicyName = "samplecodepipelinepolicy",
                PolicyDocument = new PolicyDocument(policyDocumentProps),
                Roles = new string[] { _Request.CodePipelineServiceRoleName }
            };

            var cfnPolicy = new CfnPolicy(this, "samplecodepipelinepolicy", cfnPolicyProps);
        }

        private void createBucketKmsKeyPolicy()
        {
            var kmsActions = new string[]
            {
                "kms:GenerateDataKey",
                   "kms:Decrypt",
                 "kms:ListKeys",
                 "kms:ListAliases",
                  "kms:DescribeKey",
            };
            var kmsStatementProps = new PolicyStatementProps
            {
                Actions = kmsActions,
                Effect = Effect.ALLOW,
            };

            var kmsPolicyStatement = new PolicyStatement(kmsStatementProps);
            kmsPolicyStatement.AddResources($"arn:aws:kms:{_Request.Region}:{_Request.AccountId}:key/{_Request.CodePipelineBucketKMSKeyGuid}");


            var policyDocumentProps = new PolicyDocumentProps
            {
                Statements = new PolicyStatement[] { kmsPolicyStatement }
            };
            var cfnPolicyProps = new CfnPolicyProps
            {
                PolicyName = "SampleBucketKmsKeyPolicy",
                PolicyDocument = new PolicyDocument(policyDocumentProps),
                Roles = new string[] {_Request.CodePipelineServiceRoleName,
                    _Request.CodeBuildServiceRoleName}
            };

            var cfnPolicy = new CfnPolicy(this, "SampleBucketKmsKeyPolicy", cfnPolicyProps);
        }

        private void createTaskPolicies()
        {
            var cloudwatchactions = new string[]
           {
                "logs:DisassociateKmsKey",
                "logs:CreateLogStream",
                "logs:DescribeLogGroups",
                "logs:DescribeLogStreams",
                "logs:AssociateKmsKey",
                "logs:PutLogEvents"
           };

            var secretActions = new string[]
            {
                 "secretsmanager:GetSecretValue",
                 "kms:Decrypt"
            };

            createPolicies(cloudwatchactions, "ecsSamplecloudwatchlogspolicy", _Request.TaskExecutionRoleName);
            createPolicies(secretActions, "ecsSamplesecretpolicy", _Request.TaskExecutionRoleName);
        }

        private void createPolicies(string[] actions, string policyName, string roleReference)
        {
            var policyStatementProps = new PolicyStatementProps
            {
                Actions = actions,
                Effect = Effect.ALLOW,
            };
            var policyStatement = new PolicyStatement(policyStatementProps);
            policyStatement.AddAllResources();
            var policyDocumentProps = new PolicyDocumentProps
            {
                Statements = new PolicyStatement[] { policyStatement }
            };
            var cfnPolicyProps = new CfnPolicyProps
            {
                PolicyName = policyName,
                PolicyDocument = new PolicyDocument(policyDocumentProps),
                Roles = new string[] { roleReference }
            };

            var cfnPolicy = new CfnPolicy(this, policyName, cfnPolicyProps);
        }

    }
}
