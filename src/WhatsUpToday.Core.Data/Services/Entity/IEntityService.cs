using System;
using System.Threading;
using System.Threading.Tasks;
using WhatsUpToday.Core.Types.Entity;

namespace WhatsUpToday.Core.Data.Services.Entity;

public interface IEntityService<TEntity, TId> : IDisposable
                        where TEntity : class, IEntity<TId>
                     // where TId : struct
{
    int MaxOrdinal { get; set; }

    TEntity CreateObject();

    int SaveChanges();
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
