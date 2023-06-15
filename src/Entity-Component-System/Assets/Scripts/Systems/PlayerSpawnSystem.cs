using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

// This script represents a system responsible for spawning the player entity
[BurstCompile]
public partial struct PlayerSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Require the components for update
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PlayerConfigData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Get the player config data
        var playerConfigData = SystemAPI.GetSingleton<PlayerConfigData>();

        // Get the entity command buffer singleton
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        // Add the PlayerData component to the player prefab entity
        ecb.AddComponent<PlayerData>(playerConfigData.Prefab);

        // Set the player data values
        var playerData = new PlayerData { MeshSize = playerConfigData.MeshSize, Speed = playerConfigData.Speed };
        ecb.SetComponent(playerConfigData.Prefab, playerData);

        // Instantiate the player entity
        var playerEntity = ecb.Instantiate(playerConfigData.Prefab);

        // Add the PlayerTransformAspect component to the player entity
        ecb.AddComponent<PlayerTransformAspect>(playerEntity);

        // Disable the system
        state.Enabled = false;
    }
}