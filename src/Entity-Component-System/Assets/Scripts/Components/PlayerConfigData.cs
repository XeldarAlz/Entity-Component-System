using Unity.Entities;
using Unity.Mathematics;

// This struct represents the configuration data for the player entity
public struct PlayerConfigData : IComponentData
{
    public Entity Prefab;
    public int Speed;
    public float3 MeshSize;
}