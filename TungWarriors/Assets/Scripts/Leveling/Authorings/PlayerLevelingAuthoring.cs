using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[RequireComponent(typeof(PlayerAuthoring))]
[RequireComponent(typeof(CharacterAuthoring))]
public class PlayerLevelingAuthoring : MonoBehaviour
{
    public uint RandomSeed = 12345;

    private class Baker : Baker<PlayerLevelingAuthoring>
    {
        public override void Bake(PlayerLevelingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerLevel { Value = 1 });
            AddComponent(entity, new PlayerExperience
            {
                Current = 0,
                RequiredForNext = 10
            });
            AddComponent<InitializePlayerLevelingFlag>(entity);
            AddComponent<ShowLevelUpSelectionFlag>(entity);
            AddComponent(entity, new SelectedLevelUpCard { Value = Entity.Null });
            SetComponentEnabled<InitializePlayerLevelingFlag>(entity, true);
            SetComponentEnabled<ShowLevelUpSelectionFlag>(entity, false);
            SetComponentEnabled<SelectedLevelUpCard>(entity, false);
            var seed = authoring.RandomSeed == 0 ? 1u : authoring.RandomSeed;
            AddComponent(entity, new PlayerCardRandom
            {
                Value = Random.CreateFromIndex(seed)
            });
            AddBuffer<PlayerUpgradeProgressElement>(entity);
            AddBuffer<AvailableLevelUpCardElement>(entity);
            AddBuffer<OfferedLevelUpCardElement>(entity);
            AddComponent(entity, new CharacterMoveSpeedBonus { Value = 0f });
            AddComponent(entity, new PlayerDamageBonus { Value = 0 });
            AddComponent(entity, new CharacterDefense { Value = 0 });
            AddComponent(entity, new CharacterHealthRegen { ValuePerSecond = 0f, Accumulator = 0f });
            AddComponent(entity, new LastNonZeroMoveDirection { Value = new float2(1f, 0f) });
        }
    }
}