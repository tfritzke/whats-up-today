namespace WhatsUpToday.Core.Types.Entity;

public interface IEntityWithTypedId<TId> : IEntity<TId>
{
    // inherited
    // TId Id { get; }
}
