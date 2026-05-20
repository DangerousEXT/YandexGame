using Unity.Entities;
using UnityEngine;

public class MapAuthoring : MonoBehaviour
{
    public GameObject TilePrefab;
    [Tooltip("Лучше ставить нечетные числа")]
    public float TileSize;
    [Range(3, 51)]
    public int GridDimension; 
    public GameObject RockPrefab;
    public int RocksPerTile;

    private class Baker : Baker<MapAuthoring>
    {
        public override void Bake(MapAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var tileEntity = GetEntity(authoring.TilePrefab, TransformUsageFlags.Dynamic);
            var rockEntity = GetEntity(authoring.RockPrefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new MapSettings
            {
                TilePrefab = GetEntity(authoring.TilePrefab, TransformUsageFlags.Dynamic),
                TileSize = authoring.TileSize,
                GridDimension = authoring.GridDimension,
                RockPrefab = rockEntity,
                RocksPerTile = authoring.RocksPerTile
            });
            AddComponent<InitializeMapFlag>(entity);
            AddComponent<InitializeRocksFlag>(entity);
            SetComponentEnabled<InitializeMapFlag>(entity, true);
            SetComponentEnabled<InitializeRocksFlag>(entity, true);
        }
    }
}