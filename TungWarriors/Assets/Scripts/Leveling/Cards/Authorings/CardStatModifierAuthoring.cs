using System;
using Unity.Entities;
using UnityEngine;

public class CardStatModifierAuthoring : MonoBehaviour
{
    [Serializable]
    public struct ModifierEntry
    {
        public PlayerStatType Type;
        public float AddValue;
        public float MulValue;
    }

    public ModifierEntry[] Modifiers;

    private class Baker : Baker<CardStatModifierAuthoring>
    {
        public override void Bake(CardStatModifierAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<CardStatModifierEffectElement>(entity);

            if (authoring.Modifiers == null)
                return;

            for (int i = 0; i < authoring.Modifiers.Length; i++)
            {
                var modifier = authoring.Modifiers[i];
                buffer.Add(new CardStatModifierEffectElement
                {
                    Type = modifier.Type,
                    AddValue = modifier.AddValue,
                    MulValue = modifier.MulValue
                });
            }
        }
    }
}
