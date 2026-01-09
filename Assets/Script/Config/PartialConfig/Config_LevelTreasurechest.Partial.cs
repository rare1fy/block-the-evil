using Pb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static UnityEditor.Progress;

public partial class Config_LevelTreasurechest : ConfigBase
{
    private List<LevelTreasurechest> levelTreasurechests;
    private int weight;
    private Random _random;
    public LevelTreasurechest GetRandomItem()
    {
        if (levelTreasurechests == null)
        {
            levelTreasurechests = m_LevelTreasurechestDic.Values.ToList();
            levelTreasurechests.Sort((a, b) => a.Id - b.Id);
            weight = levelTreasurechests.Sum(x => x.Weight);
            _random = new Random();
        }

        int randomValue = _random.Next(weight);
        int currentWeight = 0;

        foreach (var item in levelTreasurechests)
        {
            currentWeight += item.Weight;
            if (randomValue < currentWeight)
                return item;
        }
        return levelTreasurechests.Last(); 
    }
}
