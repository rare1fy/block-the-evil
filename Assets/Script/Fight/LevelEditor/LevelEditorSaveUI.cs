using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace LevelEditor
{
    public class LevelEditorSaveUI : MonoBehaviour
    {
        private string defaultFolder = "Bundles/Config/LevelData"; // 保存路径文件夹
        [Header("References")] 
        public LevelEditorManager editorManager;
        
        public MaskPanel maskPanel;
        public InputField fileNameInput;
        public Button saveButton;
        public Button loadButton;
        public Button clearButton;

        private void Start()
        {
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveClicked);

            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadClicked);

            if (clearButton != null)
                clearButton.onClick.AddListener(OnClearClicked);

            // 确保保存目录存在
            string folderPath = Path.Combine(Application.dataPath, defaultFolder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        void OnSaveClicked()
        {
            if (editorManager == null || fileNameInput == null) return;

            string fileName = fileNameInput.text.Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarning("File name is empty!");
                return;
            }

            string path = Path.Combine(Application.dataPath, defaultFolder, fileName + ".json");
            SaveGridData(path);
        }

        void OnLoadClicked()
        {
            if (editorManager == null || fileNameInput == null) return;

            string fileName = fileNameInput.text.Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarning("File name is empty!");
                return;
            }

            string path = Path.Combine(Application.dataPath, defaultFolder, fileName + ".json");
            LoadGridData(path);
        }

        // ===== 保存 =====
        void SaveGridData(string filePath)
        {
            var gridData = editorManager.GetGridData();
            var saveData = new GridSaveData(editorManager.gridSize, gridData, maskPanel.maskDatas);
            var json = JsonUtility.ToJson(saveData, true);

            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log($"Grid saved to: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Save failed: {ex.Message}");
            }
        }

        // ===== 加载 =====
        void LoadGridData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("No save file found at " + filePath);
                return;
            }
            
            var json = File.ReadAllText(filePath);
            var saveData = JsonUtility.FromJson<GridSaveData>(json);
            // 更新到 LevelEditorManager
            for (var x = 0; x < saveData.size; x++)
            {
                for (var y = 0; y < saveData.size; y++)
                {
                    var data = saveData.cells[x * saveData.size + y];
                    var cell = editorManager.GetCellByPos(x, y);
                    cell.UpdateVisual(data);
                }
            }
            maskPanel.LoadMaskData(saveData.masks);
        }

        void OnClearClicked()
        {
            if (editorManager == null) return;

            var cellList = editorManager.CellUIs;
            foreach (var cellUI in cellList)
            {
                cellUI.UpdateVisual(new GridCellData(0, 0));
            }

            Debug.Log("Grid cleared!");
        }
    }
}