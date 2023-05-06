namespace WhatsUpToday.Core.Types.Entity;

public abstract class EntityBaseWithTypedId<TId> : EntityBase<TId>, 
                                                   IEntityWithTypedId<TId>
{
    // inherited
    // public virtual TId Id { get; protected set; }
}
