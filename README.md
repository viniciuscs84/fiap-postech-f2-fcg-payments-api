# PaymentsAPI

Independent .NET 8 service for Phase 2 simulated payment processing.

```powershell
dotnet build src/PaymentsAPI/PaymentsAPI.csproj
dotnet run --project src/PaymentsAPI/PaymentsAPI.csproj
dotnet test tests/PaymentsAPI.Tests/PaymentsAPI.Tests.csproj
```

`PaymentsAPI` consumes `OrderPlacedEvent` messages from the durable
`payments.order-placed` queue, which is bound to the `order.placed` routing key
on the direct `fcg.events` exchange. It then publishes `PaymentProcessedEvent`
using the `payment.processed` routing key. The publisher declares and binds the
durable `catalog.payment-processed` and `notifications.payment-processed`
queues before publishing, allowing either consumer to start later.

`PaymentSimulation:Outcome` accepts `Approved` or `Rejected` and defaults to
`Approved`. Restart the service after changing the value to demonstrate the
other outcome.

RabbitMQ defaults to `localhost:5672` with the development user `guest`. Supply
the password at runtime instead of committing it:

```powershell
$env:RabbitMq__Password = '<local-password>'
```

The event payloads contain only user, game, price, result, and recipient email
data required by the approved flow. No payment credentials or provider
integration are used. `/health` reports process availability.
