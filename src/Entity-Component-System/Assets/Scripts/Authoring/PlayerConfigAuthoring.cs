using Unity.Entities;
using UnityEngine;

// This class is used for authoring player configuration data in the editor
public class PlayerConfigAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int Speed;
    public MeshRenderer MeshRenderer;
    
    // This nested class is responsible for baking the player configuration data into ECS components
    class PlayerDataBaker : Baker<PlayerConfigAuthoring>
    {
        public override void Bake(PlayerConfigAuthoring configAuthoring)
        {
            // Add a PlayerConfigData component with the appropriate values
            AddComponent(new PlayerConfigData
            {
                Prefab = GetEntity(configAuthoring.Prefab),
                Speed = configAuthoring.Speed,
                MeshSize = configAuthoring.MeshRenderer.bounds.size 
            });
        }
    }
}

