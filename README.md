# AWS ECS Cluster Fargate launch type serverless environment
*This document will show the high level architecture of the CI/CD pipeline using dot net core microservices based on Domain driven design pattern*

# Assumptions
* All the domain projects are **.net core WEB API project types** with .net core 3.1 target framework.
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
* This CDK will created the build project.
* This CDK will create the Code pipeline project.
* This project will also specify how to create and abstract docker image files for each microservice domain project independent of the service itself.
* This project will also demonstrate how to create buildspec.yml file for each microservice project which will do following operations:
  * Build the .net core project with the restoring packages.
  * Generate the build artifacts.
  * Create the docher image from the build.
  * Push the docker image to AWS ECR image repository.
  * Push the build artifacts in S3 bucket.
* The build artifacts now pushed to S3 bucket will be consumed by the AWS code deploy to create a deployment in ECS cluster fargate serverless environment.
