using Rose.Utilities;
using Rose.Core.Contracts.ApplicationServices.Common;
using Rose.Core.Contracts.ApplicationServices.Queries;

namespace Rose.Core.ApplicationServices.Queries;
public abstract class QueryHandler<TQuery, TData> : IQueryHandler<TQuery, TData>
    where TQuery : class, IQuery<TData>
{
    protected readonly RoseServices _roseApplicationContext;
    protected readonly QueryResult<TData> result = new QueryResult<TData>();

    protected virtual Task<QueryResult<TData>> ResultAsync(TData data, ApplicationServiceStatus status)
    {
        result._data = data;
        result.Status = status;
        return Task.FromResult(result);
    }

    protected virtual QueryResult<TData> Result(TData data, ApplicationServiceStatus status)
    {
        result._data = data;
        result.Status = status;
        return result;
    }


    protected virtual Task<QueryResult<TData>> ResultAsync(TData data)
    {
        var status = data != null ? ApplicationServiceStatus.Ok : ApplicationServiceStatus.NotFound;
        return ResultAsync(data, status);
    }

    protected virtual QueryResult<TData> Result(TData data)
    {
        var status = data != null ? ApplicationServiceStatus.Ok : ApplicationServiceStatus.NotFound;
        return Result(data, status);
    }

    public QueryHandler(RoseServices RoseApplicationContext)
    {
        _roseApplicationContext = RoseApplicationContext;
    }

    public abstract Task<QueryResult<TData>> Handle(TQuery request);
}
