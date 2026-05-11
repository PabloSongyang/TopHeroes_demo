using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootLevelInfo
{
    public int NextLevel;
    public int AddDamage;
    public string SpineSkinName;
    public int AddHP;
    public List<string> PrefabLabelsList;

    public string GetPrefabLabel(int index = 0)
    {
        return PrefabLabelsList[index];
    }
}

[CreateAssetMenu(fileName = "(Loot) Loot Item So", menuName = "ScriptableObject/Loot/Loot Item So")]
public class LootItemSo : ScriptableObject
{
    public Type LootType => this.m_Type;
    public List<LootLevelInfo> LootLevelInfos => this.m_LootLevelInfos;
    public SoundInfo EatSound => this.m_EatSound;

    public enum Type
    {
        HP,
        Equip,
        Coin,
        Servant,
    }

    [SerializeField]
    private Type m_Type;

    [SerializeField]
    private List<LootLevelInfo> m_LootLevelInfos;

    [SerializeField]
    private SoundInfo m_EatSound;

    public LootLevelInfo GetLootLevelInfoByNextLevel(int currentPlayerLevel = 0)
    {
        if (currentPlayerLevel <= 0)
        {
            return this.m_LootLevelInfos[0];
        }
        return this.m_LootLevelInfos.Find(x => x.NextLevel == currentPlayerLevel + 1);
    }
}