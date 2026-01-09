using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace LevelEditor
{
    public class LevelEditorManager : MonoBehaviour
    {
        [Header("Grid Settings")] public int gridSize = 8;

        [Header("UI References")] public Transform gridPanel;
        public GameObject cellPrefab;
        public Dropdown colorDropdown;
        public Dropdown itemDropdown;
        public GraphicRaycaster raycaster;

        public Button colorChangeBtn;
        public Button itemChangeBtn;

        private EventSystem eventSystem;
        private PointerEventData pointerData;
        private List<RaycastResult> _results = new List<RaycastResult>();
        private List<CellUI> cellUIs = new List<CellUI>();
        public List<CellUI> CellUIs => cellUIs;

        private int selectedX = -1;
        private int selectedY = -1;

        void Start()
        {
            CreateGridUI();
            SetupDropdowns();

            eventSystem = EventSystem.current;
            pointerData = new PointerEventData(eventSystem);

            if (colorChangeBtn != null)
                colorChangeBtn.onClick.AddListener(OnClickColorChange);

            if (itemChangeBtn != null)
                itemChangeBtn.onClick.AddListener(OnClickItemChange);
        }

        void CreateGridUI()
        {
            // 清除现有UI
            foreach (Transform child in gridPanel)
            {
                Destroy(child.gameObject);
            }

            cellUIs.Clear();

            // 创建所有格子UI
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    GameObject cellObj = Instantiate(cellPrefab, gridPanel);
                    CellUI cellUI = cellObj.GetComponent<CellUI>();

                    if (cellUI != null)
                    {
                        cellUI.Initialize(x, y);
                        cellUIs.Add(cellUI);
                    }
                }
            }
        }

        void SetupDropdowns()
        {
            // 清空现有选项
            colorDropdown.ClearOptions();
            itemDropdown.ClearOptions();

            // 添加颜色选项
            List<Dropdown.OptionData> colorOptions = new List<Dropdown.OptionData>();
            foreach (BlockColor color in System.Enum.GetValues(typeof(BlockColor)))
            {
                colorOptions.Add(new Dropdown.OptionData(color.ToString()));
            }

            colorDropdown.AddOptions(colorOptions);

            // 添加物品选项
            List<Dropdown.OptionData> itemOptions = new List<Dropdown.OptionData>();
            foreach (BlockItemEnum item in System.Enum.GetValues(typeof(BlockItemEnum)))
            {
                itemOptions.Add(new Dropdown.OptionData(item.ToString()));
            }

            itemDropdown.AddOptions(itemOptions);

            // 添加下拉菜单事件监听
            colorDropdown.onValueChanged.AddListener(OnColorChanged);
            itemDropdown.onValueChanged.AddListener(OnItemChanged);
        }

        private int _curSelectColor = 0;

        void OnColorChanged(int index)
        {
            _curSelectColor = index;
        }

        private BlockItemEnum _curSelectItem = BlockItemEnum.None;

        void OnItemChanged(int index)
        {
            _curSelectItem = (BlockItemEnum)index;
        }

        public CellUI GetCellByPos(int x, int y)
        {
            if (x >= 8 || y >= 8)
            {
                Debug.LogError("越界了");
                return null;
            }

            try
            {
                return cellUIs[x * gridSize + y];
            }
            catch (Exception e)
            {
                Debug.LogError($"越界了  x:{x} y:{y}");
                throw;
            }
        }

        // 获取当前所有格子数据
        public GridCellData[,] GetGridData()
        {
            var gridData = new GridCellData[gridSize, gridSize];
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    gridData[x, y] = GetCellByPos(x, y).cellData;
                }
            }

            return gridData;
        }

        private bool _colorChange = false;

        private void OnClickColorChange()
        {
            _colorChange = true;
        }

        private void Ray2ChangeColor()
        {
            _results.Clear();
            pointerData.position = Input.mousePosition;
            raycaster.Raycast(pointerData, _results);

            foreach (RaycastResult result in _results)
            {
                var script = result.gameObject.GetComponent<CellUI>();
                if (script != null && script.cellData.bUsed == 0)
                {
                    script.UpdateBackgroundColor(_curSelectColor);
                }
            }
        }

        private bool _itemChange = false;

        private void OnClickItemChange()
        {
            _itemChange = true;
        }

        private void Ray2ChangeItem()
        {
            _results.Clear();
            pointerData.position = Input.mousePosition;
            raycaster.Raycast(pointerData, _results);

            foreach (RaycastResult result in _results)
            {
                var script = result.gameObject.GetComponent<CellUI>();
                if (script != null && script.cellData.bUsed == 0)
                {
                    script.UpdateEffectIcon((int)_curSelectItem);
                }
            }
        }


        private bool flag = false;

        private void Update()
        {
            if (_colorChange)
            {
                if (Input.GetMouseButton(0))
                {
                    flag = true;
                    Ray2ChangeColor();
                }

                if (flag && Input.GetMouseButtonUp(0))
                {
                    flag = false;
                    _colorChange = false;
                }
            }

            if (_itemChange)
            {
                if (Input.GetMouseButton(0))
                {
                    flag = true;
                    Ray2ChangeItem();
                }

                if (flag && Input.GetMouseButtonUp(0))
                {
                    flag = false;
                    _itemChange = false;
                }
            }
        }
    }
}