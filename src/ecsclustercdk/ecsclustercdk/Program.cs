using Amazon.CDK;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecsclustercdk
{
    sealed class Program
    {
        private static App _Application;
      
        public static void Main(string[] args)
        {
            _Application = new App();

            createSecurityGroupStack("SecurityGroupStack");
            createKmsKeyStack("KMSKeyStack");
            createBucketStack("BucketStack");
            createRolesStack("RolesStack");
            createTaskDefinitionStack("TaskDefinitionStack");
            createECSClusterStack("ECSClusterStack");
            createECSServiceStack("ECSServiceStack");          
            createEcsCodePipelineStack("ECSCodepipelineStack");

            _Application.Synth();
        }
        
       
        private static void createSecurityGroupStack(string stackName)
        {
            new SecurityGroupStack(_Application, stackName, new CreateSecurityGroupStackRequest { VpcId = CloudConfiguration.Instance.VpcId });
        }
        private static void createKmsKeyStack(string stackName)
        {
            new KmsKeyStack(_Application, stackName, new CreateKmsKeyStackRequest { AccountId = CloudConfiguration.Instance.AccountId });
        }

        private static void createBucketStack(string stackName)
        {
            new BucketStack(_Application, stackName, new CreateBucketStackRequest
            {
                BucketKeyId = CloudConfiguration.Instance.BucketKmsKeyGuid,
                CodePipelineBucketName = CloudConfiguration.Instance.CodePipelineBucketName
            });
        }

        private static void createRolesStack(string stackName)
        {
            new RolesStack(_Application, stackName, new CreateRolesStackRequest
            {
                CodeBuildServiceRoleName = CloudConfiguration.Instance.CodeBuildServiceRoleName,
                CodePipelineServiceRoleName = CloudConfiguration.Instance.CodePipelineServiceRoleName,
                CodePipelineBucketName = CloudConfiguration.Instance.CodePipelineBucketName,
                AccountId = CloudConfiguration.Instance.AccountId,
                CodePipelineBucketKMSKeyGuid = CloudConfiguration.Instance.BucketKmsKeyGuid,
                TaskExecutionRoleName = CloudConfiguration.Instance.TaskExecutionRoleName
            });
        }

        private static void createTaskDefinitionStack(string stackName)
        {
            new TaskDefinitionStack(_Application, stackName, new CreateTaskDefinitionStackRequest
            {
                AccountId = CloudConfiguration.Instance.AccountId,               
                TaskExecutionRoleName = CloudConfiguration.Instance.TaskExecutionRoleName,
                Region = CloudConfiguration.Instance.Region
            });
        }

        private static void createECSClusterStack(string stackName)
        {
            new ECSClusterStack(_Application, stackName, new CreateECSClusterStackRequest
            {
                ClusterName = CloudConfiguration.Instance.ECSClusterName
            });
        }

        private static void createECSServiceStack(string stackName)
        {
            new ECSServiceStack(_Application, stackName, new CreateECSServiceStackRequest
            {
                ECSClusterName = CloudConfiguration.Instance.ECSClusterName,
                AccountId = CloudConfiguration.Instance.AccountId,
                Region = CloudConfiguration.Instance.Region,
                EcsServiceSecurityGroupId = CloudConfiguration.Instance.ECSServiceSecurityGroupId,
                BackendSubnetIdList = CloudConfiguration.Instance.BackendSubnetIdList
            });
        }

        private static void createEcsCodePipelineStack(string stackName)
        {
            var buildProjectServiceRoleArn = $"arn:aws:iam::{CloudConfiguration.Instance.AccountId}:role/service-role/{CloudConfiguration.Instance.CodeBuildServiceRoleName}";
            var codePipelineServiceRoleArn = $"arn:aws:iam::{CloudConfiguration.Instance.AccountId}:role/service-role/{CloudConfiguration.Instance.CodePipelineServiceRoleName}";

            new EcsCodepipelineStack(_Application, stackName, new CreateEcsCodePipelineStackRequest
            {
                AccountId = CloudConfiguration.Instance.AccountId,
                VpcId = CloudConfiguration.Instance.VpcId,
                SubnetIdList = CloudConfiguration.Instance.BackendSubnetIdList,
                BuildProjectSecurityGroupId = CloudConfiguration.Instance.CodeBuildSecurityGroupId,
                RepositoryConnection = CloudConfiguration.Instance.RepositoryConnection,
                CodePipelineBucketName = CloudConfiguration.Instance.CodePipelineBucketName,
                BuildProjectServiceRoleArn = buildProjectServiceRoleArn,
                CodePipelineServiceRoleArn = codePipelineServiceRoleArn
            });
        }
    }
}
