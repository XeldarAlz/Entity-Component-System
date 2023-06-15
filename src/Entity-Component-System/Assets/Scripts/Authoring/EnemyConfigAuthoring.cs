using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// This class is used for authoring enemy configuration data in the editor
public class EnemyConfigAuthoring : MonoBehaviour
{
    public Material Material;
    public GameObject Prefab;
    public List<EnemyData> EnemiesData;
    
    // This nested class is responsible for baking the enemy configuration data into ECS components
    class EnemyDataBaker : Baker<EnemyConfigAuthoring>
    {
        public override void Bake(EnemyConfigAuthoring configAuthoring)
        {
         // Create a new EnemyConfigData instance and assign the values from the authoring data
            EnemyConfigData enemyConfigData = new EnemyConfigData();
            enemyConfigData.Material = configAuthoring.Material;
            enemyConfigData.EnemiesData = configAuthoring.EnemiesData;
            enemyConfigData.Entity = GetEntity(configAuthoring.Prefab);
            AddComponentObject(enemyConfigData);
        }
    }
}