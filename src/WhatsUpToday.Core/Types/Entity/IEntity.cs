namespace WhatsUpToday.Core.Types.Entity;

/// <summary>
/// Entity abstraction.
/// </summary>
/// <typeparam name="TId">The entity ID type.</typeparam>
public interface IEntity<TId> // where TId : struct
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    /// <value>The unique identifier.</value>
    TId Id { get; /*set;*/ }
}
