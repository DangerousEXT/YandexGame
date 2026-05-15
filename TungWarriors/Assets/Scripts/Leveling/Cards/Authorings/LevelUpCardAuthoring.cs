using Unity.Entities;
using UnityEngine;

public class LevelUpCardAuthoring : MonoBehaviour
{
    [Header("Meta")]
    public string Title;
    [TextArea] public string Description;
    public Sprite Icon;

    [Header("Progression")]
    public string UpgradeId;
    [Min(1)] public int UpgradeLevel = 1;
    [Min(1)] public int MaxLevel = 1;
    [Min(1)] public int OfferWeight = 1;

    [Header("Requirement")]
    public string RequiredUpgradeId;
    [Min(0)] public int RequiredUpgradeLevel = 0;

    private class Baker : Baker<LevelUpCardAuthoring>
    {
        public override void Bake(LevelUpCardAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new LevelUpCardMeta
            {
                Title = authoring.Title,
                Description = authoring.Description,
                Icon = authoring.Icon
            });

            AddComponent(entity, new LevelUpCardUpgradeTrack
            {
                UpgradeId = authoring.UpgradeId,
                UpgradeLevel = Mathf.Max(1, authoring.UpgradeLevel),
                MaxLevel = Mathf.Max(1, authoring.MaxLevel),
                OfferWeight = Mathf.Max(1, authoring.OfferWeight)
            });

            AddComponent(entity, new LevelUpCardRequirement
            {
                UpgradeId = authoring.RequiredUpgradeId,
                RequiredLevel = Mathf.Max(0, authoring.RequiredUpgradeLevel)
            });
        }
    }
}
