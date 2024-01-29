using CQRS.Core.Events;

namespace CQRS.Core.Domain
{
    public abstract class AggregateRoot
    {
        private readonly List<BaseEvent> _changes = new();

        protected Guid _id;
        public Guid Id
        {
            get { return _id; }
        }

        // Aggregate po defaultu nema sluzbenu verziju pa mu stavljamo -1 (verzija je 0-based)
        public int Version { get; set; } = -1;
        public Func<BaseEvent, Task> ApplyFunc { get; protected set; }

        public IEnumerable<BaseEvent> UncommitedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommited()
        {
            _changes.Clear();
        }

        protected void RaiseEvent(BaseEvent @event)
        {
            ApplyChange(@event, true);
        }

        // Metoda radi replay svih evenata iz event storea koji su prethodno dodani
        // kako bi se re-kreiralo posljednje stanje prije nego li se primjene nove (uncommited) izmjene
        public void ReplayEvent(IEnumerable<BaseEvent> events)
        {
            foreach (var e in events)
            {
                ApplyChange(e, false);
            }
        }

        // Tražimo metodu Apply s parametrom tipa BaseEvent
        private void ApplyChange(BaseEvent @event, bool isNew) //isNew -> je li event novi ili je dohvaćen iz event storea
        {
            var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method), $"Apply method was not found in the aggregate for {@event.GetType().Name}");
            }

            method.Invoke(this, new object[] { @event });

            // Ako je novi event dodajemo ga u listu uncommited izmjena
            if (isNew)
            {
                _changes.Add(@event);
            }
        }

        
    }
}
