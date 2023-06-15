using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// This struct represents the enemy configuration data as an ECS component
public class EnemyConfigData : IComponentData
{
    public Material Material;
    public List<EnemyData> EnemiesData;
    public Entity Entity;
}