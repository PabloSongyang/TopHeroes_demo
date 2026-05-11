using UnityEngine;

public interface IEntity
{
    public enum Type
    {
        Player,
        Enemy,
    }

    GameObject RenderObject { get; }
    Type EntityType { get; }
}
