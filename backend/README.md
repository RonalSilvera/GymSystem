# Backend Setup

Commands to prepare the database:

```
dotnet tool update --global dotnet-ef
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef database update -p 00.Persistence/Infrastructure/Infrastructure.csproj -s 02.Service/API/API.csproj
```

## Manual Auth Tests

After building the solution run the API and use Swagger to test authentication:

```bash
dotnet restore
dotnet build
```

1. **Login**: `POST /api/Auth/login` with a valid user returns `token`, `expiresAtUtc`, `role` and `email`.
2. Use the `Authorize` button in Swagger with `Bearer <token>`.
3. **Me**: `GET /api/Auth/me` responds `200` with claims.
4. **Ping**: `GET /api/_diag/ping` responds `200` without token.
5. **Ping Auth**: `GET /api/_diag/ping-auth` responds `401` without token and `200` with token.

