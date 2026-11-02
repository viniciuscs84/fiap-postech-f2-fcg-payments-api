# PaymentsAPI

Independent .NET 8 foundation for the Phase 2 simulated payment service.

```powershell
dotnet build src/PaymentsAPI/PaymentsAPI.csproj
dotnet run --project src/PaymentsAPI/PaymentsAPI.csproj
```

`/health` reports process availability. The RabbitMQ consumer, configurable `Approved`/`Rejected` simulation, and payment result publisher belong to Wave 2. No payment credentials or provider integration are required.
