using WhatsUpToday.Core.Types.Entity;

namespace WhatsUpToday.Core.Data.Services.Entity;

public interface IBaseService<TEntity, TId> : IDataService<TEntity, TId>
                        where TEntity : EntityBase<TId>
                     // where TId : struct
{
}
