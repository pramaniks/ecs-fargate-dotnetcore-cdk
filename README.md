# AWS ECS cluster fargate serverless environment
**This document will show the high level architecture of the CI/CD pipeline using dot net core microservices based on Domain driven design pattern**

# Assumptions/ Prerequisites:
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

# What this project will do:
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
  * Create the docher image from the build.
  * Push the docker image to AWS ECR image repository.
  * Push the build artifacts in S3 bucket.
* The build artifacts pushed to S3 bucket will be consumed by the AWS code deploy to create a deployment in ECS cluster fargate serverless environment.

# Creating deployment environment on windows machine:
* **Chocolatey installation - A package manager for windows environment**
  * Open powershell as adminstrator and execute following command:
    * `Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))`
* Install dot net core 3+ version SDK using following command
  *  `choco install dotnetcore-sdk`
* Install Python using folllowing command:
  * `choco install python`
  which will be shown in later steps.
* Install nodejs/ npm using command:
  * `choco install -y --force nodejs` and check version using command `node -v`
* Install AWS CDK globally using following command:
  * `npm install -g aws-cdk` and check the version using command `cdk --version`
