using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using System;
using System.Collections.Generic;
using System.Text;
using static Amazon.CDK.AWS.ECS.CfnTaskDefinition;

namespace Ecsclustercdk
{
    public class CreateTaskDefinitionParametersRequest
    {
        public string TaskDefinitionFamilyName { get; set; }
        public string TaskCpu { get; set; }
        public string TaskMemory { get; internal set; }
        public string ContainerName { get; internal set; }
        public string ImageUri { get; internal set; }
    }
    public class CreateTaskDefinitionStackRequest
    {
        public string AccountId { get; set; }
        public string Region { get; set; }
        public string TaskExecutionRoleName { get; set; }
    }
    public class TaskDefinitionStack : Stack
    {
        private CreateTaskDefinitionStackRequest _Request;

        internal TaskDefinitionStack(Construct scope, string id, CreateTaskDefinitionStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;

            createPaymentTaskDefinition();
            //createProductTaskDefinition();
            //createBasketTaskDefinition();
            //createRecommendationTaskDefinition();
        }

        private void createPaymentTaskDefinition()
        {
            var request = new CreateTaskDefinitionParametersRequest
            {
                TaskDefinitionFamilyName = "PaymentServiceTaskDefinition",
                TaskCpu = "2048",
                TaskMemory = "4096",
                ContainerName = "PaymentServiceContainer",
                ImageUri = $"{_Request.AccountId}.dkr.ecr.{_Request.Region}.amazonaws.com/paymentmanagerservice:latest"
            };

            createTaskDefinition(request);
        }

        private void createProductTaskDefinition()
        {
            var request = new CreateTaskDefinitionParametersRequest
            {
                TaskDefinitionFamilyName = "ProductServiceTaskDefinition",
                TaskCpu = "2048",
                TaskMemory = "4096",
                ContainerName = "ProductServiceContainer",
                ImageUri = "714911308443.dkr.ecr.us-east-2.amazonaws.com/paymentmanagerservice:latest"
            };

            createTaskDefinition(request);
        }

        private void createBasketTaskDefinition()
        {
            var request = new CreateTaskDefinitionParametersRequest
            {
                TaskDefinitionFamilyName = "BasketServiceTaskDefinition",
                TaskCpu = "2048",
                TaskMemory = "4096",
                ContainerName = "BasketServiceContainer",
                ImageUri = "714911308443.dkr.ecr.us-east-2.amazonaws.com/paymentmanagerservice:latest"
            };

            createTaskDefinition(request);
        }

        private void createRecommendationTaskDefinition()
        {
            var request = new CreateTaskDefinitionParametersRequest
            {
                TaskDefinitionFamilyName = "RecommendationServiceTaskDefinition",
                TaskCpu = "2048",
                TaskMemory = "4096",
                ContainerName = "RecommendationServiceContainer",
                ImageUri = "714911308443.dkr.ecr.us-east-2.amazonaws.com/paymentmanagerservice:latest"
            };

            createTaskDefinition(request);
        }

        private CfnTaskDefinition createTaskDefinition(CreateTaskDefinitionParametersRequest Request)
        {
            var def = new ContainerDefinitionProperty
            {
                Name = Request.ContainerName,
                Image = Request.ImageUri,
                PortMappings = new PortMappingProperty[]
                {
                    new PortMappingProperty
                    {
                        ContainerPort = 5000,
                        Protocol = "tcp"
                    }
                }
            };
            var CfnTaskDefinitionProps = new CfnTaskDefinitionProps
            {
                Cpu = Request.TaskCpu,
                Memory = Request.TaskMemory,
                Family = Request.TaskDefinitionFamilyName,
                NetworkMode = "awsvpc",
                RequiresCompatibilities = new string[] { "FARGATE" },
                ExecutionRoleArn = $"arn:aws:iam::{_Request.AccountId}:role/{_Request.TaskExecutionRoleName}",
                ContainerDefinitions = new object[1]
                {
                  def
                }
            };

            var CfnTaskDefinition = new CfnTaskDefinition(this, Request.TaskDefinitionFamilyName, CfnTaskDefinitionProps);
            return CfnTaskDefinition;
        }
    }
}
