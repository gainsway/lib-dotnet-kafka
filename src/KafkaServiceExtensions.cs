using Microsoft.Extensions.DependencyInjection;

namespace Gainsway.Kafka;

public static class KafkaServiceExtensions
{
    public static void AddKafkaServices(this IServiceCollection services)
    {
        services.AddSingleton<KafkaClientHandle>();

        services
            .AddOptions<KafkaConsumerOptions>()
            .BindConfiguration(KafkaConsumerOptions.Position)
            .ValidateOnStart();

        services
            .AddOptions<KafkaProducerOptions>()
            .BindConfiguration(KafkaProducerOptions.Position)
            .ValidateOnStart();
    }
};
