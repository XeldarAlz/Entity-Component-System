using Unity.Entities;
using Unity.Mathematics;

// This struct represents data for the screen borders in the game
public struct ScreenBordersData : IComponentData
{
    public float3 TopLeft;
    public float3 TopRight;
    public float3 BottomLeft;
    public float3 BottomRight;
}