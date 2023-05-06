using System;

namespace WhatsUpToday.Core.Types.Entity;

/// <summary>
/// Abstract base class for an entity with a field "Id".
/// EF makes Id or <typename>Id a primary [Key] by convention.
/// </summary>
/// <typeparam name="TId">The entity ID type.</typeparam>
public abstract class EntityBase<TId> : ValidatableObject,
                                        IEntityWithTypedId<TId>
                         // where TId : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityBase{TId}" /> class.
    /// </summary>
    public EntityBase()
    {
        this.Id = default;
    }

    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    /// <value>The unique identifier.</value>
    // [Key] convention with EF5
    public virtual TId Id { get; set; } // protected set?
}

/// <summary>
/// Abstract base class for an entity with a field "Id" of type "long".
/// </summary>
public abstract class EntityBase : EntityBase<long> 
{
}

/// <summary>
/// Abstract base class for an entity with a field "Id" of type "int".
/// </summary>
public abstract class EntityBaseInt : EntityBase<int>
{
}

/// <summary>
/// Abstract base class for an entity with a field "Id" of type "long".
/// </summary>
public abstract class EntityBaseLong : EntityBase<long>
{
}

/// <summary>
/// Abstract base class for an entity with a field "Id" of type "Guid".
/// </summary>
public abstract class EntityBaseGuid : EntityBase<Guid>
{
}

/// <summary>
/// Abstract base class for an entity with a field "Id" of type "string".
/// </summary>
public abstract class EntityBaseString : EntityBase<string>
{
}
