using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.KMS;
using Amazon.CDK.AWS.S3;
using System;
using System.Collections.Generic;
using System.Text;
using static Amazon.CDK.AWS.CodeBuild.CfnProject;
using static Amazon.CDK.AWS.CodePipeline.CfnPipeline;
using static Amazon.CDK.AWS.S3.CfnAccessPoint;
using static Amazon.CDK.AWS.S3.CfnBucket;

namespace Ecsclustercdk
{
    public class CreateEcsCodePipelineStackRequest
    {
        public string VpcId { get; set; }
        public string[] SubnetIdList { get; set; }      
        public string BuildProjectSecurityGroupId { get; set; }
        public RepositoryConnection RepositoryConnection { get; set; }
        public string AccountId { get; set; }
        public string CodePipelineBucketName { get; set; }     
        public string BuildProjectServiceRoleArn { get; set; }
        public string CodePipelineServiceRoleArn { get; set; }
    }

    public class PipelineConfiguration
    {
        public string BuildProjectName { get; set; }
        public string BuildProjectDescription { get; set; }
        public string BuildSpecLocation { get; set; }
        public string CodePipelineProjectName { get; set; }
        public string ClusterName { get; set; }
        public string ServiceName { get; set; }
        public EnvironmentVariableProperty[] BuildEnvironmentVariablePropertyList { get; set; }
    }
    public class EcsCodepipelineStack : Stack
    {
        private CreateEcsCodePipelineStackRequest _Request;     

        internal EcsCodepipelineStack(Construct scope, string id, CreateEcsCodePipelineStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;           
          
            createProductServicePipeline();
        }

        private void createProductServicePipeline()
        {
            var pipelineConfiguration = new PipelineConfiguration
            {
                BuildProjectName = "ProductBuildProject",
                BuildProjectDescription = "Build project for Product Service",
                BuildSpecLocation = "BuildSpec/PaymentManagerService.yml",
                CodePipelineProjectName = "ProductServicePipeline",
                ClusterName = "ecscluster",
                ServiceName = "PaymentService"
            };

            createCodeBuildProject(pipelineConfiguration);
            createCodePipeline(pipelineConfiguration);
        }

        private void createCodeBuildProject(PipelineConfiguration pipelineConfiguration)
        {            
            var cfnBuildProject = new CfnProject(this, pipelineConfiguration.BuildProjectName, new CfnProjectProps
            {
                Name = pipelineConfiguration.BuildProjectName,
                Description = pipelineConfiguration.BuildProjectDescription,
                ServiceRole = _Request.BuildProjectServiceRoleArn,
                Artifacts = new ArtifactsProperty
                {
                    Type = "CODEPIPELINE"
                },
               
                Source = new SourceProperty
                {
                    Type = "CODEPIPELINE",
                    BuildSpec = pipelineConfiguration.BuildSpecLocation
                },
                Environment = new EnvironmentProperty
                {
                    Type = "LINUX_CONTAINER",
                    Image = "aws/codebuild/amazonlinux2-x86_64-standard:3.0",
                    ComputeType = "BUILD_GENERAL1_SMALL",                    
                    PrivilegedMode = true,
                    EnvironmentVariables = pipelineConfiguration.BuildEnvironmentVariablePropertyList
                }
            });
        }
        private void createCodePipeline(PipelineConfiguration pipelineConfiguration)
        {
            var sourceHash = new Dictionary<string, string>();
            sourceHash.Add("ConnectionArn", _Request.RepositoryConnection.ConnectionArn);
            sourceHash.Add("FullRepositoryId", _Request.RepositoryConnection.RepositoryId);
            sourceHash.Add("BranchName", _Request.RepositoryConnection.BranchName);


            var codebuildHash = new Dictionary<string, string>();
            codebuildHash.Add("ProjectName", pipelineConfiguration.BuildProjectName);

            var codeDeployHash = new Dictionary<string, string>();
            codeDeployHash.Add("ClusterName", pipelineConfiguration.ClusterName);
            codeDeployHash.Add("ServiceName", pipelineConfiguration.ServiceName);

            var cfnPipeline = new CfnPipeline(this, pipelineConfiguration.CodePipelineProjectName, new CfnPipelineProps
            {
                RoleArn = _Request.CodePipelineServiceRoleArn,
                Name = pipelineConfiguration.CodePipelineProjectName,

                Stages = new StageDeclarationProperty[]
                {
                    new StageDeclarationProperty {
                        Name = "Source" ,

                        Actions = new ActionDeclarationProperty[]
                        {
                            new ActionDeclarationProperty
                            {
                                Name = "Source",
                                ActionTypeId = new ActionTypeIdProperty{ Version = "1", Category = "Source" ,Owner = "AWS",Provider = "CodeStarSourceConnection"},
                                Configuration = sourceHash,
                                OutputArtifacts = new OutputArtifactProperty[]
                                {
                                    new OutputArtifactProperty{ Name = "SourceArtifact"}
                                }
                            }
                        }
                   },
                    new StageDeclarationProperty {
                        Name = "Build" ,
                        Actions = new ActionDeclarationProperty[]
                        {
                            new ActionDeclarationProperty
                            {
                                Name = "Build",
                                ActionTypeId = new ActionTypeIdProperty{Version = "1", Category = "Build" ,Owner = "AWS",Provider = "CodeBuild"},
                                Configuration = codebuildHash,
                                InputArtifacts = new InputArtifactProperty[]
                                {
                                    new InputArtifactProperty{ Name = "SourceArtifact"}
                                },
                                OutputArtifacts = new OutputArtifactProperty[]
                                {
                                    new OutputArtifactProperty{ Name = "BuildArtifact"}
                                }
                            }
                        }
                   },                    
                    new StageDeclarationProperty {
                        Name = "CodeDeploy" ,
                        Actions = new ActionDeclarationProperty[]
                        {
                            new ActionDeclarationProperty
                            {
                                Name = "CodeDeploy",
                                ActionTypeId = new ActionTypeIdProperty{Version = "1", Category = "Deploy" ,Owner = "AWS",Provider = "ECS"},
                                Configuration = codeDeployHash,
                                InputArtifacts = new InputArtifactProperty[]
                                {
                                    new InputArtifactProperty{ Name = "BuildArtifact"}
                                }
                            }
                        }
                   }
                },
                ArtifactStore = new ArtifactStoreProperty
                {
                    Location = _Request.CodePipelineBucketName,
                    Type = "S3"
                },

            });
        }
    }
}
