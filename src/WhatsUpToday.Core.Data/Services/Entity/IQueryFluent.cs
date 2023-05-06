using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WhatsUpToday.Core.Types.Entity;

namespace WhatsUpToday.Core.Data.Services.Entity;

public interface IQueryFluent<TEntity, TId>
                        where TEntity : class, IEntity<TId>
                     // where TId : struct
{
    IQueryFluent<TEntity, TId> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy);
    IQueryFluent<TEntity, TId> Include(Expression<Func<TEntity, object>> expression);

    IEnumerable<TEntity> SelectPage(int page, int pageSize, out int totalCount);
    Task<IEnumerable<TEntity>> SelectPageAsync(int page, int pageSize);

    IEnumerable<TResult> Select<TResult>(Expression<Func<TEntity, TResult>> selector = null);
    IEnumerable<TEntity> Select();
    Task<IEnumerable<TEntity>> SelectAsync();

    IQueryable<TEntity> SqlQuery(string query, params object[] parameters);
}
