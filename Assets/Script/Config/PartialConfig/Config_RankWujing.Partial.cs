using Pb;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Config_RankWujing : ConfigBase
{
    List<RankWujing> _ranks;
    public string GetRankeString(int score,int size = 36)
    {
        if (_ranks == null)
        {
            _ranks = m_RankWujingDic.Values.ToList();
            _ranks.Sort((a, b) =>
            {
                return a.Id - b.Id;
            });
        }

        RankWujing tmpRank = new();

        foreach (var rank in _ranks)
        {
            if (rank.Score <= score)
            {
                tmpRank = rank;
            }
            else
            {
                return $"已击败<size={size}><color=#ffd249>{tmpRank.Rank}</color></size>%玩家";
            }
        }
        return $"已击败<size={size}><color=#ffd249>{tmpRank.Rank}</color></size>%玩家";
    }
}
