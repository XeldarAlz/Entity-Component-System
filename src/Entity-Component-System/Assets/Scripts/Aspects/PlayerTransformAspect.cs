using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// This partial struct represents the aspect of the player's transform in the game
public readonly partial struct PlayerTransformAspect : IAspect, IComponentData
{
    public readonly Entity Self;
    public readonly RefRW<LocalTransform> Transform;    
    public readonly RefRW<PlayerData> PlayerData;

    public float3 Position
    {
        get => Transform.ValueRW.Position;
        set => Transform.ValueRW.Position = value;
    }
    
    public int Speed
    {           
        get => PlayerData.ValueRO.Speed;
        set => PlayerData.ValueRW.Speed = value;
    }
    
    // Move the player in a specified direction
    public void MoveTo(float3 direction)
    {
        Position += direction;
    }

    // Set the position of the player to a new position
    public void SetPositionTo(float3 newPosition)
    {
        Position = newPosition;
    }
}   