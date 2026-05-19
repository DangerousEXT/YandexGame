using UnityEngine;

public static class MetaProgressionFormula
{
    public static int CalculatePrice(MetaUpgradeDefinition definition, int currentLevel)
    {
        if (definition == null)
            return 0;

        var price = Mathf.Max(0, definition.BasePrice);
        var safeMultiplier = Mathf.Max(0f, definition.GeometricMultiplier);

        for (int level = 0; level < Mathf.Max(0, currentLevel); level++)
        {
            price = Mathf.FloorToInt((price + definition.ArithmeticStep) * safeMultiplier);
        }

        return price;
    }
}
