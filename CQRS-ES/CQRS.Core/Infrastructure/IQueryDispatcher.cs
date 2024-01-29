using CQRS.Core.Queries;

namespace CQRS.Core.Infrastructure
{
    public interface IQueryDispatcher<TEntity>
    {
        // Metode se mogu presložiti da imaju isti princip pozivanja, iako smo drugu presložili da koristi LSP
        void RegisterHandler<TQuery>(Func<TQuery, Task<List<TEntity>>> handler) where TQuery : BaseQuery;
        public void RegisterHandler(Func<BaseQuery, Task<List<TEntity>>> handler);
        Task<List<TEntity>> SendAsync(BaseQuery quey);
    }
}
