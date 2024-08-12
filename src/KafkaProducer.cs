using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;

namespace Gainsway.Kafka;

/// <summary>
///     Leverages the injected KafkaClientHandle instance to allow
///     Confluent.Kafka.Message{K,V}s to be produced to Kafka.
/// </summary>
public abstract class KafkaProducer<K, V>(KafkaClientHandle handle) : IKafkaProducer<K, V>
{
    protected abstract string Topic { get; }

    readonly IProducer<K, V> kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle)
        .SetKeySerializer(new SchemaLessSerializer<K>())
        .SetValueSerializer(new SchemaLessSerializer<V>())
        .BuildWithInstrumentation();

    /// <summary>
    ///     Asychronously produce a message and expose delivery information
    ///     via the returned Task. Use this method of producing if you would
    ///     like to await the result before flow of execution continues.
    /// </summary>
    public Task ProduceAsync(K key, V value) =>
        kafkaHandle.ProduceAsync(Topic, new Message<K, V> { Key = key, Value = value });

    /// <summary>
    ///     Sychronously produce a message and expose delivery information
    ///     via the provided callback function. Use this method of producing
    ///     if you would like flow of execution to continue immediately, and
    ///     handle delivery information out-of-band.
    /// </summary>
    public void Produce(K key, V value, Action<DeliveryReport<K, V>>? deliveryHandler = null) =>
        kafkaHandle.Produce(Topic, new Message<K, V> { Key = key, Value = value }, deliveryHandler);

    public void Flush(TimeSpan timeout) => kafkaHandle.Flush(timeout);
}
