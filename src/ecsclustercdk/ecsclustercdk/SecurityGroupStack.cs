using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecsclustercdk
{
    public class CreateSecurityGroupStackRequest
    {
        public string VpcId { get; set; }
    }

    public class SecurityGroupStack : Stack
    {
        private CreateSecurityGroupStackRequest _Request;
        private CfnSecurityGroup _CfnPublicAlbsecurityGroup;
        private CfnSecurityGroup _CfnEcsServiceSecurityGroup;

        internal SecurityGroupStack(Construct scope, string id, CreateSecurityGroupStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;

            createCodeBuildSecurityGroup();
            //createAlbPublicSecurityGroup();
            //createECSServiceSecurityGroup();
            //createDBSecurityGroup();
        }

        private void createAlbPublicSecurityGroup()
        {
            var albsecurityGroupProps = new CfnSecurityGroupProps
            {
                VpcId = _Request.VpcId,
                GroupDescription = "Security Group for Public ECS Application Load balancer",
                SecurityGroupIngress = new CfnSecurityGroup.IngressProperty[]
               {                 

                    new CfnSecurityGroup.IngressProperty{CidrIp = "209.190.161.36/32", Description = "Ingress to port 80", IpProtocol = "tcp", FromPort = 80, ToPort = 80 },
                    new CfnSecurityGroup.IngressProperty{CidrIp = "209.190.161.36/32", Description = "Ingress to port 443", IpProtocol = "tcp", FromPort = 443, ToPort = 443 },
               },
            };

            _CfnPublicAlbsecurityGroup = new CfnSecurityGroup(this, "PublicALBSecurityGroup", albsecurityGroupProps);
        }

        private void createECSServiceSecurityGroup()
        {
            var securityGroupProps = new CfnSecurityGroupProps
            {
                VpcId = _Request.VpcId,
                GroupDescription = "Security Group ECS Services",
                SecurityGroupIngress = new CfnSecurityGroup.IngressProperty[]
                {
                    new CfnSecurityGroup.IngressProperty { IpProtocol = "tcp", FromPort = 80, ToPort = 80, SourceSecurityGroupId = _CfnPublicAlbsecurityGroup.Ref },
                    new CfnSecurityGroup.IngressProperty { IpProtocol = "tcp", FromPort = 5000, ToPort = 5000, SourceSecurityGroupId = _CfnPublicAlbsecurityGroup.Ref },                   
                }
            };

            _CfnEcsServiceSecurityGroup = new CfnSecurityGroup(this, "ECSServiceSecurityGroup", securityGroupProps);
        }
        private void createDBSecurityGroup()
        {
            var dbsecurityGroupProps = new CfnSecurityGroupProps
            {
                VpcId = _Request.VpcId,
                GroupDescription = "Security Group for application database Services",
                SecurityGroupIngress = new CfnSecurityGroup.IngressProperty[]
                {
                    new CfnSecurityGroup.IngressProperty { IpProtocol = "tcp", FromPort = 3306, ToPort = 3306, SourceSecurityGroupId = _CfnEcsServiceSecurityGroup.Ref },
                }
            };

            new CfnSecurityGroup(this, "DBSecurityGroup", dbsecurityGroupProps);
        }

        private void createCodeBuildSecurityGroup()
        {
            var ocrCodeBuildSecurityGroup = new CfnSecurityGroupProps
            {
                VpcId = _Request.VpcId,
                GroupDescription = "Security Group for Code Build",
                GroupName = "CodeBuildSecurityGroup",
                SecurityGroupEgress = new CfnSecurityGroup.EgressProperty[]
                {
                    new CfnSecurityGroup.EgressProperty{IpProtocol = "-1", CidrIp = "0.0.0.0/0"}
                },               
            };

            new CfnSecurityGroup(this, "CodeBuildSecurityGroup", ocrCodeBuildSecurityGroup);
        }


    }
}
