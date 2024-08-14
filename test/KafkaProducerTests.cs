using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Gainsway.Kafka.Tests
{
    public sealed class KafkaProducerTests : IAsyncDisposable
    {
        private readonly EnvironmentFixture _environmentFixture = new EnvironmentFixture();
        private TestKafkaProducer _producer;
        private IConsumer<string, string> _consumer;

        public async ValueTask DisposeAsync()
        {
            await _environmentFixture.DisposeAsync();
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await _environmentFixture.InitializeAsync();

            var kafkaBootstrapServers = _environmentFixture.KafkaBootstrapServers;
            var producerOptions = Options.Create(
                new KafkaProducerOptions { BootstrapServers = kafkaBootstrapServers }
            );
            _producer = new TestKafkaProducer(new KafkaClientHandle(producerOptions));
            _consumer = new ConsumerBuilder<string, string>(
                Options
                    .Create(
                        new KafkaConsumerOptions
                        {
                            GroupId = "test-group",
                            BootstrapServers = kafkaBootstrapServers,
                            AutoOffsetReset = AutoOffsetReset.Earliest
                        }
                    )
                    .Value
            ).SetKeyDeserializer(new SchemaLessDeserializer<string>()).SetValueDeserializer(new SchemaLessDeserializer<string>()).Build();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _consumer.Dispose();
            await _environmentFixture.DisposeAsync();
        }

        [Test]
        public async Task ProduceConsumeAsync()
        {
            // Arrange
            (string key, string value) expectedMessageValue = ("key", "value");
            // Act
            await _producer.ProduceAsync(expectedMessageValue.key, expectedMessageValue.value);
            _consumer.Subscribe(_producer.GetTopic());

            var consumeResult = _consumer.Consume(CancellationToken.None);

            // Assert
            Assert.That(consumeResult.Message.Key, Is.EqualTo(expectedMessageValue.key));
            Assert.That(consumeResult.Message.Value, Is.EqualTo(expectedMessageValue.value));
        }
    }

    internal class TestKafkaProducer(KafkaClientHandle handle)
        : KafkaProducer<string, string>(handle),
            ITestKafkaProducer
    {
        protected override string Topic => "test-topic";

        public string GetTopic() => Topic;
    }

    internal interface ITestKafkaProducer : IKafkaProducer<string, string> { }
}
