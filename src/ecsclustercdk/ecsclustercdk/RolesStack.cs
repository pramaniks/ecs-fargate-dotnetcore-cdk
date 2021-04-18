using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecsclustercdk
{
    public class CreateRolesStackRequest
    {
        public string AccountId { get; set; }
        public string CodePipelineBucketKMSKeyGuid { get; set; }
        public string CodeBuildServiceRoleName { get; set; }
        public string CodePipelineBucketName { get; set; }
        public string CodePipelineServiceRoleName { get; set; }
        public string TaskExecutionRoleName { get; internal set; }
    }
    public class RolesStack : Stack
    {
        private CreateRolesStackRequest _Request;
        private CfnRole _CfnCodeBuildRole;
        private CfnRole _CfnCodePipelineRole;
        private CfnRole _CfnTaskRole;

        internal RolesStack(Construct scope, string id, CreateRolesStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;

            createCodeBuildServiceRole();
            createCodePipelineServiceRole();
            createBucketKmsKeyPolicy();
            createCodePipelinePolicy();
            createCodePipelineS3Policy();
            createTaskRole();
            createTaskPolicies();
        }

        private void createCodeBuildServiceRole()
        {
            var assumeRolePolicyDocument = new PolicyDocument();
            var assumeRolepolicyStatementProps = new PolicyStatementProps
            {
                Actions = new string[] { "sts:AssumeRole" },
                Effect = Effect.ALLOW,
                Principals = new ServicePrincipal[] { new ServicePrincipal("codebuild.amazonaws.com") }
            };

            var assumeRolePolicyStatement = new PolicyStatement(assumeRolepolicyStatementProps);
            assumeRolePolicyDocument.AddStatements(assumeRolePolicyStatement);

            var cfnRoleProps = new CfnRoleProps
            {
                RoleName = _Request.CodeBuildServiceRoleName,
                AssumeRolePolicyDocument = assumeRolePolicyDocument,
                Path = "/service-role/",
                ManagedPolicyArns = new string[] { "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser" },
            };

            _CfnCodeBuildRole = new CfnRole(this, _Request.CodeBuildServiceRoleName, cfnRoleProps);

            var logstmt = new PolicyStatement(new PolicyStatementProps
            {
                Actions = new string[] {
                 "logs:CreateLogGroup",
                "logs:CreateLogStream",
                "logs:PutLogEvents" },
                Effect = Effect.ALLOW,
            });
            logstmt.AddAllResources();


            var ec2stmt = new PolicyStatement(new PolicyStatementProps
            {
                Actions = new string[] { "ec2:*",
                 "logs:CreateLogGroup",
                "logs:CreateLogStream",
                "logs:PutLogEvents",
                 "lambda:*",
                "cloudformation:*",
                     "iam:PassRole",
                "lambda:*",
                "cloudformation:*" },
                Effect = Effect.ALLOW,
            });

            ec2stmt.AddAllResources();
            new CfnPolicy(this, "SampleCodeBuildEC2Policy", new CfnPolicyProps
            {
                PolicyName = "SampleCodeBuildEC2Policy",
                PolicyDocument = new PolicyDocument(new PolicyDocumentProps
                {
                    Statements = new PolicyStatement[] { ec2stmt }
                }),
                Roles = new string[] { _CfnCodeBuildRole.Ref }
            });



            var s3stmt = new PolicyStatement(new PolicyStatementProps
            {
                Actions = new string[] {
                 "s3:PutObject",
                "s3:GetObject",
                "s3:GetBucketAcl",
                "s3:GetBucketLocation",
                "s3:GetObjectVersion" },
                Effect = Effect.ALLOW,
            });

            s3stmt.AddResources($"arn:aws:s3:::{_Request.CodePipelineBucketName}");
            s3stmt.AddResources($"arn:aws:s3:::{_Request.CodePipelineBucketName}/*");


            new CfnPolicy(this, "SampleCodeBuildLogPolicy", new CfnPolicyProps
            {
                PolicyName = "SampleCodeBuildLogPolicy",
                PolicyDocument = new PolicyDocument(new PolicyDocumentProps
                {
                    Statements = new PolicyStatement[] { logstmt }
                }),
                Roles = new string[] { _CfnCodeBuildRole.Ref }
            });

            new CfnPolicy(this, "SampleCodeBuildS3Policy", new CfnPolicyProps
            {
                PolicyName = "SampleCodeBuildS3Policy",
                PolicyDocument = new PolicyDocument(new PolicyDocumentProps
                {
                    Statements = new PolicyStatement[] { s3stmt }
                }),
                Roles = new string[] { _CfnCodeBuildRole.Ref }
            });
        }

        private void createCodePipelineServiceRole()
        {
            var assumeRolePolicyDocument = new PolicyDocument();
            var assumeRolepolicyStatementProps = new PolicyStatementProps
            {
                Actions = new string[] { "sts:AssumeRole" },
                Effect = Effect.ALLOW,
                Principals = new ServicePrincipal[] { new ServicePrincipal("codepipeline.amazonaws.com") }
            };

            var assumeRolePolicyStatement = new PolicyStatement(assumeRolepolicyStatementProps);
            assumeRolePolicyDocument.AddStatements(assumeRolePolicyStatement);


            var cfnRoleProps = new CfnRoleProps
            {
                RoleName = _Request.CodePipelineServiceRoleName,
                AssumeRolePolicyDocument = assumeRolePolicyDocument,
                Path = "/service-role/"
            };

            _CfnCodePipelineRole = new CfnRole(this, _Request.CodePipelineServiceRoleName, cfnRoleProps);

            var codestartstmt = new PolicyStatement(new PolicyStatementProps
            {
                Actions = new string[] { "codestar-connections:UseConnection", "codebuild:BatchGetBuilds",
                "codebuild:StartBuild",
                "codebuild:BatchGetBuildBatches",
                "codebuild:StartBuildBatch"
                },
                Effect = Effect.ALLOW,
            });

            codestartstmt.AddAllResources();
            var policyDocumentProps = new PolicyDocumentProps
            {
                Statements = new PolicyStatement[] { codestartstmt }
            };
            var cfnPolicyProps = new CfnPolicyProps
            {
                PolicyName = "SampleCodeConnectionPolicy",
                PolicyDocument = new PolicyDocument(policyDocumentProps),
                Roles = new string[] { _CfnCodePipelineRole.Ref }
            };

            var cfnPolicy = new CfnPolicy(this, "SampleCodeConnectionPolicy", cfnPolicyProps);
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
                Roles = new string[] { _CfnCodePipelineRole.Ref }
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
                Roles = new string[] { _CfnCodePipelineRole.Ref }
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
            kmsPolicyStatement.AddResources($"arn:aws:kms:us-east-2:{_Request.AccountId}:key/{_Request.CodePipelineBucketKMSKeyGuid}");           


            var policyDocumentProps = new PolicyDocumentProps
            {
                Statements = new PolicyStatement[] { kmsPolicyStatement }
            };
            var cfnPolicyProps = new CfnPolicyProps
            {
                PolicyName = "SampleBucketKmsKeyPolicy",
                PolicyDocument = new PolicyDocument(policyDocumentProps),
                Roles = new string[] {_CfnCodeBuildRole.Ref,
                    _CfnCodePipelineRole.Ref}
            };

            var cfnPolicy = new CfnPolicy(this, "SampleBucketKmsKeyPolicy", cfnPolicyProps);
        }

        private void createTaskRole()
        {
            var assumeRolePolicyDocument = new PolicyDocument();
            var assumeRolepolicyStatementProps = new PolicyStatementProps
            {
                Actions = new string[] { "sts:AssumeRole" },
                Effect = Effect.ALLOW,
                Principals = new ServicePrincipal[] { new ServicePrincipal("ecs-tasks.amazonaws.com") }
            };

            var assumeRolePolicyStatement = new PolicyStatement(assumeRolepolicyStatementProps);
            assumeRolePolicyDocument.AddStatements(assumeRolePolicyStatement);


            var cfnRoleProps = new CfnRoleProps
            {
                RoleName = _Request.TaskExecutionRoleName,
                AssumeRolePolicyDocument = assumeRolePolicyDocument,
                ManagedPolicyArns = new string[] { "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser" ,
                    "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"},
            };

            _CfnTaskRole = new CfnRole(this, _Request.TaskExecutionRoleName, cfnRoleProps);
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

            createPolicies(cloudwatchactions, "ecsSamplecloudwatchlogspolicy", _CfnTaskRole.Ref);
            createPolicies(secretActions, "ecsSamplesecretpolicy", _CfnTaskRole.Ref);
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
