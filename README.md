# monorepo-clean-architecture
( POC ) ASP.NET Core Web API with Clean Architecture and Monorepo solution file and project structure

### USED TECHNOLOGIES

##### Entity Framework Core
>- Code-First Database Modeling
>- Migration
>- Data Seeding

### SOURCE CODE MANAGEMENT
>- Monorepo (Single-repository) with loosely-coupled Clean Architecture layers — Presentation, Application, Infrastructure, Domain
>- Git Version Control System with Trunk-based Development branching strategy

#### Branching Strategy Guideline
> Branching name formats:
> - Contribution: `contribution/{{github-username}}/{{short-description}}`
> - Issue: `issue/{{issue-number}}/{{github-username}}/{{short-description}}`
> - Proof-Of-Concept: `poc/{{github-username}}/{{short-description}}`

### NUKE BUILD SYSTEM
> (Under Construction) Primarily using `NukeBuild` for the build system, aiming to help with the continuous integration pipelines and spinning-up local instances for development.
> - Clean: Clean-up of the projects in the solution
> - Restore: Restore package dependencies in the solution
> - Compile: Complete build of the projects of the solution
> - Publish: Generate artifacts and publish to dedicated directory path
> - DockerBuildImage: Build docker images of selected projects available in local docker registry
> - DockerComposeUp: Spin-up compose configuration for docker instances
> - DockerComposeDown: Tearing-down docker instances related to the compose configuration
> - DockerComposeRestart: Restart docker instances related to the compose configuration
