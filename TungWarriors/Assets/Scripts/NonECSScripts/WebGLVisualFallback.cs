using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class WebGLVisualFallback : MonoBehaviour
{
    private const int MaxInactiveQuadVisuals = 64;
    private const int MaxInactiveBatVisuals = 4;

    private enum VisualKind
    {
        Quad,
        Bat
    }

    private sealed class VisualInstance
    {
        public readonly Renderer Renderer;
        public readonly Transform Transform;
        public readonly VisualKind Kind;

        public float3 LastPosition;
        public quaternion LastRotation;
        public float LastScale;
        public Vector3 LastScaleMultiplier;
        public bool HasCachedTransform;

        public VisualInstance(Renderer renderer, VisualKind kind)
        {
            Renderer = renderer;
            Transform = renderer.transform;
            Kind = kind;
        }
    }

    [SerializeField] private Material playerMaterial;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private Material plasmaBlastMaterial;
    [SerializeField] private Material gemMaterial;
    [SerializeField] private Material rockMaterial;
    [SerializeField] private Sprite batSprite;
    [SerializeField] private Material batMaterial;
    [SerializeField] private Color batColor = Color.white;

    private readonly Dictionary<Entity, VisualInstance> _visuals = new(128);
    private readonly HashSet<Entity> _seenThisFrame = new();
    private readonly List<Entity> _removeBuffer = new(128);
    private readonly Stack<VisualInstance> _quadPool = new();
    private readonly Stack<VisualInstance> _batPool = new();

    private EntityManager _entityManager;
    private EntityQuery _playerQuery;
    private EntityQuery _enemyQuery;
    private EntityQuery _plasmaBlastQuery;
    private EntityQuery _gemQuery;
    private EntityQuery _rockQuery;
    private EntityQuery _batOrbitQuery;
    private bool _shouldUseFallback;
    private bool _initialized;

    private static bool ShouldUseFallback()
    {
        return Application.platform == RuntimePlatform.WebGLPlayer || !SystemInfo.supportsComputeShaders;
    }

    private void Awake()
    {
        _shouldUseFallback = ShouldUseFallback();
        enabled = _shouldUseFallback;
    }

    private void LateUpdate()
    {
        if (!_shouldUseFallback || !TryInitialize())
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
            _seenThisFrame.Add(entity);

            var localToWorld = _entityManager.GetComponentData<LocalToWorld>(entity);
            var visual = GetOrCreateQuadVisual(entity, material, visualName, sortingOrder);
            var position = localToWorld.Position;
            var rotation = quaternion.identity;
            var scale = 1f;

            if (_entityManager.HasComponent<LocalTransform>(entity))
            {
                var localTransform = _entityManager.GetComponentData<LocalTransform>(entity);
                rotation = localTransform.Rotation;
                scale = localTransform.Scale;
            }

            UpdateVisualTransform(visual, position, rotation, scale, scaleMultiplier);
        }
    }

    private void SyncBatOrbitQuery()
    {
        if (batSprite == null || _batOrbitQuery.IsEmptyIgnoreFilter)
            return;

        using var entities = _batOrbitQuery.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            _seenThisFrame.Add(entity);

            var localToWorld = _entityManager.GetComponentData<LocalToWorld>(entity);
            var visual = GetOrCreateBatVisual(entity);
            var position = localToWorld.Position;
            var orbitData = _entityManager.GetComponentData<BatOrbitData>(entity);
            var rotation = quaternion.RotateZ(orbitData.CurrentAngle);

            UpdateVisualTransform(visual, position, rotation, 1f, Vector3.one);
        }
    }

    private VisualInstance GetOrCreateQuadVisual(Entity entity, Material material, string visualName, int sortingOrder)
    {
        if (_visuals.TryGetValue(entity, out var visual) && visual != null && visual.Renderer != null)
        {
            ApplyQuadVisualSettings(visual, material, visualName, sortingOrder);
            return visual;
        }

        var createdVisual = AcquireQuadVisual();
        ApplyQuadVisualSettings(createdVisual, material, visualName, sortingOrder);
        _visuals[entity] = createdVisual;
        return createdVisual;
    }

    private static void ApplyQuadVisualSettings(VisualInstance visual, Material material, string visualName, int sortingOrder)
    {
        var renderer = visual.Renderer;
        if (renderer.sharedMaterial != material)
            renderer.sharedMaterial = material;

        if (renderer.sortingOrder != sortingOrder)
            renderer.sortingOrder = sortingOrder;

        if (renderer.gameObject.name != visualName)
            renderer.gameObject.name = visualName;
    }

    private VisualInstance AcquireQuadVisual()
    {
        while (_quadPool.Count > 0)
        {
            var pooledVisual = _quadPool.Pop();
            if (pooledVisual == null || pooledVisual.Renderer == null)
                continue;

            pooledVisual.Renderer.gameObject.SetActive(true);
            pooledVisual.Transform.SetParent(transform, false);
            pooledVisual.HasCachedTransform = false;
            return pooledVisual;
        }

        var visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visual.transform.SetParent(transform, false);

        var collider = visual.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        var renderer = visual.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return new VisualInstance(renderer, VisualKind.Quad);
    }

    private VisualInstance GetOrCreateBatVisual(Entity entity)
    {
        if (_visuals.TryGetValue(entity, out var visual) && visual != null && visual.Renderer != null)
        {
            ApplyBatVisualSettings(visual);
            return visual;
        }

        var createdVisual = AcquireBatVisual();
        ApplyBatVisualSettings(createdVisual);
        _visuals[entity] = createdVisual;
        return createdVisual;
    }

    private void ApplyBatVisualSettings(VisualInstance visual)
    {
        if (visual.Renderer is not SpriteRenderer spriteRenderer)
            return;

        if (spriteRenderer.sprite != batSprite)
            spriteRenderer.sprite = batSprite;

        if (spriteRenderer.sharedMaterial != batMaterial)
            spriteRenderer.sharedMaterial = batMaterial;

        if (spriteRenderer.color != batColor)
            spriteRenderer.color = batColor;

        if (spriteRenderer.sortingOrder != 25)
            spriteRenderer.sortingOrder = 25;

        if (spriteRenderer.gameObject.name != "Bat Visual")
            spriteRenderer.gameObject.name = "Bat Visual";
    }

    private VisualInstance AcquireBatVisual()
    {
        while (_batPool.Count > 0)
        {
            var pooledVisual = _batPool.Pop();
            if (pooledVisual == null || pooledVisual.Renderer == null)
                continue;

            pooledVisual.Renderer.gameObject.SetActive(true);
            pooledVisual.Transform.SetParent(transform, false);
            pooledVisual.HasCachedTransform = false;
            return pooledVisual;
        }

        var visual = new GameObject("Bat Visual");
        visual.transform.SetParent(transform, false);
        var spriteRenderer = visual.AddComponent<SpriteRenderer>();
        return new VisualInstance(spriteRenderer, VisualKind.Bat);
    }

    private void UpdateVisualTransform(VisualInstance visual, float3 position, quaternion rotation, float scale, Vector3 scaleMultiplier)
    {
        var targetScale = scaleMultiplier * scale;

        if (visual.HasCachedTransform &&
            math.all(visual.LastPosition == position) &&
            math.all(visual.LastRotation.value == rotation.value) &&
            Mathf.Approximately(visual.LastScale, scale) &&
            visual.LastScaleMultiplier == scaleMultiplier)
        {
            return;
        }

        visual.Transform.SetPositionAndRotation(
            new Vector3(position.x, position.y, position.z),
            new Quaternion(rotation.value.x, rotation.value.y, rotation.value.z, rotation.value.w));

        if (!visual.HasCachedTransform || visual.LastScaleMultiplier != scaleMultiplier || !Mathf.Approximately(visual.LastScale, scale))
            visual.Transform.localScale = targetScale;

        visual.LastPosition = position;
        visual.LastRotation = rotation;
        visual.LastScale = scale;
        visual.LastScaleMultiplier = scaleMultiplier;
        visual.HasCachedTransform = true;
    }

    private void CleanupStaleVisuals()
    {
        _removeBuffer.Clear();

        foreach (var pair in _visuals)
        {
            if (_seenThisFrame.Contains(pair.Key))
                continue;

            _removeBuffer.Add(pair.Key);
        }

        foreach (var entity in _removeBuffer)
        {
            if (_visuals.TryGetValue(entity, out var visual))
                RecycleVisual(visual);

            _visuals.Remove(entity);
        }
    }

    private void RecycleVisual(VisualInstance visual)
    {
        if (visual == null || visual.Renderer == null)
            return;

        visual.HasCachedTransform = false;
        var gameObject = visual.Renderer.gameObject;
        gameObject.SetActive(false);

        if (visual.Kind == VisualKind.Bat)
        {
            if (_batPool.Count < MaxInactiveBatVisuals)
            {
                _batPool.Push(visual);
                return;
            }
        }
        else
        {
            if (_quadPool.Count < MaxInactiveQuadVisuals)
            {
                _quadPool.Push(visual);
                return;
            }
        }

        Destroy(gameObject);
    }

    private void DestroyVisual(VisualInstance visual)
    {
        if (visual == null || visual.Renderer == null)
            return;

        Destroy(visual.Renderer.gameObject);
    }

    private void OnDisable()
    {
        foreach (var visual in _visuals.Values)
            DestroyVisual(visual);

        foreach (var pooledVisual in _quadPool)
            DestroyVisual(pooledVisual);

        foreach (var pooledVisual in _batPool)
            DestroyVisual(pooledVisual);

        _visuals.Clear();
        _seenThisFrame.Clear();
        _removeBuffer.Clear();
        _quadPool.Clear();
        _batPool.Clear();
    }
}
