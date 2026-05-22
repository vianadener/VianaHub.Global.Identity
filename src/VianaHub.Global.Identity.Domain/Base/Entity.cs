namespace VianaHub.Global.Identity.Domain.Base;

/// <summary>
/// Represents a base class for entities with identity, audit information, and value-based equality.
/// </summary>
/// <remarks>Equality is determined by the entity's type and identifier. Provides audit properties for tracking
/// creation and modification metadata.</remarks>
public class Entity : IEquatable<Entity>
{
    public int Id { get; set; }
    public int AddedBy { get; set; }
    public DateTime AddedOn { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    protected Entity()
    {
        AddedOn = DateTime.UtcNow;
    }

    protected Entity(int id)
    {
        Id = id;
        AddedOn = DateTime.UtcNow;
    }


    // Equality baseada no Id
    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity left, Entity right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }

    // Método para verificar se a entidade é transiente (ainda não foi persistida)
    public bool IsTransient()
    {
        return Id == 0;
    }
}
