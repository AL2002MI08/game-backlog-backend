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
3. Create a `.env` file at the repository root containing:
   ```
   DB_URL=Host=localhost;Port=5432;Database=gamebacklog_dev;Username=YOUR_MAC_USERNAME
   ```
4. Create the tables:
   ```
   dotnet ef database update --project src/GameBacklog.Infrastructure --startup-project src/GameBacklog.Api
   ```

## Run
```
dotnet run --project src/GameBacklog.Api --launch-profile http
```
Swagger: http://localhost:5112/swagger
