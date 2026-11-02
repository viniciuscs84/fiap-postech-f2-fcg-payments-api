using RabbitMQ.Client;

namespace PaymentsAPI.Messaging;

internal static class RabbitMqConnectionFactory
{
    public static ConnectionFactory Create(RabbitMqOptions options, string clientName) => new()
    {
        HostName = options.HostName,
        Port = options.Port,
        UserName = options.UserName,
        Password = options.Password,
        ClientProvidedName = clientName,
        AutomaticRecoveryEnabled = true
    };
}
