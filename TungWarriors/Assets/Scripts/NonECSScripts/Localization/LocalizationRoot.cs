using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class LocalizationRoot
{
    public List<LocalizationData> buttons = new();
    public List<LocalizationData> shop = new();
    public List<LocalizationData> equipment = new();
    public List<LocalizationData> buffs_description = new();
    public List<LocalizationData> inventory = new();
    public List<LocalizationData> meta_progression = new();
    public List<LocalizationData> settings = new();
    public List<LocalizationData> game = new();
}
