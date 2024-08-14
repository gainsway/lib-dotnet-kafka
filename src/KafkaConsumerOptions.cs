using Confluent.Kafka;

namespace Gainsway.Kafka;

public class KafkaConsumerOptions : ConsumerConfig
{
    public const string Position = "Kafka:ConsumerSettings";
}
