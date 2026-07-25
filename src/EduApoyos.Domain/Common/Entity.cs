namespace EduApoyos.Domain.Common;

/// <summary>
/// Base class for every persisted aggregate/entity. The primary key is always a <see cref="Guid"/>
/// </summary>
public abstract class Entity
{
    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; }
}
