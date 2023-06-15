using Unity.Burst;
using Unity.Entities;
using UnityEngine;

// This script represents a system responsible for capturing player input
[BurstCompile]
public partial struct PlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.AddComponent<PlayerInputData>(state.SystemHandle);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Reset player input
        var xAxis = 0f;

        // Get the horizontal input axis
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            xAxis = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            xAxis = 1;
        }

        // Create and set the PlayerInputData component
        var playerInputData = new PlayerInputData { XAxis = xAxis };
        SystemAPI.SetComponent(state.SystemHandle, playerInputData);
    }
}