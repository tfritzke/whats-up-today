using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WhatsUpToday.Core.Types.Entity;

namespace WhatsUpToday.Core.Data.Services.Entity;

public sealed class QueryFluent<TEntity, TId> : IQueryFluent<TEntity, TId>
                        where TEntity : class, IEntity<TId>
                     // where TId : struct
{
    // data
    private readonly DataService<TEntity, TId> _service;

    private readonly Expression<Func<TEntity, bool>> _expression;
    private readonly List<Expression<Func<TEntity, object>>> _includes;
    private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderBy;

    // constuctors

    public QueryFluent(DataService<TEntity, TId> service)
    {
        _service = service;
        _includes = new List<Expression<Func<TEntity, object>>>();
    }

    public QueryFluent(DataService<TEntity, TId> service, IQueryObject<TEntity> queryObject) 
        : this(service)
    {
        _expression = queryObject.Query();
    }

    public QueryFluent(DataService<TEntity, TId> service, Expression<Func<TEntity, bool>> expression)
        : this(service)
    {
        _expression = expression;
    }

    // methods

    public IQueryFluent<TEntity, TId> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
    {
        _orderBy = orderBy;
        return this;
    }

    public IQueryFluent<TEntity, TId> Include(Expression<Func<TEntity, object>> expression)
    {
        _includes.Add(expression);
        return this;
    }

    public IEnumerable<TEntity> SelectPage(int page, int pageSize, out int totalCount)
    {
        totalCount = _service.Select(_expression).Count();
        return _service.Select(_expression, _orderBy, _includes, page, pageSize);
    }

    public async Task<IEnumerable<TEntity>> SelectPageAsync(int page, int pageSize)
    {
        return await _service.SelectAsync(_expression, _orderBy, _includes, page, pageSize);
    }

    public IEnumerable<TEntity> Select()
    {
        return _service.Select(_expression, _orderBy, _includes);
    }

    public IEnumerable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector)
    {
        return _service.Select(_expression, _orderBy, _includes).Select(selector);
    }

    public async Task<IEnumerable<TEntity>> SelectAsync()
    {
        return await _service.SelectAsync(_expression, _orderBy, _includes);
    }

    public IQueryable<TEntity> SqlQuery(string query, params object[] parameters)
    {
        return _service.SelectQuery(query, parameters).AsQueryable();
    }
}
