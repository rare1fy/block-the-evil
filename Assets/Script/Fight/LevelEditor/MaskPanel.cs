using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LevelEditor
{
    public class MaskPanel : MonoBehaviour
    {
        public InputField InputField_Length;
        public InputField InputField_Width;

        public Camera mCamera;
        public LevelEditorManager editorManager;
        public Button CreateBtn;
        public Button ClearBtn;
        public GameObject Mask;
        public GraphicRaycaster raycaster;
        public Dropdown maskTypeDropdown;

        private EventSystem eventSystem;
        private PointerEventData pointerData;
        private List<RaycastResult> _results = new List<RaycastResult>();

        public List<Sprite> maskSprites;

        [HideInInspector] public List<MaskData> maskDatas = new List<MaskData>();

        /// <summary>
        /// 添加遮罩
        /// </summary>
        /// <param name="maskDataList"></param>
        public void LoadMaskData(List<MaskData> maskDataList)
        {
            maskDatas.Clear();
            maskDatas.AddRange(maskDataList);
            RefreshMaskInGrid();
        }


        private void Start()
        {
            lengthCount = 0;
            widthCount = 0;
            eventSystem = EventSystem.current;
            pointerData = new PointerEventData(eventSystem);

            if (CreateBtn != null)
                CreateBtn.onClick.AddListener(OnCreateMask);
            if (ClearBtn != null)
                ClearBtn.onClick.AddListener(OnClearAll);

            SetupDropdowns();
            RefreshMaskInGrid();
        }

        private bool _bCreateMask;
        private GameObject tempMask;
        private int lengthCount;
        private int widthCount;

        private void OnCreateMask()
        {
            lengthCount = int.Parse(InputField_Length.text);
            widthCount = int.Parse(InputField_Width.text);
            if (lengthCount > 0 && widthCount > 0)
            {
                _bCreateMask = true;
                tempMask = Instantiate(Mask, transform.parent);
                var rect = tempMask.transform as RectTransform;
                var length = 80 * lengthCount;
                var width = 80 * widthCount;
                rect.sizeDelta = new Vector2(length, width);
                tempMask.GetComponent<Image>().sprite = maskSprites[(int)_curMaskType];
            }
        }

        void SetupDropdowns()
        {
            // 清空现有选项
            maskTypeDropdown.ClearOptions();
            // 添加颜色选项
            var colorOptions = new List<Dropdown.OptionData>();
            foreach (MaskType type in Enum.GetValues(typeof(MaskType)))
            {
                colorOptions.Add(new Dropdown.OptionData(type.ToString()));
            }

            maskTypeDropdown.AddOptions(colorOptions);

            // 添加下拉菜单事件监听
            maskTypeDropdown.onValueChanged.AddListener(OnChanged);
        }

        private MaskType _curMaskType = MaskType.金币;

        void OnChanged(int index)
        {
            _curMaskType = (MaskType)index;
        }

        private void FixedUpdate()
        {
            if (_bCreateMask)
            {
                if (Input.GetMouseButton(0))
                {
                    Ray2SetMask();
                }

                if (Input.GetMouseButtonUp(0) && _targetUI != null)
                {
                    SetMask(_targetUI.gridX, _targetUI.gridY);

                    _bCreateMask = false;
                    Destroy(tempMask);
                    tempMask = null;
                    _targetUI = null;
                }
            }
        }

        private CellUI _targetUI;

        private void Ray2SetMask()
        {
            _results.Clear();
            pointerData.position = Input.mousePosition;
            var pos = mCamera.ScreenToWorldPoint(Input.mousePosition);
            tempMask.transform.position = pos;

            raycaster.Raycast(pointerData, _results);
            foreach (var result in _results)
            {
                var script = result.gameObject.GetComponent<CellUI>();
                if (script != null)
                {
                    if (script.cellData.bUsed == 0 && CheckCanPut(script.gridX, script.gridY))
                    {
                        if (_targetUI != null)
                            _targetUI.HideMask();

                        script.ShowTempMask(lengthCount, widthCount, maskSprites[(int)_curMaskType]);
                        _targetUI = script;
                    }
                }
            }
        }

        private bool CheckCanPut(int x, int y)
        {
            var posList = new List<Vector2Int>();

            for (int i = 0; i <widthCount ; i++)
            {
                for (int j = 0; j < lengthCount; j++)
                {
                    posList.Add(new Vector2Int(i, j));
                }
            }

            for (int i = 0; i < posList.Count; i++)
            {
                var pos = posList[i] + new Vector2Int(x, y);
                if (pos.x >= 8 || pos.y >= 8) //超范围了
                    return false;

                var cell = editorManager.GetCellByPos(pos.x, pos.y);
                if (cell.cellData.color != 0) //有颜色就不能放
                {
                    return false;
                }
            }

            return true;
        }

        private void SetMask(int x, int y)
        {
            var maskData = new MaskData(x, y, lengthCount, widthCount, (int)_curMaskType);
            maskDatas.Add(maskData);

            RefreshMaskInGrid();
        }

        private void OnClearAll()
        {
            if (tempMask != null)
            {
                Destroy(tempMask);
                
                tempMask = null;
            }

            maskDatas.Clear();

            foreach (var cellUI in editorManager.CellUIs)
            {
                cellUI.cellData.bUsed = 0;
                cellUI.HideMask();
            }
        }

        private void RefreshMaskInGrid()
        {
            if (tempMask != null)
            {
                Destroy(tempMask);
                tempMask = null;
            }

            foreach (var cellUI in editorManager.CellUIs)
            {
                cellUI.cellData.bUsed = 0;
                cellUI.HideMask();
            }

            foreach (var maskData in maskDatas)
            {
                var main = editorManager.GetCellByPos(maskData.PosX, maskData.PosY);
                main.ShowMask(maskData.Length, maskData.Width, maskSprites[maskData.Type]);
                
                for (int i = 0; i < maskData.Width; i++)
                {
                    for (int j = 0; j < maskData.Length; j++)
                    {
                        var posX = maskData.PosX + i;
                        var posY = maskData.PosY + j;
                        var cell = editorManager.GetCellByPos(posX, posY);
                        
                        cell.cellData.bUsed = 1;
                    }
                }
            }
        }
    }
}