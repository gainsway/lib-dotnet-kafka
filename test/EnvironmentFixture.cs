using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Kafka;

namespace Gainsway.Kafka.Tests;

public class EnvironmentFixture : IAsyncDisposable
{
    private const string KafkaContainerName = "kafka-tests";

    private readonly IDockerEnvironment _dockerEnvironment;

    public EnvironmentFixture()
    {
        _dockerEnvironment = new DockerEnvironmentBuilder()
            .AddKafkaContainer(p =>
                p with
                {
                    Name = KafkaContainerName,
                    ImageName = "dougdonohoe/kafka-zookeeper",
                    Tag = "2.6.0"
                }
            )
            .Build();
    }

    public string KafkaBootstrapServers
    {
        get
        {
            var kafkaContainer =
                _dockerEnvironment.GetContainer<KafkaContainer>(KafkaContainerName)
                ?? throw new InvalidOperationException("Kafka container not found");
            return kafkaContainer.GetUrl();
        }
    }

    public async Task InitializeAsync()
    {
        await _dockerEnvironment.UpAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dockerEnvironment.DownAsync();
    }
}
