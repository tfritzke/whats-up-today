using System.Threading;
using System.Threading.Tasks;
using WhatsUpToday.Core.Types.Entity;

namespace WhatsUpToday.Core.Data.Services.Entity;

public interface IBaseServiceWithAudit<TEntity, TId> : IBaseService<TEntity, TId>
                        where TEntity : EntityBase<TId>
                        where TId : struct
{
    // Context Audit
    new int SaveChanges();
    new Task<int> SaveChangesAsync();
    new Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
