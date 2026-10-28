## Description
ASP.NET Core API for gamers to manage their game backlog: track status, rate games, take notes and keep a checklist of objectives.

## Prerequisites
- .NET SDK 10
- PostgreSQL 18
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Setup
1. Start PostgreSQL:
   ```
   brew services start postgresql@18
   ```
2. Create the database:
   ```
   psql -d postgres -c "CREATE DATABASE gamebacklog_dev;"
   ```
3. Create a `.env` file at the repository root from the example, then fill in your values (see `.env.example` for every variable):
   ```
   cp .env.example .env
   ```
   Generate `JWT_KEY`.
4. Create the tables:
   ```
   dotnet ef database update --project src/GameBacklog.Infrastructure --startup-project src/GameBacklog.Api
   ```

## Run
```
dotnet run --project src/GameBacklog.Api --launch-profile http
```
Swagger: http://localhost:5112/swagger

## Run on Kubernetes (kind)
1. Create the cluster (1 control plane, 2 workers):
   ```
   kind create cluster --name gamebacklog --config kind-config.yaml
   ```
2. Build the image and load it into the cluster:
   ```
   docker build -t gamebacklog-api:v1 .
   kind load docker-image gamebacklog-api:v1 --name gamebacklog
   ```
3. Create the secret (values stay out of the repo):
   ```
   kubectl create secret generic db-secret \
     --from-literal=POSTGRES_PASSWORD='choose-a-password' \
     --from-literal=JWT_KEY="$(openssl rand -base64 48)"
   ```
4. Deploy and open the API:
   ```
   kubectl apply -f k8s/
   kubectl port-forward svc/api 8080:80
   ```
   Health: http://localhost:8080/health/ready
5. Delete the cluster when done:
   ```
   kind delete cluster --name gamebacklog
   ```
