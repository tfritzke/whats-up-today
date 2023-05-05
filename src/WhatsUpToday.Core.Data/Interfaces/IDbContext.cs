using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WhatsUpToday.Core.Data.Interfaces;

public interface IDbContext
{
    /* DbContext */
    DbSet<TEntity>  Set<TEntity>() where TEntity : class;

    ChangeTracker   ChangeTracker { get; }
    DatabaseFacade  Database { get; }
    IModel          Model { get; }

    int             SaveChanges(bool acceptAllChangesOnSuccess = true);
    Task<int>       SaveChangesAsync(CancellationToken cancellationToken = default);
}
