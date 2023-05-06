using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LinqKit.Core;
using WhatsUpToday.Core.Types.Entity;
using WhatsUpToday.Core.Data.Interfaces;

namespace WhatsUpToday.Core.Data.Services.Entity;

/*
 * DataService:
 * This is a subclass of our EntityService.
 */

/// <summary>
/// DataService for a data type (class) <typeparamref name="TEntity"/>
/// keyed by (struct) <typeparamref name="TId"/>.
/// 
/// This entity should not be attached to a DB, use BaseService for that.
/// </summary>
public abstract class DataService<TEntity, TId> : EntityService<TEntity, TId>,
                                                  IDataService<TEntity, TId>
                            where TEntity : class, IEntity<TId>
                         // where TId : struct
{
    protected DataService(IEFContextFactory factory) : base(factory)
    {
        if (Context != null)
        {
            Set = Context.Set<TEntity>();
        }
    }

    ~DataService()
    {
        Set = null;
    }

    // Data

    protected DbTransaction Transaction { get; private set; }

    // Methods

    // [C]

    /// <summary>
    /// Adds the specified entity.
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="check">True to check for matching object</param>
    /// <exception cref="ArgumentException">Thrown if an entity with the same id already exists.</exception>
    protected virtual TEntity LocalAdd(TEntity entity, bool check = false)
    {
        if (check)
        { 
            if (Contains(entity.Id))
                throw new ArgumentException($"{typeof(TEntity)} with id {entity.Id} already exists");
        }
        var tracked = Set.Add(entity);

        return entity;
    }

    public virtual TEntity Insert(TEntity entity, bool check = false)
    {
        var obj = LocalAdd(entity, check);
        return obj;
    }

    public virtual void InsertRange(IEnumerable<TEntity> entities, bool check = false)
    {
        foreach (var entity in entities)
        {
            var obj = LocalAdd(entity, check);
        }
    }


    // [R]

    public IQueryable<TEntity> GetQueryable()
    {
        return Set;
    }

    public IEnumerable<TEntity> GetEnumerable()
    {
        return Set.AsEnumerable();
    }

    public virtual int Count()
    {
        return Set.Count();
    }

    public virtual async Task<int> CountAsync()
    {
        return await Set.CountAsync();
    }

    public virtual bool Contains(TId id)
    {
        return null != Find(id);
    }

    public virtual async Task<bool> ContainsAsync(TId id)
    {
        return null != await FindAsync(id);
    }

    public virtual TEntity Find(TId id)
    {
        return Set.Find(id);
    }

    public virtual TEntity Find(params object[] keyValues)
    {
        return Set.Find(keyValues);
    }

    public virtual async Task<TEntity> FindAsync(TId id)
    {
        var obj = await Set.FindAsync((new object[] { id }));
        return obj;
    }

    public virtual async Task<TEntity> FindAsync(object[] keyValues)
    {
        var obj = await Set.FindAsync(keyValues, CancellationToken.None);
        return obj;
    }

    public virtual async Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
    {
        var obj =  await Set.FindAsync(keyValues, cancellationToken);
        return obj;
    }


    // [U]

    /// <summary>
    /// Updates the specified entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <exception cref="KeyNotFoundException">Thrown if entity with the given id is not found.</exception>
    protected TEntity LocalUpdate(TEntity entity, bool check = false)
    {
        if (check)
        {
            if (!Contains(entity.Id))
                throw new KeyNotFoundException($"{typeof(TEntity)} with id {entity.Id} was not found");
        }

        Context.Entry(entity).State = EntityState.Modified;

        return entity;
    }

    public virtual void Update(TEntity entity, bool check = false)
    {
        // EFCore: update thru context
        var obj = Context.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities, bool check = false)
    {
        // EFCore: update thru context
        Context.UpdateRange(entities);
    }


    // [D]

    /// <summary>
    /// Removes the entity with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <exception cref="KeyNotFoundException">Thrown if entity with the given id is not found.</exception>
    protected virtual void LocalRemove(TEntity entity, bool check = false)
    {
        TEntity obj = entity;

        if (check)
        {
            TEntity found = Set.SingleOrDefault(x => x.Id.Equals(entity.Id));

            if (obj == null)
                throw new KeyNotFoundException($"{typeof(TEntity)} with id {entity.Id} was not found");
        }

        if (obj != null)
        {
            Set.Remove(obj);
        }
    }

    public virtual void Delete(TEntity entity)
    {
        LocalRemove(entity);
    }

    public virtual void Delete(params object[] keyValues)
    {
        var entity = Set.Find(keyValues);
        LocalRemove(entity);
    }


    /*********
    /* Context 
    /*********/

    /* Note 1:
     * Invoking SaveChanges() on one service will affect all services that
     * share that context.
     * 
     * Note 2:
     * Each service's SaveChanges() should be called so that each can
     * save local data.
     */

    /// <summary>
    /// Save any local data before the service is disposed.
    /// </summary>
    private void LocalSaveChanges()
    {
    }

    public override int SaveChanges()
    {
        LocalSaveChanges();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync()
    {
        LocalSaveChanges();
        return base.SaveChangesAsync();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        LocalSaveChanges();
        return base.SaveChangesAsync(cancellationToken);
    }


    /***************
    /* LINQ Commands
    /***************/

    internal IQueryable<TEntity> Select(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        List<Expression<Func<TEntity, object>>> includes = null,
        int? page = null,
        int? pageSize = null)
    {
        IQueryable<TEntity> query = Set;

        // orderby is required if using paging so default to Id
        // in all cases
        orderBy ??= (x => x.OrderBy(obj => obj.Id));

        if (includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        if (filter != null)
        {
            query = query.AsExpandable().Where(filter);
        }
        if (page != null && pageSize != null)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }
        return query;
    }

    internal async Task<IEnumerable<TEntity>> SelectAsync(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        List<Expression<Func<TEntity, object>>> includes = null,
        int? page = null,
        int? pageSize = null)
    {
        return await Select(filter, orderBy, includes, page, pageSize).ToListAsync();
    }

    public IQueryFluent<TEntity, TId> Query()
    {
        return new QueryFluent<TEntity, TId>(this);
    }

    public virtual IQueryFluent<TEntity, TId> Query(IQueryObject<TEntity> queryObject)
    {
        return new QueryFluent<TEntity, TId>(this, queryObject);
    }

    public virtual IQueryFluent<TEntity, TId> Query(Expression<Func<TEntity, bool>> query)
    {
        return new QueryFluent<TEntity, TId>(this, query);
    }


    /**************
    /* SQL Commands
    /**************/

    public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
    {
        return Set.FromSqlRaw(query, parameters).AsQueryable();
    }

    public virtual async Task<IEnumerable<TEntity>> SelectQueryAsync(string query, params object[] parameters)
    {
        return await Set.FromSqlRaw(query, parameters).ToArrayAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> SelectQueryAsync(string query, CancellationToken cancellationToken, params object[] parameters)
    {
        return await Set.FromSqlRaw(query, parameters).ToArrayAsync(cancellationToken);
    }

    public int? CommandTimeout
    {
        get => Context.Database.GetCommandTimeout();
        set => Context.Database.SetCommandTimeout(value);
    }

    public virtual int ExecuteSqlCommand(string sql, params object[] parameters)
    {
        return Context.Database.ExecuteSqlRaw(sql, parameters);
    }

    public virtual async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
    {
        return await Context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    public virtual async Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken, params object[] parameters)
    {
        return await Context.Database.ExecuteSqlRawAsync(sql, cancellationToken, parameters);
    }

    public virtual void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        // objects from EF6 that don't exist in EFCore
        // var objectContext = ((IObjectContextAdapter)Context).ObjectContext;
        // if (objectContext.Connection.State != ConnectionState.Open)
        //    objectContext.Connection.Open();

        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            connection.Open();

        Transaction = connection.BeginTransaction(isolationLevel);
    }

    public virtual bool Commit()
    {
        Transaction.Commit();
        return true;
    }

    public virtual void Rollback()
    {
        Transaction.Rollback();
    }
}
