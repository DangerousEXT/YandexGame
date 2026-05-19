using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class LanguageChanger : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languages;

    private Dictionary<int, string> indToLang = new()
    {
        {0, "ru" },
        {1, "en" },
        {2, "tr" }
    };
    private Dictionary<string, int> langToInd = new()
    {
        {"ru", 0 },
        {"en", 1 },
        {"tr", 2 }
    };

    private void Awake()
    {
        languages.value = langToInd[YG2.lang];
    }

    private void OnEnable()
    {
        languages.onValueChanged.AddListener(ChangeLanguage);
    }

    private void OnDisable()
    {
        languages.onValueChanged.RemoveListener(ChangeLanguage);
    }

    private void ChangeLanguage(int index)
    {
        YG2.SwitchLanguage(indToLang[index]);
    }
}