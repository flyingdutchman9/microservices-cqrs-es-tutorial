using CQRS.Core.Infrastructure;
using CQRS.Core.Queries;
using Post.Query.Domain.Entities;

namespace Post.Query.Infrastructure.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher<PostEntity>
    {
        private readonly Dictionary<Type, Func<BaseQuery, Task<List<PostEntity>>>> _handlers = new();

        public void RegisterHandler<TQuery>(Func<TQuery, Task<List<PostEntity>>> handler) where TQuery : BaseQuery
        {
            if (_handlers.ContainsKey(typeof(TQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register same Query handler twice");
            }

            _handlers.Add(typeof(TQuery), x => handler((TQuery)x));
        }

        public void RegisterHandler(Func<BaseQuery, Task<List<PostEntity>>> handler)
        {
            if (_handlers.ContainsKey(typeof(BaseQuery)))
            {
                throw new IndexOutOfRangeException("You cannot register same Query handler twice");
            }

            _handlers.Add(typeof(BaseQuery), x => handler(x));
        }

        public async Task<List<PostEntity>> SendAsync(BaseQuery query)
        {
            if (_handlers.TryGetValue(query.GetType(), out Func<BaseQuery, Task<List<PostEntity>>> handler))
            {
                return await handler(query);
            }

            throw new ArgumentNullException(nameof(handler), "No query handler was registered");
        }
    }
}
