using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// This script represents a system responsible for setting the screen borders data
[BurstCompile]
public partial struct ScreenBorderSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Set the screen borders data
        var screenBordersData = SetScreenBordersData();
        
        // Create an entity and add the ScreenBordersData component to it
        var em = state.EntityManager;
        var entity = em.CreateEntity();
        em.AddComponent<ScreenBordersData>(entity);
        em.SetComponentData(entity, screenBordersData);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
    }

    private ScreenBordersData SetScreenBordersData()
    {
        // Get the main camera and calculate the screen corners in world space
        var mainCamera = Camera.main;
        float3 topRight = mainCamera.ScreenToWorldPoint(new float3(Screen.width, Screen.height, 0));
        var topLeft = new float3(-topRight.x, topRight.y, topRight.z);
        float3 bottomLeft = mainCamera.ScreenToWorldPoint(new float3(0, 0, 0));
        var bottomRight = new float3(-bottomLeft.x, bottomLeft.y, bottomLeft.z);

        // Create and return the ScreenBordersData component
        var componentData = new ScreenBordersData
        {
            TopRight = topRight, TopLeft = topLeft, BottomRight = bottomRight, BottomLeft = bottomLeft
        };

        return componentData;
    }
}