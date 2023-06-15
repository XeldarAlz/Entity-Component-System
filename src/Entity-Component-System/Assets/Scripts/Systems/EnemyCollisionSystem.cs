using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// This system handles enemy collisions with the player
[BurstCompile]
[UpdateAfter(typeof(EnemyCheckOffScreenSystem))]
public partial struct EnemyCollisionSystem : ISystem
{
    private EntityQuery _gameOverEventQuery;
    private EntityQuery _playerQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

        // Create entity queries for game over event and player
        _gameOverEventQuery = state.GetEntityQuery(typeof(GameOverEventData));
        _playerQuery = state.GetEntityQuery(typeof(PlayerTransformAspect));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Check if game is over
        if (!_gameOverEventQuery.IsEmpty) return;

        // Create command buffer and get player position
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var playerPosition = GetPlayerPosition(state.EntityManager);

        // Loop through enemies
        foreach (var (localToWorld, enemyCharacteristicData, entity) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<EnemyCharacteristicData>>()
                     .WithEntityAccess())
        {
            // Check for collision with player
            if (math.distance(localToWorld.ValueRW.Position, playerPosition) < 1.5f)
            {
                // Enemy collided with player, call the delegate and destroy the enemy entity
                LevelController.Instance.EnemyCollidedWithPlayer(enemyCharacteristicData);
                ecb.DestroyEntity(entity);
            }
        }
    }

    // Get the position of the player entity
    private float3 GetPlayerPosition(EntityManager entityManager)
    {
        using (var playerEntities = _playerQuery.ToEntityArray(Allocator.TempJob))
        {
            if (playerEntities.Length > 0)
            {
                // Return the position of the first player entity found
                return entityManager.GetComponentData<LocalToWorld>(playerEntities[0]).Position;
            }
            else
            {
                // No player entity found, return zero position
                return float3.zero;
            }
        }
    }
}