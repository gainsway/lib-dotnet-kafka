using Confluent.Kafka;

namespace Gainsway.Kafka;

public class KafkaProducerOptions : ProducerConfig
{
    public const string Position = "Kafka:ProducerSettings";
}
