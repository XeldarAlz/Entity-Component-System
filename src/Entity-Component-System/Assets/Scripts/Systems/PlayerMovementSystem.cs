using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

// This script represents a system responsible for player movement
[BurstCompile]
[UpdateAfter(typeof(PlayerInputSystem))]
public partial struct PlayerMovementSystem : ISystem
{
    private SystemHandle _inputSystemHandle;
    private EntityQuery _gameOverEventQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ScreenBordersData>();
        state.RequireForUpdate<PlayerData>();
        _inputSystemHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<PlayerInputSystem>();
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

        // Get delta time, player input, and screen borders data
        var deltaTime = SystemAPI.Time.DeltaTime;
        var xAxis = SystemAPI.GetComponent<PlayerInputData>(_inputSystemHandle).XAxis;
        var screenBordersData = SystemAPI.GetSingleton<ScreenBordersData>();

        // Move the player left or right based on input
        foreach (var player in SystemAPI.Query<PlayerTransformAspect>())
        {
            // Calculate new position
            var direction = new float3(xAxis, 0, 0) * player.Speed * deltaTime;
            var newPosition = player.Position + direction;

            // Clamp position to screen bounds
            var halfMeshSize = player.PlayerData.ValueRW.MeshSize.x / 2;
            newPosition.x = math.clamp(newPosition.x, screenBordersData.BottomLeft.x + halfMeshSize, screenBordersData.BottomRight.x - halfMeshSize);

            // Set new position
            player.SetPositionTo(newPosition);
        }
    }
}