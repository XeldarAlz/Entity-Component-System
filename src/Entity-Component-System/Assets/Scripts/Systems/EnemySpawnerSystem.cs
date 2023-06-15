using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

// This script represents a system responsible for spawning enemies in the game
[UpdateAfter(typeof(ScreenBorderSystem))]
public partial class EnemySpawnerSystem : SystemBase
{
    private Vector3[] _vertices;
    private EntityQuery _enemyRenderQuery;
    private EntityQuery _gameOverEventQuery;
    private EntityManager _entityManager;
    private uint _seed;

    // Define a SpawnJob struct that implements the IJobParallelFor interface
    [GenerateTestsForBurstCompatibility]
    public struct SpawnJob : IJobParallelFor
    {
        public Entity RenderMeshEntity;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public RenderBounds RenderBounds;
        public float3 TopLeftCorner;
        public float3 TopRightCorner;
        public uint Seed;
        [ReadOnly] public NativeArray<Enemy> EnemyCharacteristics;

        // Method called for each iteration of the job
        public void Execute(int index)
        {
            // Instantiate entity and set components
            var e = Ecb.Instantiate(index, RenderMeshEntity);
            Ecb.SetComponent(index, e, new EnemyRenderData());
            Ecb.SetComponent(index, e, MaterialMeshInfo.FromRenderMeshArrayIndices(index, 0));
            Ecb.SetComponent(index, e, new LocalTransform { Position = CalculateRandomPosition(index), Scale = 1 });
            Ecb.SetComponent(index, e,
                new EnemyCharacteristicData
                {
                    Speed = EnemyCharacteristics[index].Speed,
                    Damage = EnemyCharacteristics[index].Damage,
                    Reward = EnemyCharacteristics[index].Reward
                });
        }

        // Calculate random position for entity
        private float3 CalculateRandomPosition(int index)
        {
            // Calculate adjusted corners and random X position
            var adjustedUpperLeftCorner = TopLeftCorner.x + RenderBounds.Value.Size.x / 2;
            var adjustedUpperRightCorner = TopRightCorner.x - RenderBounds.Value.Size.x / 2;
            var rand = Random.CreateFromIndex(Seed + (uint)index);
            var randomXPos = rand.NextFloat(adjustedUpperLeftCorner, adjustedUpperRightCorner);

            // Return calculated position
            return new float3(randomXPos, TopLeftCorner.y + RenderBounds.Value.Size.y / 2, 0);
        }
    }

    protected override void OnCreate()
    {
        // Get entity manager and create queries
        var world = World.DefaultGameObjectInjectionWorld;
        _entityManager = world.EntityManager;
        var queryBuilder = new EntityQueryBuilder(Allocator.Temp);
        queryBuilder.WithAll<EnemyRenderData>();
        _enemyRenderQuery = GetEntityQuery(typeof(EnemyRenderData));
        _gameOverEventQuery = GetEntityQuery(typeof(GameOverEventData));
    }

    protected override void OnUpdate()
    {
        // Check if enemy render and game over event queries are empty
        Entities.WithoutBurst().WithStructuralChanges().ForEach((in EnemyConfigData enemyConfigData) =>
        {
            if (_enemyRenderQuery.IsEmpty && _gameOverEventQuery.IsEmpty)
            {
                // Add components for render and fill job data
                var tuple = AddComponentsForRender(enemyConfigData);
                FillJobData(tuple.renderMeshArray, tuple.entity, enemyConfigData);
            }
        }).Run();
    }

    private (RenderMeshArray renderMeshArray, Entity entity) AddComponentsForRender(EnemyConfigData enemyConfigData)
    {
        // Create render mesh description and mesh
        var desc = new RenderMeshDescription(ShadowCastingMode.Off);
        var mesh = CreateMesh(enemyConfigData.EnemiesData[0].Texture.width * 0.01f,
            enemyConfigData.EnemiesData[0].Texture.height * 0.01f);

        // Create materials array
        var materials = new Material[enemyConfigData.EnemiesData.Count];
        for (var i = 0; i < materials.Length; i++)
            materials[i] = new Material(enemyConfigData.Material)
            {
                mainTexture = enemyConfigData.EnemiesData[i].Texture
            };

        // Create render mesh array and entity
        var meshArray = new RenderMeshArray(materials, new[] { mesh });
        var renderMeshEntity = _entityManager.CreateEntity();
        var result = (renderMeshArray: meshArray, entity: enemyConfigData.Entity);

        // Add components to entity
        RenderMeshUtility.AddComponents(enemyConfigData.Entity, _entityManager, desc, meshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        return result;
    }

    private void FillJobData(RenderMeshArray renderMeshArray, Entity entity, EnemyConfigData enemyConfigData)
    {
        // Add components data to entity
        _entityManager.AddComponentData(entity, new EnemyRenderData());
        _entityManager.AddComponentData(entity, new LocalTransform());
        _entityManager.AddComponentData(entity, new EnemyConfigData());
        _entityManager.AddComponentData(entity, new EnemyCharacteristicData());

        // Create characteristics array from enemy data
        var characteristics = enemyConfigData.EnemiesData.Select(enemyData => new Enemy
        {
            Damage = enemyData.enemy.Damage, Speed = enemyData.enemy.Speed, Reward = enemyData.enemy.Reward
        }).ToArray();

        // Create native array from characteristics and entity command buffer
        var enemyCharacteristics = new NativeArray<Enemy>(characteristics, Allocator.Persistent);
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        // Get screen borders data and seed value
        var screenBordersData = SystemAPI.GetSingleton<ScreenBordersData>();
        _seed += Convert.ToUInt32(DateTime.UtcNow.Millisecond);

        // Create spawn job and schedule it
        var spawnJob = new SpawnJob
        {
            RenderMeshEntity = entity,
            Ecb = ecb.AsParallelWriter(),
            EnemyCharacteristics = enemyCharacteristics,
            RenderBounds = new RenderBounds { Value = renderMeshArray.Meshes[0].bounds.ToAABB() },
            TopLeftCorner = screenBordersData.TopLeft,
            TopRightCorner = screenBordersData.TopRight,
            Seed = _seed
        };
        var spawnHandle = spawnJob.Schedule(enemyConfigData.EnemiesData.Count, 128);
        spawnHandle.Complete();

        // Dispose of native array and entity command buffer
        enemyCharacteristics.Dispose();
        ecb.Playback(_entityManager);
        ecb.Dispose();
    }

    private Mesh CreateMesh(float w, float h)
    {
        // Create vertices, triangles and uv arrays
        _vertices = new Vector3[4];
        var uv = new Vector2[4];
        var triangles = new int[6];

        // Calculate half width and height
        var halfW = w / 3;
        var halfV = h / 3;

        // Set vertices values
        _vertices[0] = new Vector3(-halfW, -halfV);
        _vertices[1] = new Vector3(-halfW, halfV);
        _vertices[2] = new Vector3(halfW, halfV);
        _vertices[3] = new Vector3(halfW, -halfV);

        // Set uv values
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        // Set triangles values
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;
        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        // Create and return mesh
        var mesh = new Mesh { vertices = _vertices, uv = uv, triangles = triangles };
        return mesh;
    }
}