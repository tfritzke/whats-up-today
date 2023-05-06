using WhatsUpToday.Core.Types.Entity;
using WhatsUpToday.Core.Data.Interfaces;

namespace WhatsUpToday.Core.Data.Services.Entity;

/*
 * BaseService:
 * Its goal is to perform operations on a Set of Data.
 * 
 * This is a subclass of our EntityService.
 * 
 * The Base service class is derived from:
 * https://github.com/urfnet/URF.NET
 * https://www.codeproject.com/Articles/730191/Lightweight-Entity-Services-Library
 */

/// <summary>
/// BaseService for a data type (class) <typeparamref name="TEntity"/>
/// keyed by (struct) <typeparamref name="TId"/>.
/// 
/// This entity should be derived from EntityBase and be attached to a DB.
/// </summary>
public abstract class BaseService<TEntity, TId> : DataService<TEntity, TId>,
                                                  IBaseService<TEntity, TId>
                            where TEntity : EntityBase<TId>
                         // where TId : struct
{
    protected BaseService(IEFContextFactory factory) : base(factory)
    {
    }

    ~BaseService()
    {
    }
}
