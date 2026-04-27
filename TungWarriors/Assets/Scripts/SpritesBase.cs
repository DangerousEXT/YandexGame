using System.Collections.Generic;
using UnityEngine;

public static class SpritesBase
{
    private static Dictionary<string, Sprite> idToSprite = new();
    private static Dictionary<Sprite, string> spriteToId = new();

    private static void Register(string id, Sprite sprite)
    {
        idToSprite[id] = sprite;
        spriteToId[sprite] = id;
    }

    public static Sprite GetSprite(string id)
    {
        idToSprite.TryGetValue(id, out var sprite);
        return sprite;
    }

    public static string GetId(Sprite sprite)
    {
        spriteToId.TryGetValue(sprite, out var id);
        return id;
    }

    public static void LoadAllIcons()
    {
        var sprites = Resources.LoadAll<Sprite>("Icons");
        foreach (var sprite in sprites)
        {
            Register(sprite.name, sprite);
        }
    }
}