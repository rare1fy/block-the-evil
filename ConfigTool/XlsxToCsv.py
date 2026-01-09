import openpyxl
import csv
import os
import subprocess
import shutil

xlsxPath = "Xlsx"
csvPath = "Csv"
rootPath = os.path.dirname(os.path.abspath(__file__))

def xlsx_to_csv_filter_row(xlsx_file, sheet_name, output_csv):
    outputCsvPath = os.path.join(rootPath,csvPath,output_csv)
    inputXlsxPath = os.path.join(rootPath,xlsxPath,xlsx_file)
    wb = openpyxl.load_workbook(inputXlsxPath, data_only=True)
    sheet = wb[sheet_name]
    # 获取第一行的数据作为列头
    first_row = [cell.value for cell in sheet[1]]
    # 获取第二行的数据作为列头
    second_row = [cell.value for cell in sheet[2]]
    # 筛选出值为 filter_value 的列索引
    filtered_columns = [idx for idx, value in enumerate(second_row, start=1) if (value == 1 or value == '1')]
    # 获取列头名称（保留第二行中的列名）
    filtered_column_names = [first_row[idx-1] for idx in filtered_columns]
    rows = []
    for row in sheet.iter_rows(min_row=3, values_only=True):  # 从第三行开始
        filtered_row = [row[idx-1] for idx in filtered_columns]  # 获取筛选列的数据
        if(filtered_row[0] is None):
            continue
        rows.append(filtered_row)
    # 将数据写入 CSV
    with open(outputCsvPath, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(filtered_column_names)
        writer.writerows(rows)

    print(f"{xlsx_file} 成功转换为 {output_csv}")

def readFile():
    files = os.listdir(os.path.join(rootPath,xlsxPath))
    for fileName in files:
        if("~$" in fileName):
            continue
        if(fileName.endswith(".xlsx")):
            xlsxName = os.path.splitext(fileName)[0]
            xlsx_to_csv_filter_row(fileName, "Sheet1", xlsxName+ ".csv")

if  __name__ =="__main__":
    path = os.path.join(rootPath,csvPath)
    if(os.path.exists(path)):
        shutil.rmtree(path)
    os.mkdir(path)
    readFile()
    print("----------------------------所有配置表均已经转换完成，执行下一步编译CSV文件--------------------------------------------------------")
    subprocess.run(["python", "Build_PbAndDb.py"])
    exit()