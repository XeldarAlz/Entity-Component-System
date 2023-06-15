using Unity.Entities;
using Unity.Mathematics;

// This struct represents the data for the player entity
public struct PlayerData : IComponentData
{
    public int Speed;
    public float3 MeshSize;
}