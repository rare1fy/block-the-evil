using System;
using System.Linq;
using System.Reflection;
using LevelEditor;

namespace LevelEditor
{
// 颜色类型枚举
    public enum BlockColor
    {
        空白 = 0,
        红色 = 1,
        蓝色 = 2,
        绿色 = 3,
        黄色 = 4,
        青色 = 5,
        果汁潘趣酒 = 6,
        披萨切片 = 7,
        野格酒 = 8,
        薯条篮 = 9,
        派对王冠​ = 10,
        朗姆酒 = 11,
        拍立得​ = 12,
        经典可乐 = 13,
        骰子​ = 14,
        柠檬莫吉托 = 15,
        玩具手铐 = 16,
        龙舌兰shots = 17,
        充电宝 = 18,
        茅台 = 19,
        吓人鬼面 = 20,
        鸡尾酒飓风 = 21,
        杜蕾斯 = 22,
        苏打水​ = 23,
        纸杯= 24,
        麦克风​ = 25,
        一杯威士忌 = 26,
        尤克里里 = 27,
        香槟 = 28,
        啤酒桶 = 29,
        伏特加 = 30,
        效果 = 999
    }

// 物品类型枚举
    public enum BlockItemEnum
    {
        None = 0,
        一次锁 = 1,
        两次锁 = 2,
        三次锁 = 3,
        冰桶 = 4,
        生成物品 = 5,
        炸弹 = 6,
        宝箱 = 7
    }

    public enum MaskType
    {
        金币 = 0,
        广告 = 1,
    }
}
public static class EnumHelper
{
    public static T GetEnumValueByIndex<T>(int index) where T : Enum
    {
        // 获取枚举类型的所有字段（公共静态字段）
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);

        // 按字段的声明顺序（MetadataToken）排序
        var orderedFields = fields.OrderBy(f => f.MetadataToken).ToArray();

        if (index < 0 || index >= orderedFields.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "索引超出枚举值的范围。");

        // 获取字段对应的枚举值
        return (T)orderedFields[index].GetValue(null);
    }
}

