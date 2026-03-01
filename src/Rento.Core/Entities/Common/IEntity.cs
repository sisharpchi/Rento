namespace Rento.Core.Entities.Common;

/// <summary>
/// Marker interface for entities with a primary key of type <typeparamref name="TKey"/>.
/// </summary>
public interface IEntity<out TKey>
{
    TKey Id { get; }
}
