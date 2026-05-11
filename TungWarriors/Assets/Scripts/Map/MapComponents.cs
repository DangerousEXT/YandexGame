using Unity.Entities;

public struct MapTileTag : IComponentData { }

public struct RockTag : IComponentData { }

public struct MapSettings : IComponentData
{
    public Entity TilePrefab;
    public float TileSize;
    public int GridDimension;
    public Entity RockPrefab;
    public int RocksPerTile;
}

public struct InitializeMapFlag : IComponentData, IEnableableComponent { }

public struct InitializeRocksFlag : IComponentData, IEnableableComponent { }
