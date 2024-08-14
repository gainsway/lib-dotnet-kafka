using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gainsway.Kafka.Tests
{
    [TestFixture]
    public class KafkaConsumerTests
    {
        private IOptions<KafkaConsumerOptions> _mockConsumerConfig;
        private ILogger<KafkaConsumer<string, string>> _mockLogger;
        private TestKafkaConsumer _kafkaConsumer;

        private IConsumer<string, string> _mockKafkaConsumer;
        private Func<ConsumeResult<string, string>, CancellationToken, Task> _handleEvent;

        [SetUp]
        public void Setup()
        {
            _handleEvent = Substitute.For<
                Func<ConsumeResult<string, string>, CancellationToken, Task>
            >();
            _mockConsumerConfig = Substitute.For<IOptions<KafkaConsumerOptions>>();
            _mockConsumerConfig.Value.Returns(new KafkaConsumerOptions { GroupId = "test-group" });
            _mockLogger = Substitute.For<ILogger<KafkaConsumer<string, string>>>();
            _mockKafkaConsumer = Substitute.For<IConsumer<string, string>>();
            _kafkaConsumer = new TestKafkaConsumer(
                _mockConsumerConfig,
                _mockLogger,
                _mockKafkaConsumer,
                _handleEvent
            );
        }

        [TearDown]
        public void TearDown()
        {
            _kafkaConsumer.Dispose();
            _mockKafkaConsumer.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_ShouldStartConsumerLoop()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            _ = _kafkaConsumer.TestExecuteAsync(cancellationToken);

            // Assert
            _mockKafkaConsumer.Received(1).Subscribe(_kafkaConsumer.GetTopic());
            await _mockKafkaConsumer
                .Received()
                .ConsumeWithInstrumentation(_handleEvent, cancellationToken);
        }

        [Test]
        public void Dispose_ShouldCloseAndDisposeConsumer()
        {
            // Act
            _kafkaConsumer.Dispose();

            // Assert
            _mockKafkaConsumer.Received(1).Close();
            _mockKafkaConsumer.Received(1).Dispose();
        }

        protected class TestKafkaConsumer(
            IOptions<KafkaConsumerOptions> consumerConfig,
            ILogger<KafkaConsumer<string, string>> logger,
            IConsumer<string, string> mockKafkaConsumer,
            Func<ConsumeResult<string, string>, CancellationToken, Task>? handleEvent = null
        ) : KafkaConsumer<string, string>(consumerConfig, logger, mockKafkaConsumer)
        {
            protected override string Topic => "test-topic";

            protected override Task HandleEvent(
                ConsumeResult<string, string> consumeResult,
                CancellationToken cancellationToken
            ) => handleEvent(consumeResult, cancellationToken);

            protected override void HandleEventException(ConsumeException e)
            {
                // Implement your test-specific logic here
            }

            protected override void HandleEventException(Exception e)
            {
                // Implement your test-specific logic here
            }

            public Task TestExecuteAsync(CancellationToken cancellationToken)
            {
                var t = ExecuteAsync(cancellationToken);
                Thread.Sleep(100);
                return t;
            }

            public string GetTopic()
            {
                return Topic;
            }
        }
    }
}
