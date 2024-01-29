using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Handlers
{
    // Potrebno je implementirati IEventSourcingHandler, ali s konkretnim tipom: PostAggregate
    public class EventSourcingHandler : IEventSourcingHandler<PostAggregate>
    {
        private readonly IEventStore _eventStore;
        // eventProducer koristimo samo za Restore read baze
        private readonly IEventProducer _eventProducer;

        public EventSourcingHandler(IEventStore eventStore, IEventProducer eventProducer)
        {
            _eventStore = eventStore;
            _eventProducer = eventProducer;
        }

        public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
        {
            var aggregate = new PostAggregate();
            var events = await _eventStore.GetEventsAsync(aggregateId);

            // Ako nismo pronašli evente, vraćamo novu instancu aggregatea koji još nema dodijeljene evente
            if (events == null || !events.Any()) { return aggregate; }

            // Ako imamo evente dodijeljene aggregateu, pozivamo Replay da bismo re-kreirali posljednje stanje aggregatea
            aggregate.ReplayEvent(events);

            // Postavljamo posljednju verziju
            aggregate.Version = events.Select(e => e.Version).Max();

            // Vraćamo aggregate koji je uspješno replayan ili restorean iz event storea
            return aggregate;
        }

        public async Task SaveAsync(AggregateRoot aggregate)
        {
            await _eventStore.SaveEventsAsync(aggregate.Id, aggregate.UncommitedChanges(), aggregate.Version);
            // Brišemo evente jer su sada commitani.
            aggregate.MarkChangesAsCommited();
        }

        // Za svaki aggregateId dohvaćamo sve njegove evente i reproduciramo ih
        public async Task RepublishEventsAsync()
        {
            var aggregateIds = await _eventStore.GetAggregateIdsAsync();
            if (aggregateIds == null || !aggregateIds.Any()) return;
            string topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");

            foreach (var aggregateId in aggregateIds)
            {
                var aggregate = await GetByIdAsync(aggregateId);
                if (aggregate == null || !aggregate.Active) continue;

                var events = await _eventStore.GetEventsAsync(aggregateId);

                foreach (var e in events)
                {
                    await _eventProducer.ProduceAsync(topic, e);
                }
            }
        }
    }
}
