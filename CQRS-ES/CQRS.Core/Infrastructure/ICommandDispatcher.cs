using CQRS.Core.Commands;

namespace CQRS.Core.Infrastructure
{
    public interface ICommandDispatcher
    {
        // Registracija CommandHandler metode (Ima Func delegate parametar (koji
        // može imati n ulaznih parametara, ali zadnji je uvijek out parametar)
        // Ulazni parametar je T, a izlazni tipa Task. T mora naslijediti BaseCommand
        void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand;

        // Slanje (dispatch) command objekta
        // LSP princip? Konkretna implementacija bi trebala biti zamjenjiva baznom klasom bez utjecaja na funkcionalnost
        Task SendAsync(BaseCommand command);
    }
}
