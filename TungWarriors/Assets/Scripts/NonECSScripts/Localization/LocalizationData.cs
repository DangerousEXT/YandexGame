using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YG;

[Serializable]
public class LocalizationData
{
    public string id;

    public string ru;
    public string en;
    public string tr;

    public string GetLocalizedText(string languageCode)
    {
        return languageCode switch
        {
            "ru" => ru,
            "en" => en,
            "tr" => tr,
            _ => en
        };
    }

    public string GetLocalizedText()
    {
        return YG2.lang switch
        {
            "ru" => ru,
            "en" => en,
            "tr" => tr,
            _ => en
        };
    }
}