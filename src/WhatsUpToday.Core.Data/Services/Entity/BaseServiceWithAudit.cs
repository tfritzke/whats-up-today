using System;
using System.Threading;
using System.Threading.Tasks;
using Audit.Core;
using WhatsUpToday.Core.Types.Entity;
using WhatsUpToday.Core.Data.Interfaces;

/*
 * The BaseServiceWithAudit service class is a subclass of the BaseService
 * class that provides entity auditing using the Audit.NET library.
 */

namespace WhatsUpToday.Core.Data.Services.Entity;

/// <summary>
/// BaseServiceWithAudit for a data type <typeparamref name="TEntity"/>
/// keyed by <typeparamref name="TId"/>.
/// </summary>
public abstract class BaseServiceWithAudit<TEntity, TId> :
                          BaseService<TEntity, TId>,
                          IBaseServiceWithAudit<TEntity, TId>
                                where TEntity : EntityBase<TId>
                                where TId : struct
    /*
     * The use of EntityBase may need to become BaseAuditedEntity,
     * depending on how Audit.net is designed.
     */
{
    protected BaseServiceWithAudit(IEFContextFactory factory) : base(factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));
    }

    ~BaseServiceWithAudit()
    {
        // nada
    }

    // Data

    // Audit


    /// <summary>
    /// Audit the addition of entities
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="check"></param>
    /// <returns>Entity</returns>
    new protected TEntity LocalAdd(TEntity entity, bool check = false)
    {
        string name = entity.GetType().ToString();

        // audit wrapper; event type, target object to track
        using (var scope = AuditScope.Create("{name}:Add", () => entity))
        {
            try
            {
                // do the operation
                entity = base.LocalAdd(entity);
            }
            catch (Exception ex)
            {
                // If an exception is thrown, discard the audit event
                scope.Discard();

                throw new Exception("See inner exception", ex);
            }
        }

        return entity;
    }


    /// <summary>
    /// Audit the update of entities
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="check"></param>
    new protected void LocalUpdate(TEntity entity, bool check = false)
    {
        string name = entity.GetType().ToString();

        // audit wrapper; event type, target object to track
        using (var scope = AuditScope.Create("{name}:Update", () => entity))
        {
            try
            {
                // do the operation
                base.LocalUpdate(entity, check);
            }
            catch (Exception ex)
            {
                // If an exception is thrown, discard the audit event
                scope.Discard();

                throw new Exception("See inner exception", ex);
            }
        }
    }


    /// <summary>
    /// Audit the removal of entities
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="check"></param>
    new protected void LocalRemove(TEntity entity, bool check = false)
    {
        string name = entity.GetType().ToString();

        // audit wrapper; event type, target object to track
        using (var scope = AuditScope.Create("{name}:Delete", () => entity))
        {
            try
            {
                // do the operation
                base.LocalRemove(entity, check);
            }
            catch (Exception ex)
            {
                // If an exception is thrown, discard the audit event
                scope.Discard();

                throw new Exception("See inner exception", ex);
            }
        }
    }


    /***************
    /* Context Audit
    /***************/

    /// <summary>
    /// Save any local data before the service is disposed.
    /// </summary>
    private void _SaveChanges()
    {
    }

    public override int SaveChanges()
    {
        _SaveChanges();

        int rc = base.SaveChanges();
        return rc;
    }

    public override async Task<int> SaveChangesAsync()
    {
        _SaveChanges();

        int rc = await base.SaveChangesAsync();
        return rc;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        _SaveChanges();

        int rc = await base.SaveChangesAsync(cancellationToken);
        return rc;
    }
}
