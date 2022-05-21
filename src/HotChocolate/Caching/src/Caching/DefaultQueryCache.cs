using HotChocolate.Execution;
using HotChocolate.Language;
using System.Threading.Tasks;

namespace HotChocolate.Caching;

public abstract class DefaultQueryCache : IQueryCache
{
    //public abstract Task<IQueryResult?> TryReadCachedQueryResultAsync(
    //    IRequestContext context, ICacheControlOptions options);

    public abstract Task CacheQueryResultAsync(IRequestContext context,
        ICacheControlResult result, ICacheControlOptions options);

    //public virtual bool ShouldReadResultFromCache(IRequestContext context)
    //    => true;

    public virtual bool ShouldCacheResult(IRequestContext context)
    {
        if (context.Result is not IQueryResult result)
        {
            // Result is a potentially deferred, we can not cache the entire query.
            return false;
        }

        // Operations other than query should not be cached.
        if (context.Operation is null
            || context.Operation.Definition.Operation != OperationType.Query)
        {
            return false;
        }

        if (result.Errors is { Count: > 0 })
        {
            // Result has unexpected errors, we do not want to cache it.

            return false;
        }

        return true;
    }
}