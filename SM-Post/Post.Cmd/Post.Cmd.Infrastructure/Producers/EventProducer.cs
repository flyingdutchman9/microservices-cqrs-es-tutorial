﻿using Confluent.Kafka;
using CQRS.Core.Events;
using CQRS.Core.Producers;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Post.Cmd.Infrastructure.Producers
{
    public class EventProducer : IEventProducer
    {
        private readonly ProducerConfig _config;

        public EventProducer(IOptions<ProducerConfig> config)
        {
            _config = config.Value;
        }

        public async Task ProduceAsync<T>(string topic, T @event) where T : BaseEvent
        {
            using (var producer = new ProducerBuilder<string, string>(_config)
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(Serializers.Utf8)
                .Build())
            {
                // Poruka mora imati iste key-value tipove kao i Producer
                var eventMessage = new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = JsonSerializer.Serialize(@event, @event.GetType())
                };

                var deliveryResult = await producer.ProduceAsync(topic, @eventMessage);

                // U slučaju greške spremamo u log za koji event i topic se poruka nije mogla poslati
                if (deliveryResult.Status == PersistenceStatus.NotPersisted)
                {
                    throw new Exception($"Could not produce {@event.GetType().Name} message to topic - {topic} due to a following reason:{deliveryResult.Message}!");
                }
            }

        }
    }
}
