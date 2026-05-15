using Unity.Entities;
using Unity.Mathematics;

public struct InitializeCharacterFlag : IComponentData, IEnableableComponent { }

public struct CharacterMoveDirection : IComponentData
{
    public float2 Value;
}

public struct CharacterMoveSpeed : IComponentData
{
    public float Value;
}

public struct CharacterMaxHitPoints : IComponentData
{
    public float Value;
}

public struct CharacterCurrentHitPoints : IComponentData
{
    public float Value;
}

public struct DamageThisFrame : IBufferElementData
{
    public int Value;
}
