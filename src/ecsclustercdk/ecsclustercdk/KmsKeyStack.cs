using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.KMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecsclustercdk
{
    public class CreateKmsKeyStackRequest
    {
        public string AccountId { get; set; }
    }
    public class KmsKeyStack : Stack
    {
        private CreateKmsKeyStackRequest _Request;

        internal KmsKeyStack(Construct scope, string id, CreateKmsKeyStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;

            createBucketKey();
        }

        private void createBucketKey()
        {
            var rolePolicyDocument = new PolicyDocument();
            var conditionDictionary = new Dictionary<string, object>();
            var dict = new Dictionary<string, string>();
            dict.Add("kms:CallerAccount", _Request.AccountId);
            conditionDictionary.Add("StringEquals", dict);
            var rolePolicyStatement = new PolicyStatement(new PolicyStatementProps
            {
                Sid = "Allow direct access to key metadata to any user in the account",
                Actions = new string[] {
                "kms:Create*",
                "kms:Encrypt",
                "kms:Describe*",
                "kms:Enable*",
                "kms:List*",
                "kms:Put*",
                "kms:Update*",
                "kms:Revoke*",
                "kms:Disable*",
                "kms:Get*",
                "kms:Delete*",
                "kms:ScheduleKeyDeletion",
                "kms:CancelKeyDeletion",
                 "kms:ReEncrypt*",
                "kms:GenerateDataKey*",
                "kms:Decrypt"},
                Effect = Effect.ALLOW,
                Conditions = conditionDictionary
            });

            rolePolicyStatement.AddArnPrincipal($"arn:aws:iam::{_Request.AccountId}:root");
            rolePolicyStatement.AddAllResources();

            rolePolicyDocument.AddStatements(rolePolicyStatement);

            var bucketKey = new CfnKey(this, "SampleKMSKey", new CfnKeyProps
            {
                Description = $"KMS Key for code pipeline",
                EnableKeyRotation = true,
                KeyPolicy = rolePolicyDocument,
            });

            new CfnAlias(this, $"SampleKMSKeyAlias", new CfnAliasProps { AliasName = $"alias/SampleKMSKey", TargetKeyId = bucketKey.Ref });
        }
    }
}
