using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecsclustercdk
{

    public class RepositoryConnection
    {
        public string ConnectionArn { get; set; }
        public string RepositoryId { get; set; }
        public string BranchName { get; set; }
    }

    public class CloudConfiguration
    {
        private static IConfigurationRoot _Configuration;
        private static string _EnvironmentQualifier;
        private static CloudConfiguration _Instance;
        public string AccountId { get; set; }
        public string VpcId { get; set; }
        public string[] FrontendSubnetIdList { get; set; }
        public string[] BackendSubnetIdList { get; set; }
        public string[] DatabaseSubnetIdList { get; set; }
        public string CodePipelineBucketKeyName { get; set; }
        public string CodePipelineBucketName { get; set; }
        public string CodePipelineServiceRoleName { get; set; }
        public string TaskExecutionRoleName { get; set; }
        public string ECSClusterName { get; set; }
        public string Region { get; set; }
        public string ECSServiceSecurityGroupId { get; private set; }
        public RepositoryConnection RepositoryConnection { get; private set; }


        public static CloudConfiguration Instance
        {
            get
            {
                if (_Instance == null) { _Instance = new CloudConfiguration(); return _Instance; }
                return _Instance;
            }
        }

        public string CodeBuildServiceRoleName { get; internal set; }
        public string CodeBuildSecurityGroupId { get; private set; }
        public string BucketKmsKeyGuid { get; private set; }

        private CloudConfiguration()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("cloudsettings.json");
            _Configuration = builder.Build();
            assignConfiguration();
        }

        private void assignConfiguration()
        {
            _EnvironmentQualifier = _Configuration["EnvironmentQualifier"];
            AccountId = _Configuration[$"{_EnvironmentQualifier}:AccountId"];
            VpcId = _Configuration[$"{_EnvironmentQualifier}:VpcId"];
            FrontendSubnetIdList = _Configuration.GetSection($"{_EnvironmentQualifier}:FrontendSubnetIdList").Get<string[]>();
            BackendSubnetIdList = _Configuration.GetSection($"{_EnvironmentQualifier}:BackendSubnetIdList").Get<string[]>();
            DatabaseSubnetIdList = _Configuration.GetSection($"{_EnvironmentQualifier}:DatabaseSubnetIdList").Get<string[]>();
            CodePipelineBucketKeyName = _Configuration[$"{_EnvironmentQualifier}:CodePipelineBucketKeyName"];
            CodePipelineBucketName = _Configuration[$"{_EnvironmentQualifier}:CodePipelineBucketName"];
            CodePipelineServiceRoleName = _Configuration[$"{_EnvironmentQualifier}:CodePipelineServiceRoleName"];
            CodeBuildServiceRoleName = _Configuration[$"{_EnvironmentQualifier}:CodeBuildServiceRoleName"];
            CodeBuildSecurityGroupId = _Configuration[$"{_EnvironmentQualifier}:CodeBuildSecurityGroupId"];
            BucketKmsKeyGuid = _Configuration[$"{_EnvironmentQualifier}:BucketKmsKeyGuid"];
            RepositoryConnection = _Configuration.GetSection($"{_EnvironmentQualifier}:RepositoryConnection").Get<RepositoryConnection>();
            TaskExecutionRoleName = _Configuration[$"{_EnvironmentQualifier}:TaskExecutionRoleName"];
            ECSClusterName = _Configuration[$"{_EnvironmentQualifier}:ECSClusterName"];
            Region = _Configuration[$"{_EnvironmentQualifier}:Region"];
            ECSServiceSecurityGroupId = _Configuration[$"{_EnvironmentQualifier}:ECSServiceSecurityGroupId"];
        }
    }
}
