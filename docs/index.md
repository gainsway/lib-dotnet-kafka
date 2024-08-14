# lib-dotnet-kafka

.NET client for Apache Kafka. This library exposes wrappers around Confluent Kafka client pre-configured with gainsway requirements i.e. telemetry data

## Configuration

In order to interact with an hypothetical _analytics_ topic with a message of type of `AnalyticsEvent`, the following producer and consumer can be setup.

```json
// appsettings.json
{
  // ...
  "ServiceName"  : "my-service",
  "Kafka": {
    "ProducerSettings": {
        "BootstrapServers": "localhost:9092"
    },
    "ConsumerSettings": {
        "BootstrapServers": "localhost:9092",
        "GroupId": "my-service"
    },
    "Topics": {
        "Analytics": "analytics.event.v1",
        "EntityUpdates": "entity.event.update.v1"
    }
  }
}
```

### Producer

Delegate published can be created inhereting from the `Gainsway.Kafka.Publisher`

```csharp
// IAnalyticsProducer.cs
using Gainsway.Kafka;
using Gainsway.MyService.Core.Events;

namespace Gainsway.MyService.Infrastructure.Services;

public interface IAnalyticsProducer : IKafkaProducer<string, AnalyticsEvent> { }
```

```csharp
// AnalyticsProducer.cs
using Gainsway.Kafka;
using Gainsway.MyService.Core.Events;

namespace Gainsway.MyService.Infrastructure.Services;

public class AnalyticsProducer(KafkaClientHandle handle, IConfiguration configuration)
    : KafkaProducer<string, AnalyticsEvent>(handle),
        IAnalyticsProducer
{
    protected override string Topic =>
        configuration.GetValue<string>("Kafka:Topics:Analytics")
        ?? throw new ArgumentNullException("Kafka:Topics:Analytics");
}
```

### Producer

Delegate published can be created inhereting from the `Gainsway.Kafka.KafkaConsumer`

```csharp
using System.Text.Json;
using Confluent.Kafka;
using Gainsway.Kafka;
using Gainsway.PocService.Core.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gainsway.PocService.Infrastructure.Services;

public class AnalyticsConsumer(
    IOptions<KafkaConsumerOptions> consumerConfig,
    IConfiguration configuration,
    ILogger<AnalyticsConsumer> logger
) : KafkaConsumer<string, AnalyticsEvent>(consumerConfig, logger)
{
    protected override string Topic =>
        configuration.GetValue<string>("Kafka:Topics:Analytics")
        ?? throw new ArgumentNullException("Kafka:Topics:Analytics");

    protected override Task HandleEvent(
        ConsumeResult<string, AnalyticsEvent> consumeResult,
        CancellationToken cancellationToken
    )
    {
        AnalyticsEvent analyticsEvent = consumeResult.Message.Value;
        logger.LogInformation(
            $"Consumed message '{JsonSerializer.Serialize(analyticsEvent)}' at: '{consumeResult.TopicPartitionOffset}'."
        );

        return Task.CompletedTask;
    }

    protected override void HandleEventException(ConsumeException e)
    {
        logger.LogError(e, "Error consuming message.");
    }

    protected override void HandleEventException(Exception e)
    {
        logger.LogError(e, "Error consuming message.");
    }
}

```

### Register Kafka DI services

Finally, all the services need to be registered in the DI container.

```csharp
using Gainsway.Kafka;

namespace Gainsway.MyService.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    // ...
    public static void AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        services.AddKafkaServices();
        services.AddHostedService<AnalyticsConsumer>();
        services.AddSingleton<IAnalyticsProducer, AnalyticsProducer>();
    }
    // ...
}
```

## Environment variables

| Environment variable                        | Description                                                                                            |
| ------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| `Kafka__ProducerSettings__BootstrapServers` | CSV list of broker host or host:port                                                                   |
| `Kafka__ProducerSettings__SaslMechanism`    | SASL mechanism to use for authentication. Supported:`GSSAPI`,`PLAIN`,`SCRAM-SHA-256`,`SCRAM-SHA-512`   |
| `Kafka__ProducerSettings__SecurityProtocol` | Protocol used to communicate with brokers. default: `plaintext`                                        |
| `Kafka__ProducerSettings__SaslUsername`     | SASL username for use with the PLAIN and SASL-SCRAM-.. mechanisms default: `''`                        |
| `Kafka__ProducerSettings__SaslPassword`     | SASL password for use with the PLAIN and SASL-SCRAM-.. mechanism default: `''`                         |
| `Kafka__ConsumerSettings__BootstrapServers` | CSV list of broker host or host:port                                                                   |
| `Kafka__ConsumerSettings__SaslMechanism`    | SASL mechanism to use for authentication. Supported:`GSSAPI`,`PLAIN`,`SCRAM-SHA-256`,`SCRAM-SHA-512`   |
| `Kafka__ConsumerSettings__SecurityProtocol` | Protocol used to communicate with brokers. default: `plaintext`                                        |
| `Kafka__ConsumerSettings__SaslUsername`     | SASL username for use with the PLAIN and SASL-SCRAM-.. mechanisms default: `''`                        |
| `Kafka__ConsumerSettings__SaslPassword`     | SASL password for use with the PLAIN and SASL-SCRAM-.. mechanism default: `''`                         |


```json
// appsettings.json
  "Kafka": {
    "ProducerSettings": {
        "BootstrapServers": "localhost:9092",
        "SaslMechanism": "Plain",
        "SecurityProtocol": "SaslSsl",
        "SaslUsername": "<confluent cloud key>",
        "SaslPassword": "<confluent cloud secret>"
    },
    "ConsumerSettings": {
        "BootstrapServers": "localhost:9092",
        "GroupId": "poc-service",
        "SaslMechanism": "Plain",
        "SecurityProtocol": "SaslSsl",
        "SaslUsername": "<confluent cloud key>",
        "SaslPassword": "<confluent cloud secret>"
    },
}
```