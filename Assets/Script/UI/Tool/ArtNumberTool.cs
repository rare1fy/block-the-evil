using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArtNumberTool : MonoBehaviour
{
    [Header("数字图片设置")] [Tooltip("0-9的数字图片数组，索引0对应数字0，索引9对应数字9")]
    public Sprite[] numberSprites = new Sprite[10]; // 0-9的数字图片

    [Header("显示位置设置")] [Tooltip("用于显示数字的5个Image组件，从左到右排列")]
    public Image[] digitDisplays = new Image[6]; // 6个显示位置
    
    [Header("测试功能")]
    [Tooltip("输入要显示的数字(0-99999)")]
    public int testNumber = 12345;
    
    [Header("颜色设置")]
    public Color displayColor = Color.white;

    /// <summary>
    /// 显示指定数字
    /// </summary>
    public void DisplayNumber(int number)
    {
        // 确保数字在有效范围内
        if (number < 0 || number > 999999)
        {
            Debug.LogError("数字超出范围! 请输入0-999999之间的数字");
            return;
        }
        // 检查数字图片是否齐全
        if (numberSprites.Length < 10)
        {
            Debug.LogError("数字图片不齐全，需要0-9共10张图片");
            return;
        }
        
        var numberString = number.ToString();
        var digitCount = numberString.Length;
        
        for (int i = 0; i < 6; i++)
        {
            if (digitDisplays[i] != null)
            {
                digitDisplays[i].gameObject.SetActive(false);
            }
        }
        
        for (var i = 0; i < digitCount; i++)
        {
            var displayIndex = i;
            // 获取数字字符
            var digitChar = numberString[i];
            var digit = int.Parse(digitChar.ToString());
            
            // 设置对应的数字图片
            if (digitDisplays[displayIndex] != null && numberSprites[digit] != null)
            {
                digitDisplays[displayIndex].sprite = numberSprites[digit];
                digitDisplays[displayIndex].gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 清除所有显示
    /// </summary>
    public void ClearDisplay()
    {
        foreach (Image display in digitDisplays)
        {
            if (display != null)
            {
                display.enabled = false;
            }
        }
    }
    
    public void ApplyColor()
    {
        foreach (Image display in digitDisplays)
        {
            if (display != null)
            {
                display.color = displayColor;
            }
        }
    }
    
    private void OnValidate()
    {
        // 在编辑模式下实时应用颜色变化
        if (!Application.isPlaying)
        {
            ApplyColor();
        }
    }
}