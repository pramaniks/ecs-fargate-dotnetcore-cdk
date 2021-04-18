using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using System;
using System.Collections.Generic;
using System.Text;
using static Amazon.CDK.AWS.S3.CfnBucket;

namespace Ecsclustercdk
{
    public class CreateBucketStackRequest
    {
        public string CodePipelineBucketName { get; set; }
        public string BucketKeyId { get; set; }
    }
    public class BucketStack : Stack
    {
        private CreateBucketStackRequest _Request;

        internal BucketStack(Construct scope, string id, CreateBucketStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;

            createCodePipelineBucket();
        }

        private void createCodePipelineBucket()
        {
            var cfnBucketProps = new CfnBucketProps
            {
                BucketName = _Request.CodePipelineBucketName,
                PublicAccessBlockConfiguration = new CfnBucket.PublicAccessBlockConfigurationProperty
                {
                    RestrictPublicBuckets = true,
                    BlockPublicAcls = true,
                    BlockPublicPolicy = true,
                    IgnorePublicAcls = true
                },
                BucketEncryption = new BucketEncryptionProperty
                {
                    ServerSideEncryptionConfiguration = new ServerSideEncryptionRuleProperty[]
                    {
                        new ServerSideEncryptionRuleProperty
                        {
                            ServerSideEncryptionByDefault = new ServerSideEncryptionByDefaultProperty{SseAlgorithm = "aws:kms" ,KmsMasterKeyId =_Request.BucketKeyId}
                        }
                    }
                },
            };

            new CfnBucket(this, _Request.CodePipelineBucketName, cfnBucketProps);
        }
    }
}
