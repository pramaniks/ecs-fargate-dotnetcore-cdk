# AWS ECS cluster fargate serverless environment
:star2: **This document will demonstrate the what and how's of the CI/CD pipeline for dot net core microservices based on Domain driven design pattern using AWS ECS Fargate serverless cluster** :star2:

# Assumptions/ Prerequisites: :v:
* All the domain projects are **.net core web API project types** with .net core 3.1 target framework.
* All the services will be deployed to existing ECS cluster on Fargate launch type which is a serverless offering of ECS cluster.
* AWS environment readiness:
  * VPC is created.
  * The subnets are ready for example front end , backend and Database subnets.
  * Route tables are configured.
  * Internet gateways and NAT gateways are configured.
  * ECS Cluster is ready.
  * ECR image repositories are created.
  * ECS fargate services should also be created.
  * ECS task definitions should be created.
  * AWS Code star connection respository should be created in AWS environment.

# What this project will do: :muscle:
* This project will be a CDK to create and deploy AWS code pipeline that will listen to any changes in the repository configured in the build project.
* CDK will create the following resources on AWS:
  * AWS build project.
  * AWS Code pipeline project.
  * AWS Code deploy.
  * AWS Code build project IAM roles.
  * AWS Code pipeline IAM roles.
  * Private S3 bucket for source and build artifacts
* This project will also specify how to create and abstract docker image files for each microservice domain project independent of the service itself.
* This project will also demonstrate how to create buildspec.yml file for each microservice project which will do following operations:
  * Build the .net core project with the restoring packages.
  * Generate the build artifacts.
  * Create the docker image from the build.
  * Push the docker image to AWS ECR image repository.
  * Push the build artifacts in S3 bucket.
* The build artifacts pushed to S3 bucket will be consumed by the AWS code deploy to create a deployment in ECS cluster fargate serverless environment.

# Architecture diagram for deploying microservices on AWS ECS serverless cluster: :gem:
![ECSCluster](https://user-images.githubusercontent.com/20775313/114203255-9ed64d00-9975-11eb-932b-354840173bdb.jpeg)

# Creating deployment environment on windows machine: :hammer:
 ###### **Please open powershell as an administrator to execute following commands:**
* Chocolatey installation - A package manager for windows environment
  * `Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))`
* Install dot net core 3+ version SDK using following command
   *  `choco install dotnetcore-sdk`
* Install Python using folllowing command:
   * `choco install python`
* Install nodejs/ npm using command:
   * `choco install -y --force nodejs` and check version using command `node -v`after installation.
 * Install AWS CDK globally using following command:
   * `npm install -g aws-cdk` and check the version using command `cdk --version` after installation.

# Configuring AWS credentials on local machine
* On powershell execute command `aws configure` and please fill in **Access Key Id**, **SecretKey** and **AWS region**
![image](https://user-images.githubusercontent.com/20775313/114186138-1e5b2080-9964-11eb-974f-a8c76772f93a.png)
* Once aws configure is completed you can see credentials file created under your user profile C:\Users\username\aws\
* Open the credentials file via file editor and add **aws_session_token** attribute as well.

# :skull:
* **THIS IS VERY IMPORTANT WHEN DEPLOYING TO AWS ENVIRONMENT ALWAYS USE SESSION CREDENTIALS WHICH ARE SHORT LIVED AND EXPIRES AFTER AN HOUR**.
 ![image](https://user-images.githubusercontent.com/20775313/114187829-113f3100-9966-11eb-82c0-aa746c674dfd.png)
 
 
 


# Deployment :construction_worker:
* cd to cdk.json file under src directory
* cd C:\ecs-fargate-dotnetcore-cdk\src\ecsclustercdk\
* cdk.json file has the information of which project needs to be built
* cdk.json file contents:\
{\
  "app": "dotnet run -p ecsclustercdk/ecsclustercdk.csproj",\
  "profile" : "default"\
}
* Run command `dotnet build`
![image](https://user-images.githubusercontent.com/20775313/115144172-d83f4480-a068-11eb-88a4-3fa1ddabbd88.png)

* The cloud deployment settings json file looks like this:\
{\
  "EnvironmentQualifier" :  "Dev"\
      "Dev": {\
        "AccountId": "714911308443",\
        "Region": "us-east-2",\
        "VpcId": "vpc-06a4c67ee500fdb59",\
        "FrontendSubnetIdList": [ "subnet-01e1349a99c93d1c4", "subnet-0d60780ad67a1c829" ],\
        "BackendSubnetIdList": [ "subnet-01e1349a99c93d1c4", "subnet-0d60780ad67a1c829" ],\
        "DatabaseSubnetIdList": [ "subnet-01e1349a99c93d1c4", "subnet-0d60780ad67a1c829" ],\
        "CodePipelineBucketName": "samplecodepipelinebucket",\
        "CodePipelineServiceRoleName": "SampleCodePipelineRole",\
        "CodeBuildServiceRoleName": "SampleCodeBuildRole",\
        "TaskExecutionRoleName": "SampleTaskRole",\
        "BucketKmsKeyGuid": "c2dce8e8-bfc6-4b1f-9090-97045db83d2a",\
        "CodeBuildSecurityGroupId": "sg-02fc7f9d4c8b1bd55",\
        "ECSServiceSecurityGroupId": "sg-0664a4574b1dfb4c6",\
        "ECSClusterName": "samplecluster",\
        "RepositoryConnection": {\
          "ConnectionArn": "arn:aws:codestar-connections:us-east-2:714911308443:connection/84e3bd57-26c0-41e8-a7db-c528e03f7beb",\
          "RepositoryId": "pramaniks/dotnetcore-microservices-webapi",\
          "BranchName": "master"\
        }\
      }\
    }

* After successful build start deploying the stack one by one in the following order:
   * `cdk deploy SecurityGroupStack`
   
   ![SecurityGroupStack](https://user-images.githubusercontent.com/20775313/115148268-6f61c780-a07c-11eb-8327-e324d46b4e45.PNG)
   * `cdk deploy KMSKeyStack`
   
   ![KMSKeyStack](https://user-images.githubusercontent.com/20775313/115148289-899ba580-a07c-11eb-9d84-095f6cd0ec41.PNG)
   
   * `cdk deploy BucketStack`
   
   ![S3BucketStack](https://user-images.githubusercontent.com/20775313/115148348-be0f6180-a07c-11eb-90b6-bc2177302e25.PNG)
   
   * `cdk deploy RolesStack`
   
   ![RolesStack](https://user-images.githubusercontent.com/20775313/115148363-d1bac800-a07c-11eb-9977-e1438c1caa20.PNG)
   
   * `cdk deploy RolesPolicyStack`

   ![RolesPolicyStack](https://user-images.githubusercontent.com/20775313/115148376-e39c6b00-a07c-11eb-8370-1600a7c68768.PNG)
   
   * `cdk deploy TaskDefinitionStack`
   
   ![TaskDefinitionStack](https://user-images.githubusercontent.com/20775313/115148392-fca51c00-a07c-11eb-81a0-86841496d091.PNG)
   
   * `cdk deploy ECSClusterStack`

   ![ECSClusterStack](https://user-images.githubusercontent.com/20775313/115148414-134b7300-a07d-11eb-96c2-fd00d27e14e8.PNG)
   
   * `cdk deploy ECSServiceStack`. **Please note here, the task desired count for new service always keep as 0 to avoid immediate service start**
   
   ![ECSServiceStack](https://user-images.githubusercontent.com/20775313/115148454-4e4da680-a07d-11eb-9b9b-ee2e7a1af5d5.PNG)
   
   * `cdk deploy ECSCodepipelineStack`

   ![ECSCodepipelineStack](https://user-images.githubusercontent.com/20775313/115148470-632a3a00-a07d-11eb-92e0-205690dcff6a.PNG)
   
   
   
# Initially the **Code deploy stage** in Code pipeline for the new service will be in error state , but why ?? :thought_balloon: :thought_balloon: :thought_balloon:



 
![CodePipelineInitial_1](https://user-images.githubusercontent.com/20775313/115148537-a8e70280-a07d-11eb-9522-bd50835995f9.PNG)
![CodePipelineInitial_2](https://user-images.githubusercontent.com/20775313/115148542-ae444d00-a07d-11eb-80f9-3dd2733e6e5a.PNG)

# Do you remember we deployed the Service Stack with task desired count as "0"  :bulb: :pushpin:



* Now deploy ECSService stack again with task desired Count as "1" or whatever task instances you need for the newly created service(s) \
var CfnServiceProps = new CfnServiceProps\
            {\
                Cluster = _Request.ECSClusterName,\
                LaunchType = "FARGATE",\
                ServiceName = Request.ServiceName,\
                TaskDefinition = Request.TaskDefinition,\
                DesiredCount = 1,\
                LoadBalancers = new object[1]\
                 {\
                    new LoadBalancerProperty\
                    {\
                        ContainerName = Request.ContainerName,\
                        ContainerPort = 5000,\
                        TargetGroupArn = $"arn:aws:elasticloadbalancing:{_Request.Region}:{_Request.AccountId}:targetgroup/{Request.TargetGroupId}"\
                    }\
                 },\
                NetworkConfiguration = new NetworkConfigurationProperty\
                {\
                    AwsvpcConfiguration = new AwsVpcConfigurationProperty\
                    {\
                        AssignPublicIp = "ENABLED",\
                        SecurityGroups = new string[] { _Request.EcsServiceSecurityGroupId },\
                        Subnets = _Request.BackendSubnetIdList.ToArray()\
                    }\
                }\
            };
            
 * Now the service is running the task with the desired count successfully without any issues
 
 ![ClusterRunningService](https://user-images.githubusercontent.com/20775313/115148619-0c713000-a07e-11eb-93dd-7fceb18d3c28.PNG)
 
 * Now go to the code pipeline for that service and click on Release Change, now the code pipeline executes all the steps successfully.
 
 ![CodePipelineSuccessfulMessage](https://user-images.githubusercontent.com/20775313/115151252-b4402b00-a089-11eb-8127-3b8c4a56ea4f.PNG)
 
 * The service deployment from code pipline in the cluster will be running successfully
 
 ![DeploymentSuccessful](https://user-images.githubusercontent.com/20775313/115151331-084b0f80-a08a-11eb-9352-6002bd0622fe.PNG)
 
 # Service is now up and running :clap: :clap: :tada: :tada:
 
 
 
 
 # Un-deployment (destroying the stack) :electric_plug:
 
 **Please un-deploy the stack in the reverse order of deployment or as per the dependencies**
 
 * `cdk destroy ECSCodepipelineStack`
 
 ![CodePipelineDestroy](https://user-images.githubusercontent.com/20775313/115153214-62e86980-a092-11eb-96f4-1d3ac3a5001a.PNG)

* `cdk destroy ECSServiceStack`

![ECSServiceStackDestroy](https://user-images.githubusercontent.com/20775313/115153229-785d9380-a092-11eb-91e2-789b27ea1eb1.PNG)

* `cdk destroy ECSClusterStack`

![ECSClusterStackDestroy](https://user-images.githubusercontent.com/20775313/115153249-8ca19080-a092-11eb-84c4-145265ff0c7b.PNG)

 * `cdk destroy TaskDefinitionStack`

![TaskDefinitionStackDestroy](https://user-images.githubusercontent.com/20775313/115153265-9dea9d00-a092-11eb-98a0-c412975eac3f.PNG)

 * `cdk destroy RolesStack`
 
 ![RolesStackDestroy](https://user-images.githubusercontent.com/20775313/115153284-ba86d500-a092-11eb-916f-1916a01b8892.PNG)
 
 * `cdk destroy BucketStack`

![S3BucketStackDestroy](https://user-images.githubusercontent.com/20775313/115153303-d4281c80-a092-11eb-8b62-7b5c68580cf9.PNG)

* `cdk destroy KMSKeyStack`

![KMSKeyStackDestroy](https://user-images.githubusercontent.com/20775313/115153390-4567cf80-a093-11eb-9083-53ab6a938b3f.PNG)


* `cdk destroy SecurityGroupStack`

![SecurityGroupStackDestroy](https://user-images.githubusercontent.com/20775313/115153337-03d72480-a093-11eb-9e3f-627847a19def.PNG)



# Now that's all about it :relaxed:. Till then happy Coding !! :metal::muscle:













