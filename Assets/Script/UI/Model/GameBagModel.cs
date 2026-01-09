using System.Collections.Generic;

public class GameBagModel : BaseModel
{
    /// <summary>
    /// 金币
    /// </summary>
    public const int GOLD = 1;

    /// <summary>
    /// 星星
    /// </summary>
    public const int Star = 2;
    /// <summary>
    /// 道具1（充气锤）
    /// </summary>
    public const int Prop1 = 3;
    /// <summary>
    /// 道具2（香槟）
    /// </summary>
    public const int Prop2 = 4;
    /// <summary>
    /// 道具3（调酒器）
    /// </summary>
    public const int Prop3 = 5;

    /// <summary>
    /// 物体数量
    /// </summary>
    public Dictionary<int,long> ItemInfos = new();
}
