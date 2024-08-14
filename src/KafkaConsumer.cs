using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gainsway.Kafka;

public abstract class KafkaConsumer<K, V>(
    IOptions<KafkaConsumerOptions> consumerConfig,
    ILogger<KafkaConsumer<K, V>> logger
) : BackgroundService
    where K : class
    where V : class
{
    protected abstract string Topic { get; }

    public KafkaConsumer(
        IOptions<KafkaConsumerOptions> consumerConfig,
        ILogger<KafkaConsumer<K, V>> logger,
        IConsumer<K, V>? kafkaConsumer
    )
        : this(consumerConfig, logger)
    {
        _kafkaConsumer =
            kafkaConsumer
            ?? new ConsumerBuilder<K, V>(consumerConfig.Value)
                .SetKeyDeserializer(new SchemaLessDeserializer<K>())
                .SetValueDeserializer(new SchemaLessDeserializer<V>())
                .Build();
    }

    private readonly IConsumer<K, V> _kafkaConsumer = new ConsumerBuilder<K, V>(
        consumerConfig.Value
    )
        .SetKeyDeserializer(new SchemaLessDeserializer<K>())
        .SetValueDeserializer(new SchemaLessDeserializer<V>())
        .Build();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    private async void StartConsumerLoop(CancellationToken cancellationToken)
    {
        _kafkaConsumer.Subscribe(Topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _kafkaConsumer.ConsumeWithInstrumentation(HandleEvent, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                HandleEventException(e);
                if (e.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    logger.LogError($"Error consuming message: {e.Error.Reason}");
                    break;
                }
                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                HandleEventException(e);
                break;
            }
        }
    }

    protected abstract Task HandleEvent(
        ConsumeResult<K, V> consumeResult,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///    Handle the exception thrown by the Kafka consumer.
    ///    This method is called when a ConsumeException is thrown.
    /// </summary>
    /// <param name="e"></param>
    protected abstract void HandleEventException(ConsumeException e);

    /// <summary>
    ///    Handle the exception thrown by the Kafka consumer.
    ///    This method is called when any Exception apart from  is thrown.
    /// </summary>
    /// <param name="e"></param>
    protected abstract void HandleEventException(Exception e);

    public override void Dispose()
    {
        _kafkaConsumer.Close(); // Commit offsets and leave the group cleanly.
        _kafkaConsumer.Dispose();

        base.Dispose();
    }
}
