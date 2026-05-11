using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class WebGLVisualFallback : MonoBehaviour
{
    [SerializeField] private Material playerMaterial;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private Material plasmaBlastMaterial;
    [SerializeField] private Material gemMaterial;
    [SerializeField] private Material rockMaterial;
    [SerializeField] private Sprite batSprite;
    [SerializeField] private Material batMaterial;
    [SerializeField] private Color batColor = Color.white;

    private readonly Dictionary<Entity, Renderer> _visuals = new();
    private readonly HashSet<Entity> _seenThisFrame = new();
    private readonly List<Entity> _removeBuffer = new();

    private EntityManager _entityManager;
    private EntityQuery _playerQuery;
    private EntityQuery _enemyQuery;
    private EntityQuery _plasmaBlastQuery;
    private EntityQuery _gemQuery;
    private EntityQuery _rockQuery;
    private EntityQuery _batOrbitQuery;
    private bool _initialized;

    private static bool ShouldUseFallback()
    {
        return Application.platform == RuntimePlatform.WebGLPlayer || !SystemInfo.supportsComputeShaders;
    }

    private void LateUpdate()
    {
        if (!ShouldUseFallback() || !TryInitialize())
            return;

        _seenThisFrame.Clear();

        SyncQuery(_playerQuery, playerMaterial, "Player Visual", 10, Vector3.one);
        SyncQuery(_enemyQuery, enemyMaterial, "Enemy Visual", 5, Vector3.one);
        SyncQuery(_plasmaBlastQuery, plasmaBlastMaterial, "Plasma Blast Visual", 20, Vector3.one);
        SyncQuery(_gemQuery, gemMaterial, "Gem Visual", 15, Vector3.one);
        SyncQuery(_rockQuery, rockMaterial, "Rock Visual", 2, new Vector3(1.5f, 1.5f, 1f));
        SyncBatOrbitQuery();

        CleanupStaleVisuals();
    }

    private bool TryInitialize()
    {
        if (_initialized)
            return _entityManager.World != null && _entityManager.World.IsCreated;

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
            return false;

        _entityManager = world.EntityManager;
        _playerQuery = CreateTransformQuery(typeof(PlayerTag));
        _enemyQuery = CreateTransformQuery(typeof(EnemyTag));
        _plasmaBlastQuery = CreateTransformQuery(typeof(PlasmaBlastData));
        _gemQuery = CreateTransformQuery(typeof(GemTag));
        _rockQuery = CreateTransformQuery(typeof(RockTag));
        _batOrbitQuery = CreateTransformQuery(typeof(BatOrbitData));
        _initialized = true;
        return true;
    }

    private EntityQuery CreateTransformQuery(System.Type tagType)
    {
        return _entityManager.CreateEntityQuery(
            ComponentType.ReadOnly(tagType),
            ComponentType.ReadOnly<LocalToWorld>());
    }

    private void SyncQuery(EntityQuery query, Material material, string visualName, int sortingOrder, Vector3 scaleMultiplier)
    {
        if (material == null || query.IsEmptyIgnoreFilter)
            return;

        using var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            if (!_entityManager.Exists(entity) || !_entityManager.HasComponent<LocalToWorld>(entity))
                continue;

            _seenThisFrame.Add(entity);

            var localToWorld = _entityManager.GetComponentData<LocalToWorld>(entity);
            var renderer = GetOrCreateVisual(entity, material, visualName, sortingOrder);
            var position = localToWorld.Position;
            var rotation = quaternion.identity;
            var scale = 1f;

            if (_entityManager.HasComponent<LocalTransform>(entity))
            {
                var localTransform = _entityManager.GetComponentData<LocalTransform>(entity);
                rotation = localTransform.Rotation;
                scale = localTransform.Scale;
            }

            renderer.transform.SetPositionAndRotation(
                new Vector3(position.x, position.y, position.z),
                new Quaternion(rotation.value.x, rotation.value.y, rotation.value.z, rotation.value.w));

            renderer.transform.localScale = scaleMultiplier * scale;
        }
    }

    private void SyncBatOrbitQuery()
    {
        if (batSprite == null || _batOrbitQuery.IsEmptyIgnoreFilter)
            return;

        using var entities = _batOrbitQuery.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            if (!_entityManager.Exists(entity) || !_entityManager.HasComponent<LocalToWorld>(entity))
                continue;

            _seenThisFrame.Add(entity);

            var localToWorld = _entityManager.GetComponentData<LocalToWorld>(entity);
            var renderer = GetOrCreateBatVisual(entity);
            var position = localToWorld.Position;

            renderer.transform.position = new Vector3(position.x, position.y, position.z);

            if (_entityManager.HasComponent<BatOrbitData>(entity))
            {
                var orbitData = _entityManager.GetComponentData<BatOrbitData>(entity);
                renderer.transform.rotation = Quaternion.Euler(0f, 0f, math.degrees(orbitData.CurrentAngle));
            }
        }
    }

    private Renderer GetOrCreateVisual(Entity entity, Material material, string visualName, int sortingOrder)
    {
        if (_visuals.TryGetValue(entity, out var renderer) && renderer != null)
        {
            if (renderer.sharedMaterial != material)
                renderer.sharedMaterial = material;

            return renderer;
        }

        var visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visual.name = visualName;
        visual.transform.SetParent(transform, false);

        var collider = visual.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        renderer = visual.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = sortingOrder;

        _visuals[entity] = renderer;
        return renderer;
    }

    private Renderer GetOrCreateBatVisual(Entity entity)
    {
        if (_visuals.TryGetValue(entity, out var renderer) && renderer != null)
            return renderer;

        var visual = new GameObject("Bat Visual");
        visual.transform.SetParent(transform, false);

        var spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = batSprite;
        spriteRenderer.sharedMaterial = batMaterial;
        spriteRenderer.color = batColor;
        spriteRenderer.sortingOrder = 25;

        _visuals[entity] = spriteRenderer;
        return spriteRenderer;
    }

    private void CleanupStaleVisuals()
    {
        _removeBuffer.Clear();

        foreach (var pair in _visuals)
        {
            if (_seenThisFrame.Contains(pair.Key) && _entityManager.Exists(pair.Key))
                continue;

            if (pair.Value != null)
                Destroy(pair.Value.gameObject);

            _removeBuffer.Add(pair.Key);
        }

        foreach (var entity in _removeBuffer)
            _visuals.Remove(entity);
    }

    private void OnDisable()
    {
        foreach (var renderer in _visuals.Values)
        {
            if (renderer != null)
                Destroy(renderer.gameObject);
        }

        _visuals.Clear();
        _seenThisFrame.Clear();
        _removeBuffer.Clear();
    }
}
