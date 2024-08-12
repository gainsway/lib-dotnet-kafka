# lib-dotnet-kafka

This library add Kafka producer and consumer functionalities for .NET Core apps.

## Configuration

### Publisher

Delegate published can be created inhereting from the Gainsway.Kafka.Publisher 

```csharp

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