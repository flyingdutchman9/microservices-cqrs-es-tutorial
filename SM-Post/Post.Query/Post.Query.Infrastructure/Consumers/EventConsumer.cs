using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;
using System.Text.Json;


namespace Post.Query.Infrastructure.Consumers
{
    public class EventConsumer : IEventConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly IEventHandler _eventHandler;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public EventConsumer(IOptions<ConsumerConfig> config, IEventHandler eventHandler)
        {
            _config = config.Value;
            _eventHandler = eventHandler;
        }

        public async Task Consume(string topic)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config)
                    .SetKeyDeserializer(Deserializers.Utf8)
                    .SetValueDeserializer(Deserializers.Utf8)
                    .Build();

            consumer.Subscribe(topic);

            // Infinite loop je specifičan u filozofiji dizajna Kafke, a omogućuje:
            // Scalability: Pull model omogućuje consumerima određivanje brzine kojom obrađuju poruke -> Korisno kod skaliranja jer sami sebi određuju kapacitet obrade
            // Flexibility: Omogućuje Consumerima da sami određuju kada i koliko često dohvaćaju poruke...
            // Backpressure: Ako je Consumer preopterećen ili ima zastajkivanja u obradi, može privremeno usporiti ili stopirati primanje poruka
            while (true)
            {
                try
                {
                    var consumeResult = consumer.Consume();
                    if (consumeResult?.Message == null) continue;

                    var options = new JsonSerializerOptions { Converters = { new EventJsonConverter() } };
                    var @event = JsonSerializer.Deserialize<BaseEvent>(consumeResult.Message.Value, options);

                    // Reflection dohvaćamo specifičnu metodu na event handleru s parametrima koji odgovaraju našem tipu (PostCreatedEvent itd)
                    var handlerMethod = _eventHandler.GetType().GetMethod("On", new Type[] { @event.GetType() });

                    if (handlerMethod == null)
                    {
                        throw new ArgumentNullException(nameof(handlerMethod), "Couldn't find event handler method");
                    }

                    // Moje izmjene:
                    // Pretvorio sam Consume metodu u Task i zatim dodao povratni tip za Invoke metode
                    // Ovo sam napravio zato što mi se podatak nije spremio, a Consumer je dobio info da je sve ok
                    // Nakon što sam popravio spremanje i dodao if isSuccess, dobios sam grešku za primary key violationa,
                    // ali Consumer više nije vraćao info da je taj podatak spremljen..
                    // Zato je u Repou dodana provjera i ako podatak postoji vratim da je sve OK pa Kafka pomakne offset
                    var result = handlerMethod.Invoke(_eventHandler, new object[] { @event });
                    bool isSuccess = false;

                    // If the result is a Task, await its completion
                    if (result is Task<bool> taskResult)
                    {
                        isSuccess = await taskResult;
                    }

                    // Šaljemo Kafki info da smo uspješno konzumirali event
                    // Commit metoda će inkrementalno uvećati log offset

                    if (isSuccess)
                        consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    // If topic doesn't exist...
                    Logger.Error(ex, "Consumer error");
                    Thread.Sleep(5000);
                }

            }
        }

        public async Task ResetTopicAsync(string topic)
        {
            // Moja skalamerija jer mi se nije spremilo u bazu i treba podesiti offset

            using var consumer = new ConsumerBuilder<string, string>(_config)
                    .SetKeyDeserializer(Deserializers.Utf8)
                    .SetValueDeserializer(Deserializers.Utf8)
                    .Build();
            // Manually resetting the offset to the beginning (for example)
            consumer.Seek(new TopicPartitionOffset(topic, 0, new Offset(0)));

            await Task.CompletedTask;
        }
    }
}
