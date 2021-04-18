using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using Amazon.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Amazon.CDK.AWS.ECS.CfnService;

namespace Ecsclustercdk
{
    public class CreateECSServiceStackRequest
    {
        public string ECSClusterName { get; set; }
        public string Region { get;  set; }
        public string AccountId { get;  set; }
        public string EcsServiceSecurityGroupId { get; set; }
        public ICollection<string> BackendSubnetIdList { get; set; }
    }

    public class CreateServiceRequest
    {
        public string ServiceName { get; set; }
        public string ContainerName { get; set; }
        public string TargetGroupId { get; set; }
        public string TaskDefinition { get; set; }
    }
    public class ECSServiceStack : Stack
    {
        private CreateECSServiceStackRequest _Request;
        private AmazonECSClient _ECSClient;

        internal ECSServiceStack(Construct scope, string id, CreateECSServiceStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;
            _ECSClient = new AmazonECSClient();
            createPaymentService();
        }

        private void createPaymentService()
        {           
            var taskDefinitionRevisionListResponse = _ECSClient.ListTaskDefinitionsAsync(new Amazon.ECS.Model.ListTaskDefinitionsRequest
            {
                FamilyPrefix = "PaymentServiceTaskDefinition",
                Sort = SortOrder.DESC,
                MaxResults = 1
            }).Result;

           var taskDefintionArn = taskDefinitionRevisionListResponse.TaskDefinitionArns?.FirstOrDefault();

             var serviceRequest = new CreateServiceRequest
            {
                ServiceName = "PaymentService",
                ContainerName = "PaymentServiceContainer",
                TaskDefinition = taskDefintionArn,
                TargetGroupId = "tgtest/6cf6f3fda0953a7d"
            };

            createECSService(serviceRequest);
        }

        private void createECSService(CreateServiceRequest Request)
        {
            var CfnServiceProps = new CfnServiceProps
            {
                Cluster = _Request.ECSClusterName,
                LaunchType = "FARGATE",
                ServiceName = Request.ServiceName,
                TaskDefinition = Request.TaskDefinition,
                DesiredCount = 1,
                LoadBalancers = new object[1]
                 {
                    new LoadBalancerProperty
                    {
                        ContainerName = Request.ContainerName,
                        ContainerPort = 5000,
                        TargetGroupArn = $"arn:aws:elasticloadbalancing:{_Request.Region}:{_Request.AccountId}:targetgroup/{Request.TargetGroupId}"
                    }
                 },
                NetworkConfiguration = new NetworkConfigurationProperty
                {
                    AwsvpcConfiguration = new AwsVpcConfigurationProperty
                    {
                        AssignPublicIp = "ENABLED",
                        SecurityGroups = new string[] { _Request.EcsServiceSecurityGroupId },
                        Subnets = _Request.BackendSubnetIdList.ToArray()
                    }
                }
            };

            new CfnService(this, Request.ServiceName, CfnServiceProps);
        }
    }
}
