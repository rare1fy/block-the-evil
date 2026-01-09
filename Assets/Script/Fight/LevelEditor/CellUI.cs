using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace LevelEditor
{
    public class CellUI : MonoBehaviour
    {
        [HideInInspector] public int gridX, gridY;
        public Image colorImg;
        public Image itemIcon;
        public Image mask;

        [Header("颜色Icon")] public List<Sprite> colorSprites;
        [Header("效果Icon")] public List<Sprite> itemSprites;
        [HideInInspector] public GridCellData cellData;

        private LevelEditorManager editorManager;

        public void Initialize(int x, int y)
        {
            gridX = x;
            gridY = y;
            // 设置格子名称
            gameObject.name = $"Cell_{x}_{y}";

            cellData = new GridCellData(0, 0);
            colorImg.sprite = colorSprites[0];
            itemIcon.sprite = itemSprites[0];
            mask.gameObject.SetActive(false);
        }

        public void UpdateVisual(GridCellData data)
        {
            cellData = data;
            HideMask();
            UpdateBackgroundColor(data.color, true); // 更新背景颜色
            UpdateEffectIcon(data.item);       // 更新效果图标
        }

        public void UpdateBackgroundColor(int color, bool isId = false)
        {
            colorImg.sprite = color < colorSprites.Count ? colorSprites[color] : colorSprites[6];
            cellData.color = isId ? color : (int)EnumHelper.GetEnumValueByIndex<BlockColor>(color);

            if (color == 0)
            {
                itemIcon.sprite = itemSprites[0];
                cellData.item = 0;
            }
        }

        public void UpdateEffectIcon(int item)
        {
            if (cellData.bUsed > 0)
                return;

            if (cellData.color <= 0 && item != 0) // 没有颜色就不能添加效果
                return;

            itemIcon.sprite = itemSprites[item];
            cellData.item = item;
        }

        public void ShowTempMask(int length, int width, Sprite maskSprite)
        {
            mask.gameObject.SetActive(true);
            var l = 80 * length;
            var w = 80 * width;
            mask.rectTransform.sizeDelta = new Vector2(l, w);
            mask.color = new Color(1, 1, 1, 0.5f);
            mask.sprite = maskSprite;
        }

        public void ShowMask(int length, int width, Sprite maskSprite)
        {
            mask.gameObject.SetActive(true);
            var l = 80 * length;
            var w = 80 * width;
            mask.rectTransform.sizeDelta = new Vector2(l, w);
            mask.color = Color.white;
            mask.sprite = maskSprite;
        }

        public void HideMask()
        {
            mask.gameObject.SetActive(false);
        }
    }
}