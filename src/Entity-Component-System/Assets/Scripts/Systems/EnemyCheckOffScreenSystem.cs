using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

// This system checks if enemies have gone off-screen and destroys them
[BurstCompile]
[UpdateAfter(typeof(EnemyMovementSystem))]
public partial struct EnemyCheckOffScreenSystem : ISystem
{
    private EntityQuery _gameOverEventQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ScreenBordersData>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

        // Create entity query for game over event
        _gameOverEventQuery = state.GetEntityQuery(typeof(GameOverEventData));
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

        // Create command buffer
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Loop through enemies
        foreach (var (localToWorld, enemyCharacteristicData, renderBounds, entity) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<EnemyCharacteristicData>, RefRO<RenderBounds>>()
                     .WithEntityAccess())
        {
            // Check if enemy is off screen and destroy it
            if (localToWorld.ValueRW.Position.y < -SystemAPI.GetSingleton<ScreenBordersData>().TopLeft.y -
                renderBounds.ValueRO.Value.Size.y)
            {
                // Enemy is off-screen, call the delegate for out of bounds and destroy the entity
                LevelController.Instance.EnemyOutOfBoundsDelegate(enemyCharacteristicData);
                ecb.DestroyEntity(entity);
            }
        }
    }
}