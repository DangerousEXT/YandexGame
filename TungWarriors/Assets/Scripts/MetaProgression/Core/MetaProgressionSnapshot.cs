public struct MetaProgressionSnapshot
{
    public static MetaProgressionSnapshot Zero => new MetaProgressionSnapshot(0f, 0f, 0f);

    public float MaxHitPointsBonus;
    public float DamageBonus;
    public float MoveSpeedBonus;

    public MetaProgressionSnapshot(float maxHitPointsBonus, float damageBonus, float moveSpeedBonus)
    {
        MaxHitPointsBonus = maxHitPointsBonus;
        DamageBonus = damageBonus;
        MoveSpeedBonus = moveSpeedBonus;
    }
}
