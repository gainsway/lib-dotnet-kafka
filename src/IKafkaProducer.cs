using Confluent.Kafka;

namespace Gainsway.Kafka;

public interface IKafkaProducer<K, V>
{
    /// <summary>
    ///     Asychronously produce a message and expose delivery information
    ///     via the returned Task. Use this method of producing if you would
    ///     like to await the result before flow of execution continues.
    /// </summary>
    public Task ProduceAsync(K key, V value);

    /// <summary>
    ///     Sychronously produce a message and expose delivery information
    ///     via the provided callback function. Use this method of producing
    ///     if you would like flow of execution to continue immediately, and
    ///     handle delivery information out-of-band.
    /// </summary>
    public void Produce(K key, V value, Action<DeliveryReport<K, V>>? deliveryHandler = null);

    //
    // Summary:
    //     Wait until all outstanding produce requests and delivery report callbacks are
    //     completed. [API-SUBJECT-TO-CHANGE] - the semantics and/or type of the return
    //     value is subject to change.
    //
    // Parameters:
    //   timeout:
    //     The maximum length of time to block. You should typically use a relatively short
    //     timeout period and loop until the return value becomes zero because this operation
    //     cannot be cancelled.
    //
    // Returns:
    //     The current librdkafka out queue length. This should be interpreted as a rough
    //     indication of the number of messages waiting to be sent to or acknowledged by
    //     the broker. If zero, there are no outstanding messages or callbacks. Specifically,
    //     the value is equal to the sum of the number of produced messages for which a
    //     delivery report has not yet been handled and a number which is less than or equal
    //     to the number of pending delivery report callback events (as determined by the
    //     number of outstanding protocol requests).
    //
    // Remarks:
    //     This method should typically be called prior to destroying a producer instance
    //     to make sure all queued and in-flight produce requests are completed before terminating.
    //     The wait time is bounded by the timeout parameter. A related configuration parameter
    //     is message.timeout.ms which determines the maximum length of time librdkafka
    //     attempts to deliver a message before giving up and so also affects the maximum
    //     time a call to Flush may block. Where this Producer instance shares a Handle
    //     with one or more other producer instances, the Flush method will wait on messages
    //     produced by the other producer instances as well.
    public void Flush(TimeSpan timeout);
}
