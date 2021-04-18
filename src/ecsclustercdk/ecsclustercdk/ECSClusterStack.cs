using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecsclustercdk
{
    public class CreateECSClusterStackRequest
    {
        public string ClusterName { get;  set; }
    }
    public class ECSClusterStack : Stack
    {
        private CreateECSClusterStackRequest _Request;

        internal ECSClusterStack(Construct scope, string id, CreateECSClusterStackRequest Request, IStackProps props = null) : base(scope, id, props)
        {
            _Request = Request;

            createEcsCluster();
           
        }
        private void createEcsCluster()
        {
            var cfnClusterProps = new CfnClusterProps
            {
                ClusterName = _Request.ClusterName,                
            };

           new CfnCluster(this, _Request.ClusterName, cfnClusterProps);
        }
    }

   

}
