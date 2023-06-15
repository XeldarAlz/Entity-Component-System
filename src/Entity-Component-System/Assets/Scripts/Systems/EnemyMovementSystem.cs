using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// This system updates the movement of enemies with a fall component
[BurstCompile]
[UpdateAfter(typeof(EnemySpawnerSystem))]
public partial struct EnemyMovementSystem : ISystem
{
    private EntityQuery _fallComponentQuery;
    private EntityQuery _gameOverEventQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

        // Create entity queries for game over event and fall component
        _gameOverEventQuery = state.GetEntityQuery(typeof(GameOverEventData));
        _fallComponentQuery = state.GetEntityQuery(typeof(EnemyFallComponent));
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

        // Create command buffer and get delta time
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var deltaTime = SystemAPI.Time.DeltaTime;

        // Add fall component to enemy
        if (_fallComponentQuery.IsEmpty)
        {
            foreach (var (enemyRender, entity) in SystemAPI.Query<EnemyRenderData>().WithEntityAccess())
            {
                ecb.AddComponent(entity, new EnemyFallComponent());
                break;
            }
        }

        // Loop through enemies with fall component
        foreach (var (localToWorld, enemyCharacteristicData) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<EnemyCharacteristicData>>()
                     .WithAll<EnemyFallComponent>())
        {
            // Move enemy down
            localToWorld.ValueRW.Position += new float3(0, -deltaTime * enemyCharacteristicData.ValueRO.Speed, 0);
        }
    }
}