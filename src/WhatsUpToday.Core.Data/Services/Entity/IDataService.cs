using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using WhatsUpToday.Core.Types.Entity;

namespace WhatsUpToday.Core.Data.Services.Entity;

public interface IDataService<TEntity, TId> : IEntityService<TEntity, TId>
                        where TEntity : class, IEntity<TId>
                     // where TId : struct
{
    // [C]
    TEntity Insert(TEntity entity, bool check = false);
    void InsertRange(IEnumerable<TEntity> entities, bool check = false);

    // [R]
    IQueryable<TEntity> GetQueryable();
    IEnumerable<TEntity> GetEnumerable();
    int Count();
    Task<int> CountAsync();
    bool Contains(TId id);
    Task<bool> ContainsAsync(TId id);

    TEntity Find(TId Id);
    TEntity Find(params object[] keyValues);
    Task<TEntity> FindAsync(TId Id);
    Task<TEntity> FindAsync(object[] keyValues);
    Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken);

    // [U]
    void Update(TEntity entity, bool check = false);
    void UpdateRange(IEnumerable<TEntity> entities, bool check = false);

    // [D]
    void Delete(TEntity entity);

    // Context
    new int SaveChanges();
    new Task<int> SaveChangesAsync();
    new Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    // LINQ
    IQueryFluent<TEntity, TId> Query();
    IQueryFluent<TEntity, TId> Query(IQueryObject<TEntity> queryObject);
    IQueryFluent<TEntity, TId> Query(Expression<Func<TEntity, bool>> query);

    // SQL
    IQueryable<TEntity> SelectQuery(string query, params object[] parameters);
    Task<IEnumerable<TEntity>> SelectQueryAsync(string query, params object[] parameters);
    Task<IEnumerable<TEntity>> SelectQueryAsync(string query, CancellationToken cancellationToken, params object[] parameters);

    // Commands
    int ExecuteSqlCommand(string sql, params object[] parameters);
    Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);
    Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken, params object[] parameters);

    // Transactions
    int? CommandTimeout { get; set; }
    void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    bool Commit();
    void Rollback();
}
