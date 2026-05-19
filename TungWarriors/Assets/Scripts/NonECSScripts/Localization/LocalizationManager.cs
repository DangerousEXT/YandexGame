using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YG;

class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private Dictionary<LocalizationCategories, Dictionary<string, LocalizationData>> categories = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLocalization();
    }

    private void LoadLocalization()
    {
        var jsonFile = Resources.Load<TextAsset>("Localization/localization");
        if (jsonFile == null)
        {
            Debug.LogError("Localization file not found!");
            return;
        }

        var root = JsonUtility.FromJson<LocalizationRoot>(jsonFile.text);

        AddCategory(LocalizationCategories.shop, root.shop);
        AddCategory(LocalizationCategories.buttons, root.buttons);
        AddCategory(LocalizationCategories.equipment, root.equipment);
        AddCategory(LocalizationCategories.buffs_description, root.buffs_description);
        AddCategory(LocalizationCategories.inventory, root.inventory);
        AddCategory(LocalizationCategories.meta_progression, root.meta_progression);
        AddCategory(LocalizationCategories.settings, root.settings);
    }

    private void AddCategory(LocalizationCategories category, List<LocalizationData> entries)
    {
        var dict = new Dictionary<string, LocalizationData>();
        if (entries == null)
        {
            categories[category] = dict;
            return;
        }

        foreach (var entry in entries)
        {
            dict[entry.id] = entry;
        }
        categories[category] = dict;
    }

    public string Get(LocalizationCategories category, string id)
    {
        if (!categories.TryGetValue(category, out var categoryDict))
        {
            Debug.LogWarning($"Category not found: {category}");
            return id;
        }

        if (!categoryDict.TryGetValue(id, out var data))
        {
            Debug.LogWarning($"Key not found: {category}/{id}");
            return id;
        }
        
        return data.GetLocalizedText();
    }
}
