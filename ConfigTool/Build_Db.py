#! /usr/bin/env python
#coding=utf-8

import xlrd
import os
import sys
import shutil
import csv
import copy
import numpy as np
import chardet
import subprocess

xlsPath = "Csv"
protoPath = "output"
protoName = "conf_pb3.proto"
bytesPath = "bytes"
csConfigPath = "CSConfig"

confBytesPath = "\\Assets\\Bundles\\Config\\Tables"
csPath = "\\Assets\\Script\\Config"
csAssetsConfigPath = "\\Assets\\Script\\Config\\AnalyzeConfig"
csName_list =[]
confLanguageBytesPath = "\\Assets\\Resources"


rootPath = os.path.dirname(os.path.abspath(__file__))


########################################LogHelp###########################################
class LogHelp :
    """日志辅助类"""
    _logger = None
    _close_imme = True

    @staticmethod
    def set_close_flag(flag):
        LogHelp._close_imme = flag

    @staticmethod
    def _initlog():
        import logging

        LogHelp._logger = logging.getLogger()
        logfile = 'xls_deploy_tool.log'
        hdlr = logging.FileHandler(logfile)
        formatter = logging.Formatter('%(asctime)s|%(levelname)s|%(lineno)d|%(funcName)s|%(message)s')
        hdlr.setFormatter(formatter)
        LogHelp._logger.addHandler(hdlr)
        LogHelp._logger.setLevel(logging.NOTSET)
        # LogHelp._logger.setLevel(logging.WARNING)

        LogHelp._logger.info("\n\n\n")
        LogHelp._logger.info("logger is inited!")

    @staticmethod
    def get_logger() :
        if LogHelp._logger is None :
            LogHelp._initlog()

        return LogHelp._logger

    @staticmethod
    def close() :
        if LogHelp._close_imme:
            import logging
            if LogHelp._logger is None :
                return
            logging.shutdown()

# log macro
# LOG_DEBUG=LogHelp.get_logger().debug
# LOG_INFO=LogHelp.get_logger().info
# LOG_WARN=LogHelp.get_logger().warn
# LOG_ERROR=LogHelp.get_logger().error


'''
读取文件，第一行为说明，第二行类型，第三行key,第三行后面的是数据
'''

class SheetInterpreter:

    def __init__(self):
        self.outputStr = ""
        self.writerHeader()

    def writerHeader(self):
        self.outputStr = "/*\n"
        self.outputStr+="* @file:   " + protoName + "\n"
        self.outputStr+="* @brief:  这个文件是通过工具自动生成的，建议不要手动修改\n"
        self.outputStr+="*/\n"
        self.outputStr+="\n\n"
        self.outputStr+="syntax = \"proto3\";\n\n"
        self.outputStr+="package pb;\n\n"

    def CSVToProto(self,file):
        name = self.UnderlineToCamel(os.path.splitext(file)[0])
        print("CSVToProto",name)
        path = os.path.join(rootPath,xlsPath,file)
        #读取前三行数据，用于生成pb格式字符串
        firstLine=[]
        secondLine=[]
        thirdLine=[]
        encode = self.get_encoding(path)
        with open(path,'r',encoding=encode) as csvFile:
            reader = csv.reader(csvFile)
            
            for index,rows in enumerate(reader):
                if index > 2:
                    break
                if index == 0:
                    firstLine = rows
                if index == 1:
                    # secondLine = rows
                    thirdLine = rows
                if index == 2:
                    # thirdLine = rows
                    secondLine = rows
        set_list = set(thirdLine)
        if(len(thirdLine) != len(set_list)):
            print("==============================================================行列数不一致,请检查 ======================================================================",file)
            print("=== 程序因错误暂停，按Enter键退出 ===")
            input()
            exit(2)
        self.outputStr += ""
        try:
            if(len(secondLine)<1):
                print("=========================================配表错误，请检查前三行是否有误 ===============================================>>>>> ",file)
                print("=== 程序因错误暂停，按Enter键退出 ===")
                input()
                exit(2)
            self.outputStr+= "message " + name + "\n{\n"
            for index in range(len(secondLine)):
                type = secondLine[index].lower()
                if(type == "int"):
                    self.outputStr += "     int32 %s = %s;                  //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                if(type == "int64"):
                    self.outputStr += "     int64 %s = %s;                  //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                if(type == "string"):
                    self.outputStr += "     string %s = %s;                 //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                if("array" in type):
                    if("int" in type):
                        if("int64" in type):
                            self.outputStr += "     repeated int64 %s = %s;     //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                        else:
                            self.outputStr += "     repeated int32 %s = %s;     //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                    if("string" in type):
                        self.outputStr += "     repeated string %s = %s;     //%s\n"%(thirdLine[index],str(index+1),firstLine[index])


            self.outputStr += "\n}\n\n"
            self.outputStr += "message %sConfig\n{\n	repeated %s data = 1;\n}\n\n"%(name,name)
            with open(os.path.join(rootPath,protoPath,protoName),"w+",encoding='utf-8') as pb:
                pb.write(self.outputStr)
                pb.close()
        
        except Exception as e:
            print(e)
            print("转换protobuf错误 ===>>>>> ",file)
            print("=== 程序因错误暂停，按Enter键退出 ===")
            input()
            exit(2)

    def XLSXToProto(self,file):
        try:
            print("===========>>>>>>>>>>>>>>",file)
            name = self.UnderlineToCamel(os.path.splitext(file)[0])
            xls = xlrd.open_workbook(os.path.join(rootPath,xlsPath,file))
            xls = xls.sheet_by_index(0)
            #读取前三行数据，用于生成pb格式字符串
            firstLine = []
            secondLine = []
            thirdLine = []
            for colindex in range(xls.ncols):
                firstLine.append(xls.cell(0, colindex).value)
                thirdLine.append(xls.cell(1, colindex).value)
                secondLine.append(xls.cell(2, colindex).value)
                
            set_list = set(thirdLine)
            if(len(thirdLine) != len(set_list)):
                print("==============================================================有重复的key,请检查 ======================================================================",file)
                print("=== 程序因错误暂停，按Enter键退出 ===")
                input()
                exit(2)
            self.outputStr += ""
            if(len(secondLine)<1):
                print("配表错误，请检查前三行是否有误 ===>>>>> ",file)
                print("=== 程序因错误暂停，按Enter键退出 ===")
                input()
                exit(2)
            self.outputStr+= "message " + name + "\n{\n"
            for index in range(len(secondLine)):
                type = secondLine[index].lower()
                if(type == "int"):
                    self.outputStr += "     int32 %s = %s;                  //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                if(type == "int64"):
                    self.outputStr += "     int64 %s = %s;                  //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                if(type == "string"):
                    self.outputStr += "     string %s = %s;                 //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                if("array" in type):
                    if("int" in type):
                        if("int64" in type):
                            self.outputStr += "     repeated int64 %s = %s;     //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                        else:
                            self.outputStr += "     repeated int32 %s = %s;     //%s\n"%(thirdLine[index],str(index+1),firstLine[index])
                    if("string" in type):
                        self.outputStr += "     repeated string %s = %s;     //%s\n"%(thirdLine[index],str(index+1),firstLine[index])


            self.outputStr += "\n}\n\n"
            self.outputStr += "message %sConfig\n{\n	repeated %s data = 1;\n}\n\n"%(name,name)
            with open(os.path.join(rootPath,protoPath,protoName),"w+",encoding='utf-8') as pb:
                pb.write(self.outputStr)
                pb.close()

        except Exception as e:
            print(e)
            print("转换protobuf错误 ===>>>>> ",file)
            print("=== 程序因错误暂停，按Enter键退出 ===")
            input()
            exit(2)

    '''
    使用google官方导出cs
    '''
    def DoGenProto(self):
        print("导出python的protobuf") 
        protocEXEPath = os.path.join(rootPath,"protoc/bin/protoc.exe")
        os.chdir(os.path.join(rootPath,protoPath))
        cmd = "%s --python_out=%s %s"%(protocEXEPath,os.path.join(rootPath,protoPath),protoName)
        print(cmd)
        os.system(cmd) 



    def readFile(self):
        files = os.listdir(os.path.join(rootPath,xlsPath))
        for fileNmae in files:
            if("~$" in fileNmae):
                continue
            if(fileNmae.endswith(".csv")):
                self.CSVToProto(fileNmae)
            if("xls" in fileNmae or "xlsx" in fileNmae or "xlsm" in fileNmae):
                self.XLSXToProto(fileNmae)
        self.DoGenProto()

    def UnderlineToCamel(self, underline_format):
        camel_format = ''
        if isinstance(underline_format, str):
            if("_" in underline_format):
                for _s_ in underline_format.split('_'):
                    camel_format += _s_.capitalize()
            else:
                camel_format = underline_format.capitalize()
        return camel_format
    
    #获取文件编码格式
    def get_encoding(self,file):
        with open(file, 'rb') as f:
            return chardet.detect(f.read())['encoding']



class DataParser:
    def __init__(self):
        try:
            self._module_name = os.path.splitext(protoName)[0]+"_pb2"
            sys.path.append(os.path.join(rootPath,protoPath))
            exec('from '+self._module_name + ' import *')
            self._module = sys.modules[self._module_name]
        except BaseException as e :
            print("load module(%s) failed"%(self._module_name))
            print("=== 程序因错误暂停，按Enter键退出 ===")
            input()
            raise


    def CSVToBytesAndCs(self,file):
        try:
            name = self.UnderlineTocamel(os.path.splitext(file)[0])
            path = os.path.join(rootPath,xlsPath,file)
            #读取第二,第三行数据，用于生成pb格式字符串
            secondLine=[]
            thirdLine = []
            firstId = "Id"
            encode = self.get_encoding(path)
            with open(path,'r',encoding=encode) as csvFlie:
                reader = csv.reader(csvFlie)
                for index,rows in enumerate(reader):
                    if (index > 2):
                        break
                    if (index == 1):
                        # secondLine = rows
                        thirdLine = rows 
                        firstId = thirdLine[0]
                    if(index == 2):
                        # thirdLine = rows     
                        secondLine = rows
                csvFlie.close()
            pascal_case_str = self.to_pascal_case(firstId)
            item_array = getattr(self._module, name+'Config')()
            with open(path,'r',encoding=encode) as csvFlie:
                reader = csv.reader(csvFlie)
                for index,rows in enumerate(reader):
                    if index < 3:
                        continue
                    item = item_array.data.add()
                    #  item.__getattribute__(field_name).append(field_value)
                    for index in range(len(secondLine)):
                        type = secondLine[index].lower()
                        field_name = thirdLine[index]
                        if(type == "int"):
                            field_value = rows[index]
                            if(field_value == '' or field_value == 'null'):
                                field_value = 0
                            else:
                                field_value = int(rows[index])
                            item.__setattr__(field_name, field_value)
                        if(type == "int64"):
                            field_value = rows[index]
                            if(field_value == '' or field_value == 'null'):
                                field_value = 0
                            else:
                                field_value = int(rows[index])
                            item.__setattr__(field_name, field_value)
                        if(type == "string"):
                            if(rows[index] == 'null'):
                                field_value = ''
                            else:
                                field_value = rows[index]
                            item.__setattr__(field_name, field_value)
                        if("array" in type):
                            if("int" in type):
                                field_value = rows[index]
                                if(field_value == ''):
                                    field_value = 0
                                    item.__getattribute__(field_name).append(field_value)
                                else:
                                    if('|' in field_value):
                                        strs = field_value.split("|")
                                        for v in strs:
                                            item.__getattribute__(field_name).append(int(v))
                                    else:
                                        print("=======================转换protobuf错误 ===>>>>> ",file)
                                        print("=== 程序因错误暂停，按Enter键退出 ===")
                                        input()
                                        exit(2)
                            if("int64" in type):
                                field_value = rows[index]
                                if(field_value == ''):
                                    field_value = 0
                                    item.__getattribute__(field_name).append(field_value)
                                else:
                                    if('|' in field_value):
                                        strs = field_value.split("|")
                                        for v in strs:
                                            item.__getattribute__(field_name).append(int(v))
                                    else:
                                        print("=============================转换protobuf错误 ===>>>>> ",file)
                                        print("=== 程序因错误暂停，按Enter键退出 ===")
                                        input()
                                        exit(2)
                            if("string" in type):
                                field_value = rows[index]
                                if(field_value == ''):
                                    item.__getattribute__(field_name).append(field_value)
                                else:
                                    if('|' in field_value):
                                        strs = field_value.split("|")
                                        for v in strs:
                                            item.__getattribute__(field_name).append(v)
                                    else:
                                        print("转换protobuf错误 ===>>>>> ",file)
                                        print("=== 程序因错误暂停，按Enter键退出 ===")
                                        input()
                                        exit(2)     
                csvFlie.close()
                data = item_array.SerializeToString()
                self._WriteData2File(data,name)
                if name != "LanguageConfig":
                   self.generate_cs_code(name,pascal_case_str)
                   csName_list.append("Config_"+ name)
                self._WriteReadableData2File(str(item_array),name)
        except Exception as e:
            print(f"CSVToBytesAndCs 处理 {file} 时发生错误: {e}")
            import traceback
            traceback.print_exc()
            print("=== 程序因错误暂停，按Enter键退出 ===")
            input()
            exit(1)

    def _WriteData2File(self, data,name) :
        file_name =  os.path.join(rootPath,bytesPath,name+ ".bytes")  
        file = open(file_name, 'wb+')
        file.write(data)
        file.close()

    def _WriteReadableData2File(self, data,name) :
        file_name =  os.path.join(rootPath,bytesPath,name+ ".txt")  
        file = open(file_name, 'wb+')
        file.write(data.encode("UTF-8"))
        file.close()

    def generate_cs_code(self, class_name, id_field):
        cs_template = f"""using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_{class_name} : ConfigBase
{{
    public Dictionary<int, Pb.{class_name}> m_{class_name}Dic = new();

    public override void LoadConfigStep()
    {{
        Config.instance._configLoadData.TryAdd("{class_name}", ReadConfig);
    }}

    private void ReadConfig(Stream data)
    {{
        var tempConfig = {class_name}Config.Descriptor.Parser.ParseFrom(data) as {class_name}Config;
        if (!ReferenceEquals(tempConfig, null))
        {{
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_{class_name}Dic, null))
            {{
                m_{class_name}Dic = new();
            }}

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {{
                var item = dataList[i];
                m_{class_name}Dic.TryAdd(item.{id_field}, item);
            }}
        }}
    }}

    public Pb.{class_name} GetConfigById(int Id)
    {{
        if (m_{class_name}Dic.TryGetValue(Id, out var config))
        {{
            return config;
        }}
        return null;
    }}
}}
"""
        file_name =  os.path.join(rootPath,csConfigPath,"Config_"+ class_name+ ".cs")  
        file = open(file_name, 'wb+')
        file.write(cs_template.encode("UTF-8"))
        file.close()

    def generate_cs_config(self):
        _str_01 = "using System;\nusing System.Collections;\nusing System.Collections.Generic;\nusing Framework;\nusing System.IO;\nusing Cysharp.Threading.Tasks;\nusing UnityEngine;\n"
        _str_02 = "\npublic class Config : Singleton<Config>\n{\n\tprivate static readonly HashSet<ConfigBase> ConfigList = new()\n\t{\n"
        _str_03 = ""
        for line in csName_list:
            _str_03 += '\t\t'+ "new\t" + line+ "(),\n"
        _str_03 += '\t};'
        _str_04 = """  
\n\tpublic static T GetConfig<T>() where T : ConfigBase
    {
        foreach (var item in ConfigList)
        {
            if (item is T)
            {
                var asset = item as T;
                return asset;
            }
        }
        Debug.LogError($"解析的配置表名字不存在!!!!! {typeof(T)}");
        return null;
    }

    public Dictionary<string, Action<Stream>> _configLoadData = new();

    public IEnumerator LoadConfigStep(Action callBack = null)
    {
        foreach (var item in ConfigList)
        {
            item.LoadConfigStep();
        }
        var lit = _configLoadData;
        foreach (var configList in lit)
        {
            var mStrName = configList.Key;
            var mAction = configList.Value;
            var isComplect = false;
            ResourceManagerNew.instance.LoadConfigAsync(mStrName, delegate (byte[] bytes)
            {
                if (!ReferenceEquals(bytes,null))
                {
                    mAction(new MemoryStream(bytes));
                }
                else
                {
                    Debug.LogError($"===Load Config Fail=== Config Name:{mStrName}");
                }
                isComplect = true;
            });
            
            while (!isComplect)
            {
                yield return null;
            }
        }

        if (!ReferenceEquals(callBack, null))
        {
            callBack();
        }
    }

    public async UniTask LoadConfigStepAsync()
    {
	    foreach (var item in ConfigList)
	    {
		    item.LoadConfigStep();
	    }
	    
	    var loadTasks = _configLoadData.Select(async configPair =>
	    {
		    var configName = configPair.Key;
		    var action = configPair.Value;
        
		    byte[] bytes = await ResourceManagerNew.instance.LoadConfigAsync(configName);
		    
		    if (bytes != null)
		    {
			    action(new MemoryStream(bytes));
		    }
		    else
		    {
			    Debug.LogError($"===Load Config Fail=== Config Name:{configName}");
		    }
	    });
	    
	    await UniTask.WhenAll(loadTasks);
    }

    public override void Dispose()
    {
        _configLoadData = null;
        ConfigList.Clear();
    }
}
"""
        cs_config = _str_01 + _str_02 + _str_03 + _str_04
        file_name =  os.path.join(rootPath,csConfigPath,"Config.cs")  
        file = open(file_name, 'wb+')
        file.write(cs_config.encode("UTF-8"))
        file.close()
        print("生成Config.cs成功!!!!!!!!!")

    def XLSXToBytes(self,file):

        path = os.path.join(rootPath,xlsPath,file)
        print(path)
        name = self.UnderlineTocamel(os.path.splitext(file)[0])
        xls = xlrd.open_workbook(os.path.join(rootPath,xlsPath,file))
        xls = xls.sheet_by_index(0)
        #读取第二，三行数据，用于生成pb格式字符串
        secondLine = []
        thirdLine = []
        
        for colindex in range(xls.ncols):
            thirdLine.append(xls.cell(1, colindex).value)
            secondLine.append(xls.cell(2, colindex).value)
            
        print(secondLine)
        print(thirdLine)
        item_array = getattr(self._module, name+'Config')()
        print("=======================>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
        print(xls.nrows)
        for index in range(xls.nrows):
            if(index < 3):
                continue
            print(xls.row_values(index))
            print("\n")
            item = item_array.data.add()
            dataList = xls.row_values(index)
            for index in range(len(secondLine)):
                type = secondLine[index].lower()
                field_name = thirdLine[index]
                if(type == "int"):
                    field_value = dataList[index]
                    if(field_value == ''):
                        field_value = 0
                    else:
                        field_value = int(dataList[index])
                    item.__setattr__(field_name, field_value)
                if(type == "string"):
                    field_value = dataList[index]
                    item.__setattr__(field_name, field_value)
                if("array" in type):
                    if("int" in type):
                        field_value = dataList[index]
                        if(field_value == ''):
                            field_value = 0
                            item.__getattribute__(field_name).append(field_value)
                        else:
                            if('|' in field_value):
                                strs = field_value.split("|")
                                for v in strs:
                                    item.__getattribute__(field_name).append(int(v))
                            else:
                                print("转换protobuf错误 ===>>>>> ",file)
                                print("=== 程序因错误暂停，按Enter键退出 ===")
                                input()
                                exit(2)
                    if("string" in type):
                        field_value = dataList[index]
                        if(field_value == ''):
                            item.__getattribute__(field_name).append(field_value)
                        else:
                            if('|' in field_value):
                                strs = field_value.split("|")
                                for v in strs:
                                    item.__getattribute__(field_name).append(v)
                            else:
                                print("转换protobuf错误 ===>>>>> ",file)
                                print("=== 程序因错误暂停，按Enter键退出 ===")
                                input()
                                exit(2)  

        data = item_array.SerializeToString()
        self._WriteData2File(data,name)
        
        self._WriteReadableData2File(str(item_array),name)   


    def readFile(self):
        os.chdir(rootPath)
        files = os.listdir(os.path.join(rootPath,xlsPath))
        for fileNmae in files:
            if("~$" in fileNmae):
                continue
            if(fileNmae.endswith(".csv")):
                self.CSVToBytesAndCs(fileNmae)
            if("xls" in fileNmae or "xlsx" in fileNmae or "xlsm" in fileNmae):
                self.XLSXToBytes(fileNmae)
        self.generate_cs_config()
    def UnderlineTocamel(self, underline_format):
        camel_format = ''
        if isinstance(underline_format, str):
            if("_" in underline_format):
                for _s_ in underline_format.split('_'):
                    camel_format += _s_.capitalize()
            else:
                camel_format = underline_format
        return camel_format
    
    #获取文件编码格式
    def get_encoding(self,file):
        with open(file, 'rb') as f:
            return chardet.detect(f.read())['encoding']

    # 得到驼峰式命名  
    def to_pascal_case(self,snake_str):
       # 将字符串按下划线分割
       components = snake_str.split('_')
       # 每个单词首字母大写，其他字母保持原样
       return ''.join([component[0].upper() + component[1:] if component else '' for component in components])
    
'''
protobuf-net 插件导出cs文件
'''
def DoProtoNetCS():
    print("导出CS的protobuf") 
    protocEXEPath = os.path.join(rootPath,"protogen/protogen.exe")
    os.chdir(os.path.join(rootPath,protoPath))
    cmd = "%s conf_pb3.proto --csharp_out=\"%s\""%(protocEXEPath,os.path.join(rootPath,protoPath))
    print(cmd)
    os.system(cmd) 
    if(os.path.exists("conf_pb3.cs")):
        os.rename("conf_pb3.cs","ConfigProto.cs")

def DoProtoCS():
    print("导出CS的protobuf") 
    protocEXEPath = os.path.join(rootPath,"protoc/bin/protoc.exe")
    os.chdir(os.path.join(rootPath,protoPath))
    cmd = "%s --csharp_out=%s %s"%(protocEXEPath,os.path.join(rootPath,protoPath),protoName)
    print(cmd)
    os.system(cmd) 
    if(os.path.exists("ConfPb3.cs")):
        os.rename("ConfPb3.cs","ConfigProto.cs")
    
    

def copyFile():
    try:
        print("开始复制bytes文件到工程")
        os.chdir(rootPath)
        parent_dir = os.path.dirname(rootPath)
        print(f"父目录: {parent_dir}")
        
        # 复制bytes文件
        path = parent_dir + confBytesPath
        print(f"目标bytes路径: {path}")
        
        if not os.path.exists(parent_dir):
            print(f"错误: 父目录不存在: {parent_dir}")
            print("=== 程序因错误暂停，按Enter键退出 ===")
            input()
            return
            
        if os.path.exists(path):
            print("删除已存在的bytes目录")
            shutil.rmtree(path)
        
        print("创建新的bytes目录")
        os.makedirs(path, exist_ok=True)
        
        bytePath = os.path.join(rootPath, bytesPath)
        print(f"源bytes路径: {bytePath}")
        
        if not os.path.exists(bytePath):
            print(f"错误: 源bytes路径不存在: {bytePath}")
            print("=== 程序因错误暂停，按Enter键退出 ===")
            input()
            return
            
        files = os.listdir(bytePath)
        print(f"找到 {len(files)} 个文件在源目录")
        
        LanguagePath = parent_dir + confLanguageBytesPath
        print(f"语言文件路径: {LanguagePath}")
        
        for v in files:
            if v.endswith(".bytes"):
                source_file = os.path.join(bytePath, v)
                if v == "LanguageConfig.bytes":
                    print(f"复制语言文件: {source_file} -> {LanguagePath}")
                    os.makedirs(LanguagePath, exist_ok=True)
                    shutil.copy(source_file, LanguagePath)
                else:
                    print(f"复制配置文件: {source_file} -> {path}")
                    shutil.copy(source_file, path)

        # 复制CS文件
        print("复制cs文件到工程")
        path = parent_dir + csPath
        print(f"CS目标路径: {path}")
        os.makedirs(path, exist_ok=True)
        
        csToPath = os.path.join(rootPath, protoPath)
        source_cs = os.path.join(csToPath, "ConfigProto.cs")
        
        if os.path.exists(source_cs):
            target_cs = os.path.join(path, "ConfigProto.cs")
            print(f"复制CS文件: {source_cs} -> {target_cs}")
            shutil.copy(source_cs, target_cs)
        else:
            print(f"警告: CS源文件不存在: {source_cs}")

        # 复制配置解析文件
        print("复制解析配置表文件到工程")
        parent_dir = os.path.dirname(rootPath)
        path = parent_dir + csAssetsConfigPath
        print(f"配置解析目标路径: {path}")
        
        if os.path.exists(path):
            print("删除已存在的配置解析目录")
            shutil.rmtree(path)
        
        os.makedirs(path, exist_ok=True)
        
        source_config_path = os.path.join(rootPath, csConfigPath)
        if not os.path.exists(source_config_path):
            print(f"错误: 源配置路径不存在: {source_config_path}")
            print("=== 程序因错误暂停，按Enter键退出 ===")
            input()
            return
            
        files = os.listdir(source_config_path)
        print(f"找到 {len(files)} 个配置文件")
        
        for v in files:
            if v.endswith(".cs"):
                source_file = os.path.join(source_config_path, v)
                target_file = os.path.join(path, v)
                print(f"复制配置文件: {source_file} -> {target_file}")
                shutil.copy(source_file, path)
                
        print("复制文件完成，编译结束")
        
    except Exception as e:
        print(f"复制文件时发生错误: {e}")
        import traceback
        traceback.print_exc()
        print("=== 程序因错误暂停，按Enter键退出 ===")
        input()
        exit(1)


if  __name__ =="__main__":
    try:
        print("开始执行构建流程...")
        
        subprocess.run(["python", "Build_Font.py"])
        
        path = os.path.join(rootPath,protoPath)
        if(os.path.exists(path)):
            shutil.rmtree(path)
        os.mkdir(path)
        
        if(os.path.exists(os.path.join(rootPath,bytesPath))):
            shutil.rmtree(os.path.join(rootPath,bytesPath))
        os.mkdir(os.path.join(rootPath,bytesPath))
        
        if(os.path.exists(os.path.join(rootPath,csConfigPath))):
           shutil.rmtree(os.path.join(rootPath,csConfigPath))
        os.mkdir(os.path.join(rootPath,csConfigPath))
        
        #导出protobuf
        print("开始导出protobuf...")
        tool = SheetInterpreter()
        tool.readFile()

        # 转为bytes
        print("开始转换bytes...")
        parser = DataParser()
        parser.readFile()
        
        #生成CS文件
        print("生成CS文件...")
        DoProtoCS()

        #复制文件
        print("开始复制文件...")
        copyFile()

        print("所有步骤完成!")
        
    except Exception as e:
        print(f"构建过程中发生错误: {e}")
        import traceback
        traceback.print_exc()
        print("\n=== 程序因错误暂停，按Enter键退出 ===")
        input()
        exit(1)