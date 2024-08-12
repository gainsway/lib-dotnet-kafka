using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using Microsoft.Extensions.Options;

namespace Gainsway.Kafka;

public class KafkaClientHandle(IOptions<KafkaProducerOptions> producerConfig) : IDisposable
{
    readonly IProducer<byte[], byte[]> kafkaProducer = new ProducerBuilder<byte[], byte[]>(
        producerConfig.Value
    ).BuildWithInstrumentation();

    public Handle Handle
    {
        get => kafkaProducer.Handle;
    }

    public void Dispose()
    {
        // Block until all outstanding produce requests have completed (with or
        // without error).
        kafkaProducer.Flush();
        kafkaProducer.Dispose();
    }
}
