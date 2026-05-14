using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using YG;

public class LocalizedText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private LocalizationCategories category;
    [SerializeField] private string key;
    
    private void Start()
    {
        UpdateText();
        YG2.onSwitchLang += _ => UpdateText();
    }

    private void UpdateText()
    {
        if (text != null)
        {
            text.text = LocalizationManager.Instance.Get(category, key);
        }
    }
}
