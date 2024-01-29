using CQRS.Core.Commands;
using CQRS.Core.Infrastructure;

namespace Post.Cmd.Infrastructure.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        // Dictionary koji će čuvati sve naše Command handler metode (kao Function delegate)
        // Type: tip Command handlera (ekvivalent Command tipu)
        // Func delegate s ulaznim parametrom BaseCommand i izlaznim parametrom tipa Task
        private readonly Dictionary<Type, Func<BaseCommand, Task>> _handlers = new();

        public void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand
        {
            if (_handlers.ContainsKey(typeof(T)))
            {
                throw new IndexOutOfRangeException("You cannot register same command handler twice!");
            }

            // T: concrete command object Type
            // Kao vrijednost šaljemo handler, ali radimo cast iz baznog tipa (x) u konkretni object tip (T)
            _handlers.Add(typeof(T), x => handler((T)x));
        }

        public async Task SendAsync(BaseCommand command)
        {
            if (_handlers.TryGetValue(command.GetType(), out Func<BaseCommand, Task> handler))
            {
                await handler(command);
            }
            else
            {
                throw new ArgumentNullException(nameof(handler), "No command handler was registered!");
            }
        }
    }
}
