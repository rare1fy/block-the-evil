import openpyxl 
import os
import sys

rootPath = os.path.dirname(os.path.abspath(__file__))

CHUNK_SIZE = 100  # 每行字符数

def extract_unique_chinese():
    # 获取当前脚本所在目录的路径
    current_dir = os.path.dirname(os.path.abspath(__file__))
    
    # 拼接得到目标文件的完整路径
    file_path = os.path.join(current_dir, "Xlsx", "language_Config.xlsx")
        
    if(not os.path.exists(file_path)):
       print("Not Find:", file_path)
       
    wb = openpyxl.load_workbook(file_path, data_only=True)
    sheet = wb["Sheet1"] 
    unique_chars = []
    
    #解析并且去重     
    for row in sheet.iter_rows(5,None, 2, 2):
        cell = row[0]
        if cell.value is None:
           continue
        for char in cell.value:
           if '\u4e00' <= char <= '\u9fff':
             if char not in unique_chars:
               unique_chars.append(char)
    #写入的中文字符路径          
    output_path = os.path.join(current_dir, "FontTool", "src", "FontExtract", "ChineseOutPut.txt")
    with open(output_path, "w", encoding="utf-8") as outfile:
        buffer = []
        for i, char in enumerate(unique_chars):
            buffer.append(char)
            if (i + 1) % CHUNK_SIZE == 0:
                outfile.write("".join(buffer) + "\n")
                buffer = []
        if buffer:
            outfile.write("".join(buffer) + "\n")            
    
if __name__ == "__main__":
    extract_unique_chinese()
    
    python_exe = sys.executable
    cmd = os.path.join(rootPath, "FontTool", "creatfont.cmd")
    cmd = cmd + " " + python_exe
    # 执行命令
    exit_code = os.system(cmd)
    if exit_code == 0:
        print("----------------------------执行生成字体成功，执行下一步打DB--------------------------------------------------------")
    else:
        print(f"执行生成字体失败，退出码：{exit_code}")
