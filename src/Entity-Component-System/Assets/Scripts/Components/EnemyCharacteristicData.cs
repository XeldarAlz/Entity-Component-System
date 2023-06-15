using Unity.Entities;

// This struct represents the characteristics of an enemy entity
public struct EnemyCharacteristicData : IComponentData
{
    public float Speed;
    public int Damage;
    public int Reward;
}