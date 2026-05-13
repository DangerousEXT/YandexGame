using Unity.Entities;
using Random = Unity.Mathematics.Random;

public struct EnemySpawnData : IComponentData
{
    public Entity EnemyPrefab;
    public float spawnInterval;
    public float spawnDistance;
}

public struct EnemySpawnState : IComponentData
{
    public float SpawnTimer;
    public int CurrentSpawnedEnemies;
    public int MaxSpawnedEnemies;
    public Random Random;
}

public struct EnemySpawnPoolState : IComponentData
{
    public bool IsInitialized;
}

public struct EnemyPoolOwner : IComponentData
{
    public Entity Spawner;
}

public struct EnemyPoolElement : IBufferElementData
{
    public Entity Value;
}
